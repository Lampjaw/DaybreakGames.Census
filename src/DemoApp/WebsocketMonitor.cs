using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class WebsocketMonitor : IWebsocketMonitor, IDisposable
    {
        private readonly ICensusStreamClient _client;
        private readonly ILogger<WebsocketMonitor> _logger;

        public WebsocketMonitor(ICensusStreamClient censusStreamClient, ILogger<WebsocketMonitor> logger)
        {
            _client = censusStreamClient;
            _logger = logger;

            var subscription = new CensusStreamSubscription
            {
                Characters = new[] { "all" },
                Worlds = new[] { "all" },
                EventNames = new[] { "PlayerLogin" }
            };

            _client.Subscribe(subscription)
                .OnMessage(OnMessage)
                .OnDisconnect(OnDisconnect);
        }

        public Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            return _client.DisconnectAsync();
        }

        public Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            return _client.ConnectAsync();
        }

        private Task OnMessage(string message)
        {
            if (message == null)
            {
                return Task.CompletedTask;
            }

            JToken msg;

            try
            {
                msg = JToken.Parse(message);
            }
            catch (Exception)
            {
                _logger.LogError("Failed to parse message: {0}", message);
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Received Message: {msg.ToString()}");

            return Task.CompletedTask;
        }

        private Task OnDisconnect(string error)
        {
            _logger.LogInformation("Websocket Client Disconnected!!");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_client != null && _client is IDisposable disClient)
            {
                disClient.Dispose();
            }
        }
    }
}
