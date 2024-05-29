using BaseCrud.Dtos;

namespace BaseCrud.Interfaces
{
    public interface IFileService
    {
        Task<ResponseUploadMediaDto> UploadFileAsync(IFormFile input);
        Task<ExportFileDto> DownloadFileAsync(string s3key);
        Task DeleteFileAsync(string s3key);
        Task<ResponseUploadMediaDto> MoveAsync(string s3key);
    }
}
