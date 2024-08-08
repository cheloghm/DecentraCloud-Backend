using DecentraCloud.API.Helpers;

namespace DecentraCloud.API.Services
{
    public class TokenRenewalService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TokenHelper _tokenHelper;

        public TokenRenewalService(IServiceScopeFactory scopeFactory, TokenHelper tokenHelper)
        {
            _scopeFactory = scopeFactory;
            _tokenHelper = tokenHelper;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RenewTokens, null, TimeSpan.Zero, TimeSpan.FromDays(7));
            return Task.CompletedTask;
        }

        private void RenewTokens(object state)
        {
            _tokenHelper.RenewTokens(_scopeFactory);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
