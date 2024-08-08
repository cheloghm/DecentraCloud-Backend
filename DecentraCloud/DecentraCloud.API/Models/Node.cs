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

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("storage")]
        public int Storage { get; set; }

        [BsonElement("uptime")]
        public long Uptime { get; set; }

        [BsonElement("downtime")]
        public long Downtime { get; set; }

        [BsonElement("storageStats")]
        public StorageStats StorageStats { get; set; }

        [BsonElement("causeOfDowntime")]
        public string CauseOfDowntime { get; set; }

        [BsonElement("token")]
        public string Token { get; set; }

        [BsonElement("endpoint")]
        public string Endpoint { get; set; }

        [BsonElement("nodeName")]
        public string NodeName { get; set; }

        [BsonElement("isOnline")]
        public bool IsOnline { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }
    }
}
