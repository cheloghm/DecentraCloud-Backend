namespace DecentraCloud.API.DTOs
{
    public class UserDetailsDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public UserSettingsDto Settings { get; set; }
        public long AllocatedStorage { get; set; }
        public long UsedStorage { get; set; }
    }
}
