namespace BaseCrud.Dtos
{
    public class ExportFileDto
    {
        public Stream Stream { get; set; } = null!;
        public string? FileName { get; set; }
        public string ContentType { get; set; } = null!;
    }
}
