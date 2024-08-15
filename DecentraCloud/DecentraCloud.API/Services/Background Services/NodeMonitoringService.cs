using DecentraCloud.API.Interfaces.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DecentraCloud.API.Services.Background_Services
{
    public class NodeMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NodeMonitoringService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                    var nodes = await nodeService.GetAllNodes();
                    foreach (var node in nodes)
                    {
                        await nodeService.MonitorNode(node.Id);
                    }
                }

                // Wait for a certain period before the next monitoring round
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
