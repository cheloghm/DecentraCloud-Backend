using DecentraCloud.API.Interfaces.ServiceInterfaces;

namespace DecentraCloud.API.Services.Background_Services
{
    public class NodeMonitoringService : BackgroundService
    {
        private readonly INodeService _nodeService;

        public NodeMonitoringService(INodeService nodeService)
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
                    await _nodeService.MonitorNode(node.Id);
                }

                // Wait for a certain period before the next monitoring round
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
