using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pdftopng.Services
{
    public class WebSocketClientService : BackgroundService
    {
        private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(20);

        private readonly Uri _serverUri;
        private readonly ILogger<WebSocketClientService> _logger;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly object _connectionSync = new object();

        private ClientWebSocket _client;
        private TaskCompletionSource<bool> _connectionReady = CreateConnectionSignal();

        public WebSocketClientService(Uri serverUri, ILogger<WebSocketClientService> logger)
        {
            _serverUri = serverUri ?? throw new ArgumentNullException(nameof(serverUri));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private ClientWebSocket CreateNewClient()
        {
            var client = new ClientWebSocket();
            client.Options.RemoteCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;
            return client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket client service started for {ServerUri}.", _serverUri);
            int retryCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var client = CreateNewClient();
                DateTime connectedAt = DateTime.MinValue;

                try
                {
                    _logger.LogInformation(
                        "Connecting to WebSocket server (attempt {Attempt}).",
                        retryCount + 1);

                    using (var connectionCts =
                        CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
                    {
                        connectionCts.CancelAfter(ConnectionTimeout);
                        await client.ConnectAsync(_serverUri, connectionCts.Token);
                        connectionCts.CancelAfter(Timeout.InfiniteTimeSpan);

                        await SendOnSocketAsync(
                            client,
                            JsonConvert.SerializeObject(new
                            {
                                Event = 15,
                                Chanel = "dencanhbao_cd_dap",
                                MessageText = ""
                            }),
                            connectionCts.Token);

                        lock (_connectionSync)
                        {
                            _client = client;
                            _connectionReady.TrySetResult(true);
                        }
                        connectedAt = DateTime.UtcNow;
                    }

                    _logger.LogInformation(
                        "WebSocket connected and channel registration sent to {ServerUri}.",
                        _serverUri);

                    await ReceiveLoopAsync(client, stoppingToken);
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("WebSocket connection closed by the server.");
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket connection failed.");
                }
                finally
                {
                    await _sendLock.WaitAsync();
                    try
                    {
                        lock (_connectionSync)
                        {
                            if (ReferenceEquals(_client, client))
                            {
                                _client = null;
                                _connectionReady = CreateConnectionSignal();
                            }
                        }

                        client.Abort();
                        client.Dispose();
                    }
                    finally
                    {
                        _sendLock.Release();
                    }
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                if (connectedAt != DateTime.MinValue &&
                    DateTime.UtcNow - connectedAt > TimeSpan.FromMinutes(1))
                {
                    retryCount = 0;
                }

                retryCount++;
                TimeSpan delay = GetReconnectDelay(retryCount);
                _logger.LogWarning("Reconnecting to WebSocket in {DelaySeconds} seconds.", delay.TotalSeconds);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("WebSocket client service stopped.");
        }

        private static TimeSpan GetReconnectDelay(int retryCount)
        {
            if (retryCount <= 1) return TimeSpan.FromSeconds(2);
            if (retryCount <= 4) return TimeSpan.FromSeconds(5);
            return TimeSpan.FromSeconds(10);
        }

        public async Task Send(string message, CancellationToken token)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            while (true)
            {
                token.ThrowIfCancellationRequested();

                Task readyTask;
                lock (_connectionSync)
                {
                    readyTask = _connectionReady.Task;
                }

                await WaitForConnectionAsync(readyTask, token);
                await _sendLock.WaitAsync(token);

                try
                {
                    ClientWebSocket client;
                    lock (_connectionSync)
                    {
                        client = _client;
                    }

                    if (client == null || client.State != WebSocketState.Open)
                    {
                        client?.Abort();
                        continue;
                    }

                    await SendOnSocketAsync(client, message, token);
                    _logger.LogInformation(
                        "WebSocket message sent ({MessageLength} characters).",
                        message.Length);
                    return;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket send failed; the connection will be restarted.");
                    ClientWebSocket client;
                    lock (_connectionSync)
                    {
                        client = _client;
                    }

                    client?.Abort();
                    throw;
                }
                finally
                {
                    _sendLock.Release();
                }
            }
        }

        private static async Task WaitForConnectionAsync(
            Task readyTask,
            CancellationToken token)
        {
            var cancellationTask = new TaskCompletionSource<bool>();
            using (token.Register(
                state => ((TaskCompletionSource<bool>)state).TrySetResult(true),
                cancellationTask))
            {
                Task completedTask = await Task.WhenAny(readyTask, cancellationTask.Task);
                if (completedTask != readyTask)
                    token.ThrowIfCancellationRequested();

                await readyTask;
            }
        }

        private async Task SendOnSocketAsync(
            ClientWebSocket client,
            string message,
            CancellationToken token)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                token);
        }

        private async Task ReceiveLoopAsync(
            ClientWebSocket client,
            CancellationToken token)
        {
            var buffer = new byte[8192];
            using (var messageStream = new MemoryStream())
            {
                while (!token.IsCancellationRequested &&
                       client.State == WebSocketState.Open)
                {
                    var result = await client.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning(
                            "WebSocket server requested close: {Status} {Description}.",
                            result.CloseStatus,
                            result.CloseStatusDescription);
                        return;
                    }

                    messageStream.Write(buffer, 0, result.Count);
                    if (!result.EndOfMessage)
                        continue;

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(messageStream.ToArray());
                        _logger.LogDebug("WebSocket message received ({MessageLength} characters).", message.Length);
                    }

                    messageStream.SetLength(0);
                }
            }
        }

        private static TaskCompletionSource<bool> CreateConnectionSignal()
        {
            return new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
