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

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isResolved")]
        public bool IsResolved { get; set; } = false;

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; } = null;

        [BsonElement("criticalLevel")]
        public string CriticalLevel { get; set; }

        [BsonElement("relatedNodeId")]
        public string RelatedNodeId { get; set; } // Ensure this exists

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } // Ensure this exists
    }

}
