using DecentraCloud.API.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DecentraCloud.API.Models
{
    public class Node : IEntityWithId
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public int Storage { get; set; }
        public long Uptime { get; set; }
        public long Downtime { get; set; }
        public StorageStats StorageStats { get; set; }
        public string CauseOfDowntime { get; set; }
        public string Token { get; set; }
        public string Endpoint { get; set; }
        public string NodeName { get; set; }
        public bool IsOnline { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
    }
}
