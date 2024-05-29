using System.Text.Json.Serialization;

namespace BaseCrud.Dtos
{
    public class ResponseUploadMediaDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("s3Key")]
        public string? S3Key { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }
}
