using DecentraCloud.API.Interfaces.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DecentraCloud.API.Services.Background_Services
{
    public class NodePingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NodePingService(IServiceProvider serviceProvider)
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
                        await nodeService.EnsureNodeIsOnline(node.Id);
                    }
                }

                // Wait for a certain period before the next ping round
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

}
