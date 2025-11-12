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
        private bool _isManuallyClosed = false;
        private bool _isConnecting = false;
        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(15);
        private DateTime _lastReceivedTime = DateTime.UtcNow;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnLog;

        public bool IsManuallyClosed => _isManuallyClosed;

        public WebSocketClient(string serverUrl)
        {
            _serverUri = new Uri(serverUrl);
        }

        public async Task StartAsync(CancellationToken token)
        {
            _isManuallyClosed = false;
            await ConnectLoopAsync(token);
        }

        public async Task StopAsync()
        {
            _isManuallyClosed = true;

            if (_client != null && _client.State == WebSocketState.Open)
            {
                try
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    Log("❌ StopAsync WebSocketException: " + ex.Message);
                }
                Log("🚪 Disconnected by client.");
            }
        }

        private async Task ConnectLoopAsync(CancellationToken token)
        {
            if (_isConnecting) return;
            _isConnecting = true;

            while (!_isManuallyClosed && !token.IsCancellationRequested)
            {
                try
                {
                    _client = new ClientWebSocket();
                    ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidation;

                    Log("🔌 Connecting to WebSocket server...");
                    await _client.ConnectAsync(_serverUri, token);
                    Log("✅ Connected to WebSocket server!");
                    _isConnecting = false;
                    OnConnected?.Invoke();

                    _lastReceivedTime = DateTime.UtcNow;

                    // chạy song song Receive và KeepAlivePing
                    _ = Task.Run(() => ReceiveMessagesAsync(token));
                    _ = Task.Run(() => KeepAlivePingAsync(token));

                    return; // thoát loop sau khi kết nối thành công
                }
                catch (Exception ex)
                {
                    Log($"❌ Connection failed: {ex.Message}");
                    Log($"⏳ Reconnecting in {_reconnectDelay.TotalSeconds} seconds...");
                    await Task.Delay(_reconnectDelay, token);
                }
            }

            _isConnecting = false;
        }

        public async Task SendMessageAsync(string message, CancellationToken token)
        {
            if (_client == null || _client.State != WebSocketState.Open)
            {
                Log("⚠️ Cannot send message: WebSocket not connected.");
                await HandleDisconnectAsync(token);
                return;
            }

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, token);
                Log("📤 Sent: " + message);
            }
            catch (WebSocketException ex)
            {
                Log("❌ Send failed: " + ex.Message);
                await HandleDisconnectAsync(token);
            }
            catch (Exception ex)
            {
                Log("❌ Unexpected send error: " + ex.Message);
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (_client != null && _client.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log("🔚 Server requested close connection.");
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", token);
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _lastReceivedTime = DateTime.UtcNow;

                    // Nếu server gửi ping, trả pong
                    if (message.Trim().ToLower() == "ping")
                    {
                        await SendMessageAsync("pong", token);
                        continue;
                    }

                    OnMessageReceived?.Invoke(message);
                    Log("📩 Received: " + message);
                }
            }
            catch (WebSocketException ex)
            {
                Log("⚠️ Receive WebSocketException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log("⚠️ Receive error: " + ex.Message);
            }

            await HandleDisconnectAsync(token);
        }

        private async Task KeepAlivePingAsync(CancellationToken token)
        {
            while (!_isManuallyClosed && !token.IsCancellationRequested)
            {
                try
                {
                    if (_client == null || _client.State != WebSocketState.Open)
                    {
                        Log("⚠️ Connection lost. Reconnecting...");
                        await HandleDisconnectAsync(token);
                        return;
                    }

                    // ping server
                    var pingMsg = JsonConvert.SerializeObject(new
                    {
                        Event = 15,
                        Chanel = "dencanhbao_cd_dap",
                        MessageText = ""
                    });
                    await SendMessageAsync(pingMsg, token);
                }
                catch
                {
                    // lỗi ping sẽ được xử lý trong SendMessageAsync
                }

                await Task.Delay(_pingInterval, token);
            }
        }

        private async Task HandleDisconnectAsync(CancellationToken token)
        {
            if (_isManuallyClosed) return;

            OnDisconnected?.Invoke();
            Log($"🔁 Reconnecting in {_reconnectDelay.TotalSeconds} seconds...");

            try
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }
            catch { }

            await Task.Delay(_reconnectDelay, token);
            await ConnectLoopAsync(token);
        }

        private static bool IgnoreCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // chỉ dùng test
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"{DateTime.Now:HH:mm:ss} {message}");
            Console.WriteLine(message);
        }
    }
}
