using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DaybreakGames.Census.Stream
{
    public class CensusStreamClient : ICensusStreamClient, IDisposable
    {
        private readonly IOptions<CensusOptions> _options;
        private readonly ILogger<CensusStreamClient> _logger;

        private const int _chunkSize = 1024;
        private JsonSerializerSettings sendMessageSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private WebSocketWrapper _client;

        private bool _internalState = false;

        private string _serviceId { get; set; }
        private string _serviceNamespace { get; set; }
        private CensusStreamSubscription _subscription { get; set; }

        private Timer _reconnectTimer;
        private readonly TimeSpan _stateCheckInterval = TimeSpan.FromSeconds(5);
        private Func<string, Task> _onMessage;
        private Func<string, Task> _onDisconnected;

        public CensusStreamClient(IOptions<CensusOptions> options, ILogger<CensusStreamClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public CensusStreamClient SetServiceId(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId));
            }

            _serviceId = serviceId;

            return this;
        }

        public CensusStreamClient SetServiceNamespace(string serviceNamespace)
        {
            if (string.IsNullOrWhiteSpace(serviceNamespace))
            {
                throw new ArgumentNullException(nameof(serviceNamespace));
            }

            _serviceNamespace = serviceNamespace;

            return this;
        }

        public CensusStreamClient Subscribe(CensusStreamSubscription subscription)
        {
            _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));

            return this;
        }

        public CensusStreamClient OnDisconnect(Func<string, Task> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        public CensusStreamClient OnMessage(Func<string, Task> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        public Task ConnectAsync()
        {
            _internalState = true;

            _reconnectTimer?.Dispose();
            _reconnectTimer = new Timer(ReconnectClientAsync, null, (int)_stateCheckInterval.TotalMilliseconds, (int)_stateCheckInterval.TotalMilliseconds);

            return ConnectInternal();
        }

        public async Task DisconnectAsync()
        {
            _internalState = false;
            _reconnectTimer?.Dispose();

            if (_client != null && _client.State == WebSocketState.Open)
            {
                await _client.Disconnect();
                _client.Dispose();
            }
        }

        private async Task ConnectInternal()
        {
            _client = WebSocketWrapper.Create(GetEndpoint())
                .OnDisconnect(HandleDisconnect)
                .OnMessage(HandleMessage);

            await _client.Connect();

            _logger.LogInformation("Census stream connected");

            if (_subscription.EventNames.Any())
            {
                var sMessage = JsonConvert.SerializeObject(_subscription, sendMessageSettings);

                await _client.SendMessage(sMessage);

                _logger.LogInformation($"Subscribed to census with: {sMessage}");
            }
        }

        private async void HandleDisconnect(string error, WebSocketWrapper ws)
        {
            _logger.LogInformation($"Census stream client has closed: {error}");

            OnDisconnect(error);

            if (_internalState)
            {
                _logger.LogInformation("Attempting reconnect of Census stream...");

                await ConnectInternal();
            }
        }

        private void HandleMessage(string message, WebSocketWrapper ws)
        {
            if (_onMessage != null)
            {
                Task.Run(() => _onMessage(message));
            }
        }

        private void OnDisconnect(string error = null)
        {
            if (_onDisconnected != null)
            {
                Task.Run(() => _onDisconnected(error));
            }
        }

        private async void ReconnectClientAsync(Object stateInfo)
        {
            if (!_internalState)
            {
                _reconnectTimer?.Dispose();
                return;
            }

            if (_client?.State == null || _client?.State == WebSocketState.Closed)
            {
                _logger.LogInformation("Census stream client is closed. Attempting reconnect");

                await ConnectInternal();
            }
        }

        private string GetEndpoint()
        {
            var ns = _serviceNamespace ?? _options.Value.CensusServiceNamespace;
            var sId = _serviceId ?? _options.Value.CensusServiceId;

            return $"{Constants.CensusWebsocketEndpoint}?environment={ns}&service-id=s:{sId}";
        }

        public void Dispose()
        {
            _reconnectTimer?.Dispose();
            _client?.Dispose();
        }
    }
}
