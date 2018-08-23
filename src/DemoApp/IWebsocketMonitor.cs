using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public interface IWebsocketMonitor
    {
        Task OnApplicationStartup(CancellationToken cancellationToken);
        Task OnApplicationShutdown(CancellationToken cancellationToken);
    }
}
