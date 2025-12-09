using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pdftopng.Services
{
    public class WebSocketClientService : BackgroundService
    {
        private ClientWebSocket _client;
        private readonly Uri _serverUri;
        private readonly ILogger<WebSocketClientService> _logger;

        private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(20);

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public WebSocketClientService(Uri serverUri, ILogger<WebSocketClientService> logger)
        {
            _serverUri = serverUri;
            _logger = logger;
            _client = CreateNewClient();
        }

        private ClientWebSocket CreateNewClient()
        {
            var ws = new ClientWebSocket();
            ws.Options.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
            return ws;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocketClientService started.");

            await ConnectWithRetry(stoppingToken);

            _ = Task.Run(() => ReceiveLoop(stoppingToken), stoppingToken);
            _ = Task.Run(() => PingLoop(stoppingToken), stoppingToken);

            // Gửi message mẫu
            var obj = new { Event = 15, Chanel = "dencanhbao_cd_dap" };
            await Send(JsonConvert.SerializeObject(obj), stoppingToken);

            // Không cần vòng lặp Console.ReadLine – BackgroundService chạy ngầm
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        // ==============================
        // CONNECT + RECONNECT
        // ==============================
        private async Task ConnectWithRetry(CancellationToken token)
        {
            int retry = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    retry++;
                    _logger.LogInformation($"Connecting to {_serverUri} (attempt {retry})");

                    _client = CreateNewClient();
                    await _client.ConnectAsync(_serverUri, token);

                    _logger.LogInformation("WebSocket connected.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Connect failed: {ex.Message}");
                    await Task.Delay(GetReconnectDelay(retry), token);
                }
            }
        }

        private TimeSpan GetReconnectDelay(int retryCount)
        {
            if (retryCount < 3) return TimeSpan.FromSeconds(2);
            if (retryCount < 10) return TimeSpan.FromSeconds(5);
            return TimeSpan.FromSeconds(10);
        }

        // ==============================
        // SEND
        // ==============================
        public async Task Send(string message, CancellationToken token)
        {
            try
            {
                await _sendLock.WaitAsync(token);

                if (_client.State != WebSocketState.Open)
                {
                    _logger.LogWarning("Send failed – socket not open. Reconnecting...");
                    await ConnectWithRetry(token);
                }

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(buffer, WebSocketMessageType.Text, true, token);

                _logger.LogInformation($"Sent: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Send ERROR: {ex.Message}");
            }
            finally
            {
                _sendLock.Release();
            }
        }

        // ==============================
        // RECEIVE LOOP
        // ==============================
        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[8192];

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_client.State != WebSocketState.Open)
                    {
                        _logger.LogWarning("ReceiveLoop: socket closed. Reconnecting...");
                        await ConnectWithRetry(token);
                        continue;
                    }

                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("Server requested close. Reconnecting...");
                        await ConnectWithRetry(token);
                        continue;
                    }

                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Received: {msg}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Receive ERROR: {ex.Message}");
                    await ConnectWithRetry(token);
                }
            }
        }

        // ==============================
        // PING LOOP
        // ==============================
        private async Task PingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_pingInterval, token);

                try
                {
                    if (_client.State == WebSocketState.Open)
                    {
                        await Send("[PING]", token);
                    }
                }
                catch { }
            }
        }

        // ==============================
        // STOP
        // ==============================
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping WebSocket...");
            try
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", cancellationToken);
            }
            catch { }
            _client.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}
