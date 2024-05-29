using BaseCrud.Dtos;
using BaseCrud.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BaseCrud.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class ValueController : ControllerBase
    {
        private readonly IFileService _minioClient;

        public ValueController(IFileService minioClient)
        {
            _minioClient = minioClient;
        }

        [HttpPost("add")]
        public async Task<ResponseUploadMediaDto> Upload(IFormFile input)
        {
            return await _minioClient.UploadFileAsync(input);
        }

        [HttpPost("move")]
        public async Task MoveAsync(string s3key)
        {
            await _minioClient.MoveAsync(s3key);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string s3key)
        {
            var result = await _minioClient.DownloadFileAsync(s3key);
            return File(result.Stream, result.ContentType, result.FileName);
        }

        [HttpDelete]
        public async Task Delete(string s3key)
        {
            await _minioClient.DeleteFileAsync(s3key);
        }
    }
}
