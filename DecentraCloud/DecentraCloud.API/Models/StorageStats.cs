using MongoDB.Bson.Serialization.Attributes;

namespace DecentraCloud.API.Models
{
    public class StorageStats
    {
        [BsonElement("usedStorage")]
        public long UsedStorage { get; set; }

        [BsonElement("availableStorage")]
        public long AvailableStorage { get; set; }
    }
}
