namespace DecentraCloud.API.DTOs
{
    public class FileUploadDto
    {
        public string UserId { get; set; }
        public string Filename { get; set; }
        public byte[] Data { get; set; }
        public string NodeId { get; set; }
    }
}
