using System;
using System.Threading.Tasks;

namespace DaybreakGames.Census.Stream
{
    public interface ICensusStreamClient
    {
        CensusStreamClient SetServiceId(string serviceId);
        CensusStreamClient SetServiceNamespace(string serviceNamespace);
        CensusStreamClient Subscribe(CensusStreamSubscription subscription);
        CensusStreamClient OnDisconnect(Func<string, Task> onDisconnect);
        CensusStreamClient OnMessage(Func<string, Task> onMessage);
        Task ConnectAsync();
        Task DisconnectAsync();
    }
}
