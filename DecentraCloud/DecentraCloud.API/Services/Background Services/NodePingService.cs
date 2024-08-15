using DecentraCloud.API.Interfaces.ServiceInterfaces;

namespace DecentraCloud.API.Services.Background_Services
{
    public class NodePingService : BackgroundService
    {
        private readonly INodeService _nodeService;

        public NodePingService(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nodes = await _nodeService.GetAllNodes();
                foreach (var node in nodes)
                {
                    await _nodeService.EnsureNodeIsOnline(node.Id);
                }

                // Wait for a certain period before the next ping round
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

}
