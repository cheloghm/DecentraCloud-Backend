namespace DecentraCloud.API.DTOs
{
    public class NodeUpdateDto
    {
        public int Storage { get; set; }
        public string NodeName { get; set; }
        public string Password { get; set; }
        // Add other properties that can be updated
    }
}
