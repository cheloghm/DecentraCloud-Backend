using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Models;
using Newtonsoft.Json;
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
        private readonly INotificationRepository _notificationRepository;

        public NodeService(INodeRepository nodeRepository, IUserRepository userRepository, TokenHelper tokenHelper, EncryptionHelper encryptionHelper, INotificationRepository notificationRepository)
        {
            _nodeRepository = nodeRepository;
            _userRepository = userRepository;
            _tokenHelper = tokenHelper;
            _encryptionHelper = encryptionHelper;
            _notificationRepository = notificationRepository;
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

        private async Task NotifyAdmin(string message, Node node)
        {
            var notification = new Notification
            {
                Message = message,
                NodeId = node.Id,
                Timestamp = DateTime.UtcNow
            };

            await _notificationRepository.AddNotification(notification);
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
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null)
            {
                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                var pingResult = await PingNodeWithLatency(nodeId);
                if (pingResult.IsSuccess)
                {
                    NodeStatusHelper.RecordNodeAvailable(node);
                    await _nodeRepository.UpdateNode(node);
                    return true;
                }
                else if (!NodeStatusHelper.CheckPingLatency(pingResult.Latency, node))
                {
                    await _nodeRepository.UpdateNode(node);
                    return false;
                }
            }

            // Node is considered unavailable after 3 failed pings
            NodeStatusHelper.RecordNodeUnavailable(node);

            if ((DateTime.UtcNow - (DateTime)node.Availability["Timestamp"]).TotalHours >= 1)
            {
                NodeStatusHelper.RecordNodeOffline(node);
            }

            await _nodeRepository.UpdateNode(node);
            return false;
        }

        private async Task<(bool IsSuccess, int Latency)> PingNodeWithLatency(string nodeId)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null || string.IsNullOrEmpty(node.Token))
            {
                return (false, 0);
            }

            var httpClient = CreateHttpClient();
            var url = $"{node.Endpoint}/storage/ping";

            // Add the authorization header
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", node.Token);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url);
            stopwatch.Stop();

            var latency = (int)stopwatch.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                return (true, latency);
            }
            return (false, latency);
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

            // Update the allocated file storage and deployment storage
            node.AllocatedFileStorage.UsedStorage = nodeStatusDto.StorageStats.UsedStorage / 2;  // Assuming half is used for file storage
            node.AllocatedFileStorage.AvailableStorage = node.AllocatedFileStorage.AvailableStorage - (nodeStatusDto.StorageStats.UsedStorage / 2);

            node.AllocatedDeploymentStorage.UsedStorage = nodeStatusDto.StorageStats.UsedStorage / 2;  // Assuming the other half is used for deployment storage
            node.AllocatedDeploymentStorage.AvailableStorage = node.AllocatedDeploymentStorage.AvailableStorage - (nodeStatusDto.StorageStats.UsedStorage / 2);

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

        private async Task<(int CpuUsage, int MemoryUsage)> FetchNodeResourceUsage(string nodeId)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null || string.IsNullOrEmpty(node.Endpoint) || string.IsNullOrEmpty(node.Token))
            {
                throw new Exception("Node not found or not properly configured.");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", node.Token);
            var response = await httpClient.GetAsync($"{node.Endpoint}/status/resource-usage");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch resource usage for node {nodeId}");
            }

            var resourceUsageJson = await response.Content.ReadAsStringAsync();
            var resourceUsageData = JsonConvert.DeserializeObject<ResourceUsageDto>(resourceUsageJson);

            int cpuUsage = int.Parse(resourceUsageData.CpuUsage.Replace("%", ""));
            int memoryUsage = (int)((double.Parse(resourceUsageData.MemoryUsage.UsedMemory) / double.Parse(resourceUsageData.MemoryUsage.TotalMemory)) * 100);

            return (cpuUsage, memoryUsage);
        }

        private async Task<int> FetchNodeAuthAttempts(string nodeId)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null || string.IsNullOrEmpty(node.Endpoint) || string.IsNullOrEmpty(node.Token))
            {
                throw new Exception("Node not found or not properly configured.");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", node.Token);
            var response = await httpClient.GetAsync($"{node.Endpoint}/status/auth-attempts");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch auth attempts for node {nodeId}");
            }

            var failedAttemptsJson = await response.Content.ReadAsStringAsync();
            var failedAttemptsData = JsonConvert.DeserializeObject<AuthAttemptsDto>(failedAttemptsJson);

            return failedAttemptsData.FailedAttempts;
        }

        public async Task<bool> CheckAndHandleNodeResourceUsage(string nodeId, int cpuUsage, int memoryUsage)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null)
            {
                return false;
            }

            if (!NodeStatusHelper.CheckResourceUsage(cpuUsage, memoryUsage, node))
            {
                await _nodeRepository.UpdateNode(node);
                await NotifyAdmin("High resource usage detected", node);
                return false;
            }

            await _nodeRepository.UpdateNode(node);
            return true;
        }

        public async Task<bool> HandleFailedAuthAttempts(string nodeId, int failedAttempts)
        {
            var node = await _nodeRepository.GetNodeById(nodeId);
            if (node == null)
            {
                return false;
            }

            if (!NodeStatusHelper.CheckFailedAuthAttempts(failedAttempts, node))
            {
                await _nodeRepository.UpdateNode(node);
                await NotifyAdmin("Multiple failed authentication attempts", node);
                return false;
            }

            return true;
        }

        public async Task MonitorNode(string nodeId)
        {
            try
            {
                // Fetch CPU and memory usage
                var resourceUsage = await FetchNodeResourceUsage(nodeId);
                await CheckAndHandleNodeResourceUsage(nodeId, resourceUsage.CpuUsage, resourceUsage.MemoryUsage);

                // Fetch authentication attempts
                var failedAttempts = await FetchNodeAuthAttempts(nodeId);
                await HandleFailedAuthAttempts(nodeId, failedAttempts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Monitoring failed for node {nodeId}: {ex.Message}");
            }
        }

    }
}
