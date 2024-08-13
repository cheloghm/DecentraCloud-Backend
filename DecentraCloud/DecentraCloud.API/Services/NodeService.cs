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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(10) // Set a longer timeout
            };
        }

        public async Task<bool> PingNode(string nodeId)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null || string.IsNullOrEmpty(node.Token))
            {
                return false;
            }

            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/ping";

            // Add the authorization header
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", node.Token);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // If the node was previously offline, mark it as online and add the uptime
                if (!node.IsOnline)
                {
                    node.IsOnline = true;
                    node.Uptime.Add(DateTime.UtcNow); // Record uptime without overwriting existing values
                }
                await _nodeRepository.UpdateNode(node);
                return true;
            }
            else
            {
                // If the ping fails, handle the node's downtime status
                if (node.IsOnline)
                {
                    node.IsOnline = false;
                    node.Downtime.Add(new Dictionary<string, object>
            {
                { "Reason", "Ping failed" },
                { "Timestamp", DateTime.UtcNow }
            }); // Record downtime without overwriting existing values
                }
                await _nodeRepository.UpdateNode(node);
                return false;
            }
        }

        public async Task<bool> EnsureNodeIsOnline(string nodeId)
        {
            for (int i = 0; i < 3; i++)
            {
                if (await PingNode(nodeId))
                {
                    return true;
                }
            }

            // If ping fails 3 times, the node is considered offline, which is handled by PingNode
            return false;
        }

        public async Task<bool> UpdateNodeUptime(string nodeId)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null) return false;

            node.Uptime.Add(DateTime.UtcNow); // Append uptime record
            return await _nodeRepository.UpdateNode(node);
        }

        public async Task<bool> UpdateNodeStatus(NodeStatusDto nodeStatusDto)
        {
            var node = await _nodeRepository.GetNodeById(nodeStatusDto.NodeId);
            if (node == null)
            {
                return false;
            }

            node.Uptime = new List<DateTime> { DateTime.UtcNow }; // Example of how Uptime might be handled
            node.Downtime = new List<Dictionary<string, object>>(); // Initialize as empty list, to be updated as needed

            node.StorageStats = new StorageStats
            {
                UsedStorage = nodeStatusDto.StorageStats.UsedStorage,
                AvailableStorage = nodeStatusDto.StorageStats.AvailableStorage
            };

            node.IsOnline = nodeStatusDto.IsOnline;

            return await _nodeRepository.UpdateNode(node);
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

            var region = RegionHelper.DetermineRegion(nodeRegistrationDto.Country, nodeRegistrationDto.City);

            // Convert the storage from GB to bytes
            long storageInBytes = nodeRegistrationDto.Storage * 1024L * 1024L * 1024L;
            long halfStorageInBytes = storageInBytes / 2;

            var node = new Node
            {
                UserId = user.Id,
                Storage = storageInBytes,
                AllocatedFileStorage = new StorageStats
                {
                    UsedStorage = 0,
                    AvailableStorage = halfStorageInBytes
                },
                AllocatedDeploymentStorage = new StorageStats
                {
                    UsedStorage = 0,
                    AvailableStorage = halfStorageInBytes
                },
                NodeName = nodeRegistrationDto.NodeName,
                Country = nodeRegistrationDto.Country,
                City = nodeRegistrationDto.City,
                Region = region,
                Password = _encryptionHelper.HashPassword(nodeRegistrationDto.Password),
                StorageStats = new StorageStats
                {
                    UsedStorage = 0,
                    AvailableStorage = storageInBytes
                }
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

            node.Uptime.Add(DateTime.UtcNow); // Record uptime without overwriting existing values

            await _nodeRepository.UpdateNode(node);

            return token;
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

        public async Task<bool> VerifyNode(string userEmail, string nodeName)
        {
            var user = await _userRepository.GetUserByEmail(userEmail);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var existingNode = (await _nodeRepository.GetNodesByUser(user.Id))
                .FirstOrDefault(n => n.NodeName == nodeName);

            return existingNode != null;
        }
    }
}
