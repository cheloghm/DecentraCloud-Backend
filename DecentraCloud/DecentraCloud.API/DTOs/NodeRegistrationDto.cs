namespace DecentraCloud.API.DTOs
{
    public class NodeRegistrationDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int Storage { get; set; }
        public string NodeName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
}
