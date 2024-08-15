using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DecentraCloud.API.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("nodeId")]
        public string NodeId { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
