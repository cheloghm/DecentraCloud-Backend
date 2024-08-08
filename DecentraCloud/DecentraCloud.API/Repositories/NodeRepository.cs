using DecentraCloud.API.Data;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecentraCloud.API.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly DecentraCloudContext _context;

        public NodeRepository(DecentraCloudContext context)
        {
            _context = context;
        }

        public async Task AddNode(Node node)
        {
            if (string.IsNullOrEmpty(node.Id))
            {
                node.Id = ObjectId.GenerateNewId().ToString();
            }
            await _context.Nodes.InsertOneAsync(node);
        }

        public async Task<Node> GetNodeById(string nodeId)
        {
            return await _context.Nodes.Find(n => n.Id == nodeId).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateNode(Node node)
        {
            var result = await _context.Nodes.ReplaceOneAsync(n => n.Id == node.Id, node);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNode(string nodeId)
        {
            var result = await _context.Nodes.DeleteOneAsync(n => n.Id == nodeId);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Node>> GetNodesByUser(string userId)
        {
            return await _context.Nodes.Find(n => n.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Node>> GetAllNodes()
        {
            return await _context.Nodes.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Node>> GetOnlineNodes()
        {
            return await _context.Nodes.Find(n => n.IsOnline).ToListAsync();
        }

    }
}
