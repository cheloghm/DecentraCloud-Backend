using DecentraCloud.API.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DecentraCloud.API.Models
{
    public class User : IEntityWithId
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("settings")]
        public UserSettings Settings { get; set; }

        [BsonElement("token")]
        public string Token { get; set; }

        [BsonElement("allocatedStorage")]
        public long AllocatedStorage { get; set; } = 1_073_741_824; // 1 GB in bytes

        [BsonElement("usedStorage")]
        public long UsedStorage { get; set; } = 0; // Initial storage used is 0
    }
}
