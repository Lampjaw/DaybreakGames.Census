using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class WebsocketMonitorHostedService : IHostedService
    {
        private IWebsocketMonitor _hostedService { get; set; }

        public WebsocketMonitorHostedService(IWebsocketMonitor hostedService)
        {
            _hostedService = hostedService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _hostedService.OnApplicationStartup(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _hostedService.OnApplicationShutdown(cancellationToken);
        }
    }
}
