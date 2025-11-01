using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnLog; // dùng để log ra TextBox WinForms

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
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                Log("🚪 Disconnected by client.");
            }
        }

        private async Task ConnectLoopAsync(CancellationToken token)
        {
            while (!_isManuallyClosed)
            {
                try
                {
                    _client = new ClientWebSocket();

                    // ⚙️ Cho phép bỏ qua chứng chỉ SSL (dành cho server nội bộ)
                    ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidation;

                    Log("🔌 Connecting to WebSocket server...");
                    await _client.ConnectAsync(_serverUri, token);
                    Log("✅ Connected to WebSocket server!");
                    OnConnected?.Invoke();

                    _ = Task.Run(() => ReceiveMessagesAsync(token));

                    return; // khi kết nối thành công thì thoát loop, chờ tới khi mất kết nối
                }
                catch (Exception ex)
                {
                    Log("❌ Connection failed: " + ex.Message);
                    Log("⏳ Reconnecting in 5 seconds...");
                    await Task.Delay(5000, token);
                }
            }
        }

        public async Task SendMessageAsync(string message, CancellationToken token)
        {
            if (_client == null || _client.State != WebSocketState.Open)
            {
                Log("⚠️ Cannot send message: WebSocket not connected.");
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, token);
            Log("📤 Sent: " + message);
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (_client.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log("🔚 Server requested close connection.");
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", token);
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(message);
                    Log("📩 Received: " + message);
                }
            }
            catch (Exception ex)
            {
                Log("⚠️ Receive error: " + ex.Message);
            }

            if (!_isManuallyClosed)
            {
                OnDisconnected?.Invoke();
                Log("🔁 Attempting to reconnect in 5 seconds...");
                await Task.Delay(5000, token);
                await ConnectLoopAsync(token);
            }
        }

        private static bool IgnoreCertificateValidation(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // ⚠️ Bỏ qua xác thực SSL (chỉ nên dùng cho môi trường test)
            return true;
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"{DateTime.Now:HH:mm:ss} {message}");
            Console.WriteLine(message);
        }
    }
}
