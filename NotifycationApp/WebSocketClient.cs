using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotifycationApp
{
    public class WebSocketClient
    {
        private readonly Uri _serverUri;
        private ClientWebSocket _client;

        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _receiveTimeout = TimeSpan.FromSeconds(30);

        private DateTime _lastReceivedTime;
        private bool _isConnecting;
        private bool _isReconnecting;

        private readonly object _syncLock = new object();

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnLog;

        public WebSocketClient(string serverUrl)
        {
            _serverUri = new Uri(serverUrl);
        }

        /* ======================= PUBLIC ======================= */

        public async Task StartAsync(CancellationToken token)
        {
            await ConnectLoopAsync(token);
        }

        public async Task StopAsync()
        {
            try
            {
                if (_client != null && _client.State == WebSocketState.Open)
                {
                    await _client.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client closed",
                        CancellationToken.None
                    );
                }
            }
            catch { }

            try
            {
                _client?.Dispose();
                _client = null;
            }
            catch { }

            Log("🚪 WebSocket stopped.");
        }

        public async Task SendMessageAsync(string message, CancellationToken token)
        {
            if (_client == null || _client.State != WebSocketState.Open)
            {
                Log("⚠️ Cannot send: socket not connected.");
                await HandleDisconnectAsync(token);
                return;
            }

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    token
                );
            }
            catch (Exception ex)
            {
                Log("❌ Send error: " + ex.Message);
                await HandleDisconnectAsync(token);
            }
        }

        /* ======================= CORE ======================= */

        private async Task ConnectLoopAsync(CancellationToken token)
        {
            lock (_syncLock)
            {
                if (_isConnecting) return;
                _isConnecting = true;
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    _client?.Dispose();

                    _client = new ClientWebSocket();
                    ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidation;

                    Log("🔌 Connecting...");
                    await _client.ConnectAsync(_serverUri, token);

                    _lastReceivedTime = DateTime.UtcNow;
                    _isConnecting = false;

                    Log("✅ Connected!");
                    OnConnected?.Invoke();

                    _ = Task.Run(() => ReceiveLoopAsync(token));
                    _ = Task.Run(() => KeepAliveLoopAsync(token));

                    return;
                }
                catch (Exception ex)
                {
                    Log("❌ Connect failed: " + ex.Message);
                    await Task.Delay(_reconnectDelay, token);
                }
            }

            _isConnecting = false;
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (_client != null &&
                       _client.State == WebSocketState.Open &&
                       !token.IsCancellationRequested)
                {
                    var result = await _client.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log("🔚 Server closed connection.");
                        break;
                    }

                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _lastReceivedTime = DateTime.UtcNow;

                    OnMessageReceived?.Invoke(msg);
                }
            }
            catch (Exception ex)
            {
                Log("⚠️ Receive error: " + ex.Message);
            }

            await HandleDisconnectAsync(token);
        }

        private async Task KeepAliveLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_client == null || _client.State != WebSocketState.Open)
                    {
                        await HandleDisconnectAsync(token);
                        return;
                    }

                    // TIMEOUT kiểm tra mạng treo
                    if (DateTime.UtcNow - _lastReceivedTime > _receiveTimeout)
                    {
                        Log("⏰ Receive timeout. Network lost?");
                        await HandleDisconnectAsync(token);
                        return;
                    }

                    // Ping
                    var ping = JsonConvert.SerializeObject(new
                    {
                        Event = 15,
                        Chanel = "dencanhbao_cd_dap",
                        MessageText = "ping"
                    });

                    await SendMessageAsync(ping, token);
                }
                catch { }

                await Task.Delay(_pingInterval, token);
            }
        }

        /* ======================= RECONNECT ======================= */

        private async Task HandleDisconnectAsync(CancellationToken token)
        {
            lock (_syncLock)
            {
                if (_isReconnecting) return;
                _isReconnecting = true;
            }

            try
            {
                OnDisconnected?.Invoke();
                Log("🔁 Reconnecting...");

                _client?.Dispose();
                _client = null;

                await Task.Delay(_reconnectDelay, token);
            }
            catch { }
            finally
            {
                _isReconnecting = false;
            }

            await ConnectLoopAsync(token);
        }

        /* ======================= UTILS ======================= */

        private static bool IgnoreCertificateValidation(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            return true; // chỉ dùng test
        }

        private void Log(string msg)
        {
            OnLog?.Invoke($"{DateTime.Now:HH:mm:ss} {msg}");
        }
    }
}
