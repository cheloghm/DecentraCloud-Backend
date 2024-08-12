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

        [BsonElement("allocatedFileStorage")]
        public double AllocatedFileStorage { get; set; }

        [BsonElement("allocatedDeploymentStorage")]
        public double AllocatedDeploymentStorage { get; set; }

        [BsonElement("uptime")]
        public List<DateTime> Uptime { get; set; } = new List<DateTime>();

        [BsonElement("downtime")]
        public List<Dictionary<string, object>> Downtime { get; set; } = new List<Dictionary<string, object>>();
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
