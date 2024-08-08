using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DecentraCloud.API.Services
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly TokenHelper _tokenHelper;
        private readonly EncryptionHelper _encryptionHelper;

        public NodeService(INodeRepository nodeRepository, IUserRepository userRepository, TokenHelper tokenHelper, EncryptionHelper encryptionHelper)
        {
            _nodeRepository = nodeRepository;
            _userRepository = userRepository;
            _tokenHelper = tokenHelper;
            _encryptionHelper = encryptionHelper;
        }

        private HttpClient CreateHttpClient()
        {
            return new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        }

        public async Task<Node> RegisterNode(NodeRegistrationDto nodeRegistrationDto)
        {
            var user = await _userRepository.GetUserByEmail(nodeRegistrationDto.Email);
            if (user == null)
            {
                throw new Exception("User not found. Please go to decentracloud.com and sign up.");
            }

            var existingNode = (await _nodeRepository.GetNodesByUser(user.Id))
                .FirstOrDefault(n => n.NodeName == nodeRegistrationDto.NodeName);

            if (existingNode != null)
            {
                throw new Exception("Node already exists. Please login.");
            }

            var node = new Node
            {
                UserId = user.Id,
                Storage = nodeRegistrationDto.Storage,
                NodeName = nodeRegistrationDto.NodeName,
                Password = _encryptionHelper.HashPassword(nodeRegistrationDto.Password)
            };

            await _nodeRepository.AddNode(node);
            return node;
        }

        public async Task<string> LoginNode(NodeLoginDto nodeLoginDto)
        {
            var user = await _userRepository.GetUserByEmail(nodeLoginDto.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            var node = (await _nodeRepository.GetNodesByUser(user.Id))
                .FirstOrDefault(n => n.NodeName == nodeLoginDto.NodeName);

            if (node == null || !_encryptionHelper.VerifyPassword(nodeLoginDto.Password, node.Password))
            {
                throw new Exception("Invalid Node Name or Password");
            }

            var token = _tokenHelper.GenerateJwtToken(node);
            node.Token = token;
            node.IsOnline = true;
            node.Endpoint = nodeLoginDto.Endpoint;
            await _nodeRepository.UpdateNode(node);

            return token;
        }

        public async Task<bool> UpdateNodeStatus(NodeStatusDto nodeStatusDto)
        {
            var node = await _nodeRepository.GetNodeById(nodeStatusDto.NodeId);
            if (node == null)
            {
                return false;
            }

            node.Uptime = nodeStatusDto.Uptime;
            node.Downtime = nodeStatusDto.Downtime;
            node.StorageStats = new StorageStats
            {
                UsedStorage = nodeStatusDto.StorageStats.UsedStorage,
                AvailableStorage = nodeStatusDto.StorageStats.AvailableStorage
            };
            node.IsOnline = nodeStatusDto.IsOnline;
            node.CauseOfDowntime = nodeStatusDto.CauseOfDowntime;

            return await _nodeRepository.UpdateNode(node);
        }

        public async Task<long> GetFileSize(string nodeId, string filename)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null)
            {
                return 0;
            }

            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/file-size/{filename}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            var fileSizeString = await response.Content.ReadAsStringAsync();
            return long.Parse(fileSizeString);
        }

        public async Task<bool> UploadFileToNode(FileUploadDto fileUploadDto)
        {
            var node = await GetRandomOnlineNode();
            if (node == null)
            {
                return false;
            }

            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/upload";
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileUploadDto.Data);
            content.Add(fileContent, "file", fileUploadDto.Filename);
            content.Add(new StringContent(fileUploadDto.UserId), "userId");
            content.Add(new StringContent(fileUploadDto.Filename), "filename");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", node.Token);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {response.StatusCode}, Body: {responseBody}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Node> GetRandomOnlineNode()
        {
            var nodes = await _nodeRepository.GetAllNodes();
            var onlineNodes = nodes.Where(n => n.IsOnline && !string.IsNullOrEmpty(n.Endpoint)).ToList();
            if (!onlineNodes.Any())
            {
                return null;
            }

            var random = new Random();
            return onlineNodes[random.Next(onlineNodes.Count)];
        }

        public async Task<IEnumerable<NodeDto>> GetAllNodes()
        {
            var nodes = await _nodeRepository.GetAllNodes();
            return nodes.Select(n => new NodeDto
            {
                NodeName = n.NodeName,
                Endpoint = n.Endpoint,
                IsOnline = n.IsOnline,
                Storage = n.Storage
            });
        }

        public async Task<IEnumerable<Node>> GetNodesByUser(string userId)
        {
            return await _nodeRepository.GetNodesByUser(userId);
        }

        public async Task<Node> GetNodeById(string nodeId)
        {
            return await _nodeRepository.GetNodeById(nodeId);
        }

        public async Task<bool> UpdateNode(Node node)
        {
            return await _nodeRepository.UpdateNode(node);
        }

        public async Task<bool> DeleteNode(string nodeId)
        {
            return await _nodeRepository.DeleteNode(nodeId);
        }

        public async Task<Node> GetRandomNode()
        {
            var nodes = await _nodeRepository.GetAllNodes();
            if (nodes == null || !nodes.Any())
            {
                throw new Exception("No available nodes found.");
            }
            var random = new Random();
            return nodes.ElementAt(random.Next(nodes.Count()));
        }
    }
}
