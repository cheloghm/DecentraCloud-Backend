using DecentraCloud.API.Models;
using MongoDB.Driver;

namespace DecentraCloud.API.Data
{
    // Context class for MongoDB interactions.
    public class DecentraCloudContext
    {
        private readonly IMongoDatabase _database;

        public DecentraCloudContext(IMongoDatabase database)
        {
            _database = database;
        }

        // Provides access to the Users collection in MongoDB.
        public IMongoCollection<User> Users
        {
            get { return _database.GetCollection<User>("Users"); }
        }

        public IMongoCollection<Node> Nodes
        {
            get { return _database.GetCollection<Node>("Nodes"); }
        }

        public IMongoCollection<FileRecord> Files
        {
            get { return _database.GetCollection<FileRecord>("Files"); }
        }

    }
}
