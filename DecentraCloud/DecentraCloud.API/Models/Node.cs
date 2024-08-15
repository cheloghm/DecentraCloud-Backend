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
        public long Storage { get; set; }

        public StorageStats AllocatedFileStorage { get; set; } = new StorageStats();
        public StorageStats AllocatedDeploymentStorage { get; set; } = new StorageStats();

        public List<DateTime> Uptime { get; set; } = new List<DateTime>();
        public List<Dictionary<string, object>> Downtime { get; set; } = new List<Dictionary<string, object>>();

        public string Token { get; set; }
        public string Endpoint { get; set; }
        public string NodeName { get; set; }
        public bool IsOnline { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Region { get; set; }

        [BsonElement("Availability")]
        public Dictionary<string, object> Availability { get; set; } = new Dictionary<string, object>();

        // Aggregate storage stats
        public StorageStats StorageStats => new StorageStats
        {
            UsedStorage = AllocatedFileStorage.UsedStorage + AllocatedDeploymentStorage.UsedStorage,
            AvailableStorage = Storage - (AllocatedFileStorage.UsedStorage + AllocatedDeploymentStorage.UsedStorage)
        };
    }

}
