namespace DecentraCloud.API.DTOs
{
    public class ReplicationRequestDto
    {
        public string SourceNodeId { get; set; }
        public string DestinationNodeId { get; set; }
        public string Filename { get; set; }
        public string Data { get; set; }
        public string Checksum { get; set; }
    }
}
