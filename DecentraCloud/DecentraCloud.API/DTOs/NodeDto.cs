namespace DecentraCloud.API.DTOs
{
    public class NodeDto
    {
        public string Id { get; set; }
        public string NodeName { get; set; }
        public string Endpoint { get; set; }
        public bool IsOnline { get; set; }
        public long Storage { get; set; }
    }
}
