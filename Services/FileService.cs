using Amazon.S3;
using Amazon.S3.Model;
using Azure;
using BaseCrud.Dtos;
using BaseCrud.Interfaces;
using BaseCrud.Migrations;
using BaseCrud.Utils;
using System.Drawing;

namespace BaseCrud.Services
{
    public class FileService : IFileService
    {
        private IAmazonS3 _s3Client;

        public FileService()
        {
            _s3Client = new AmazonS3Client(
                "user1",
                "1234@qwer",
                new AmazonS3Config
                {
                    ServiceURL = "http://localhost:9000",
                    ForcePathStyle = true // Yêu cầu để làm việc với MinIO,
                }
            );
            ConfigureLifecyclePolicyAsync().GetAwaiter().GetResult();
        }

        public async Task ConfigureLifecyclePolicyAsync()
        {
            var lifecycleConfiguration = new LifecycleConfiguration
            {
                Rules = new List<LifecycleRule>
                {
                    new LifecycleRule
                    {
                        Id = "DeleteOldObjects",
                        Filter = new LifecycleFilter
                        {
                            LifecycleFilterPredicate = new LifecyclePrefixPredicate
                            {
                                Prefix = "temp"
                            }
                        },
                        Status = LifecycleRuleStatus.Enabled,
                        Expiration = new LifecycleRuleExpiration
                        {
                           // Số ngày sau đó đối tượng sẽ bị xóa
                           Days = 1
                        }
                    }
                }
            };

            var putLifecycleRequest = new PutLifecycleConfigurationRequest
            {
                BucketName = "ehuce-bucket",
                Configuration = lifecycleConfiguration
            };

            try
            {
                await _s3Client.PutLifecycleConfigurationAsync(putLifecycleRequest);
                Console.WriteLine("Lifecycle policy đã được thiết lập thành công.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    $"Error encountered on server. Message:'{e.Message}' when writing an object"
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Unknown encountered on server. Message:'{e.Message}' when writing an object"
                );
            }
        }

        public string GenerateObjectName()
        {
            DateTime currentDate = DateTime.Now;
            string year = currentDate.Year.ToString();
            string month = currentDate.Month.ToString("00"); // Đảm bảo định dạng 2 chữ số
            string day = currentDate.Day.ToString("00"); // Đảm bảo định dạng 2 chữ số

            // Tạo object name theo định dạng: yyyy/MM/dd
            string objectName = $"{year}/{month}/{day}";

            return objectName;
        }

        private async Task DoesS3BucketExistAsync(string bucketName)
        {
            var result = false;
            var response = await _s3Client.ListBucketsAsync();
            foreach (var bucket in response.Buckets)
            {
                if (bucket.BucketName == bucketName)
                {
                    result = true;
                }
            }
            if (!result)
            {
                try
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    var responseCreateBucket = await _s3Client.PutBucketAsync(putBucketRequest);
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine(
                        "Error encountered on server. Message:'{0}' when writing an object",
                        e.Message
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "Unknown encountered on server. Message:'{0}' when writing an object",
                        e.Message
                    );
                }
            }
        }

        private string GetFileNameFromPath(string path)
        {
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                return path;
            }
            else
            {
                return path.Substring(lastSlashIndex + 1);
            }
        }

        private string GenerateNewMoveS3Key(string tempS3Key)
        {
            // Define the marker string
            string marker = "temp/";

            // Find the index of the marker
            int index = tempS3Key.IndexOf(marker);

            // Check if the marker exists in the file path
            if (index != -1)
            {
                // Extract the substring starting just after the marker
                return tempS3Key.Substring(index + marker.Length);
            }
            else
            {
                // If the marker is not found, return an empty string or handle as needed
                return string.Empty;
            }
        }

        private ResponseUploadMediaDto MapResponse(string s3Key, string contentType, string fileName)
        {
            var result = new ResponseUploadMediaDto();
            result.Uri = $"ehuce-bucket/{s3Key}";
            result.S3Key = s3Key;
            result.MimeType = contentType;
            result.Name = fileName;
            return result;
        }

        public async Task<ResponseUploadMediaDto> UploadFileAsync(IFormFile input)
        {
            string bucketName = "ehuce-bucket";
            string fileName = input.FileName;
            var contentType = input.ContentType;
            // Loại bỏ phần mở rộng của file
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            fileName = StringUtils
                .RemoveSign4VietnameseString(nameWithoutExtension)
                .ToLower()
                .Replace(" ", string.Empty);

            string s3Key =
                $"temp/{GenerateObjectName()}/{fileName}-{DateTime.Now.ToFileTime()}{Path.GetExtension(input.FileName)}";

            await DoesS3BucketExistAsync(bucketName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await input.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = memoryStream,
                };
                var response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("Tệp đã được tải lên thành công, ETag: " + response.ETag);
            }
            return MapResponse(s3Key, contentType, fileName);
        }

        public async Task<ExportFileDto> DownloadFileAsync(string s3Key)
        {
            var result = new ExportFileDto();
            await DoesS3BucketExistAsync("ehuce-bucket");
            try
            {
                var getRequest = new GetObjectRequest { BucketName = "ehuce-bucket", Key = s3Key };

                var response = await _s3Client.GetObjectAsync(getRequest);

                var fileName = GetFileNameFromPath(response.Key);
                var responseStream = response.ResponseStream;

                // Đọc metadata và headers
                string title = response.Metadata["x-amz-meta-title"];
                string contentType = response.Headers["Content-Type"];
                Console.WriteLine(
                    "Tệp tải xuống, Tiêu đề: {0}, Loại nội dung: {1}",
                    title,
                    contentType
                );

                result.Stream = responseStream;
                result.ContentType = contentType;
                result.FileName = fileName;
                return result;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Lỗi gặp phải khi tải xuống tệp: " + e.Message);
                throw new NotImplementedException();
            }
        }

        public async Task DeleteFileAsync(string s3key)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = "ehuce-bucket",
                    Key = s3key
                };

                Console.WriteLine($"Deleting object {s3key} from bucket ehuce.s3bucket");
                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                Console.WriteLine("Object deleted successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown error encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
        }

        public async Task<ResponseUploadMediaDto> MoveAsync(string s3key)
        {
            var newS3Key = GenerateNewMoveS3Key(s3key);
            var title = string.Empty;
            var contentType = string.Empty;
            try
            {
                var getRequest = new GetObjectRequest { BucketName = "ehuce-bucket", Key = s3key };

                var response = await _s3Client.GetObjectAsync(getRequest);
                // Đọc metadata và headers
                title = response.Metadata["x-amz-meta-title"];
                contentType = response.Headers["Content-Type"];

                // Copy the object from the source to the destination
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = "ehuce-bucket",
                    SourceKey = s3key,
                    DestinationBucket = "ehuce-bucket",
                    DestinationKey = newS3Key
                };

                CopyObjectResponse copyResponse = await _s3Client.CopyObjectAsync(copyRequest);
                Console.WriteLine($"File copied to {newS3Key} successfully.");

                // Delete the original object
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = "ehuce-bucket",
                    Key = s3key
                };

                DeleteObjectResponse deleteResponse = await _s3Client.DeleteObjectAsync(deleteRequest);
                Console.WriteLine($"File {s3key} deleted successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when moving file", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error encountered on server. Message:'{0}' when moving file", e.Message);
            }
            return MapResponse(newS3Key, contentType, title);
        }
    }
}
