using Newtonsoft.Json;
using System;
using System.IO;
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
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

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
            ClientWebSocket socket;
            lock (_syncLock)
            {
                socket = _client;
                _client = null;
            }

            try
            {
                if (socket != null && socket.State == WebSocketState.Open)
                {
                    await _sendLock.WaitAsync();
                    try
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Client closed",
                            CancellationToken.None);
                    }
                    finally
                    {
                        _sendLock.Release();
                    }
                }
            }
            catch { }

            try
            {
                socket?.Dispose();
            }
            catch { }

            Log("🚪 WebSocket stopped.");
        }

        public async Task SendMessageAsync(string message, CancellationToken token)
        {
            await SendMessageAsync(message, token, null);
        }

        private async Task SendMessageAsync(
            string message,
            CancellationToken token,
            ClientWebSocket expectedSocket)
        {
            ClientWebSocket socket = null;
            Exception sendError = null;
            bool disconnected = false;

            await _sendLock.WaitAsync(token);
            try
            {
                lock (_syncLock)
                    socket = expectedSocket ?? _client;

                if (socket == null)
                {
                    Log("⚠️ Cannot send: socket not connected.");
                    return;
                }

                if (expectedSocket != null && !ReferenceEquals(socket, expectedSocket))
                    return;

                if (socket.State != WebSocketState.Open)
                {
                    disconnected = true;
                }
                else
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        token);
                }
            }
            catch (Exception ex)
            {
                sendError = ex;
            }
            finally
            {
                _sendLock.Release();
            }

            if (token.IsCancellationRequested)
                return;

            if (sendError != null)
            {
                if (IsCurrentSocket(socket))
                {
                    Log("❌ Send error: " + sendError.Message);
                    await HandleDisconnectAsync(socket, token);
                }
                return;
            }

            if (disconnected)
                await HandleDisconnectAsync(socket, token);
        }

        /* ======================= CORE ======================= */

        private async Task ConnectLoopAsync(CancellationToken token)
        {
            lock (_syncLock)
            {
                if (_isConnecting) return;
                _isConnecting = true;
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var socket = new ClientWebSocket();
                    lock (_syncLock)
                        _client = socket;

                    try
                    {
                        ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidation;

                        Log("🔌 Connecting...");
                        await socket.ConnectAsync(_serverUri, token);

                        _lastReceivedTime = DateTime.UtcNow;
                        Log("✅ Connected!");
                        OnConnected?.Invoke();

                        _ = Task.Run(() => ReceiveLoopAsync(socket, token));
                        _ = Task.Run(() => KeepAliveLoopAsync(socket, token));
                        return;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        socket.Dispose();
                        lock (_syncLock)
                        {
                            if (ReferenceEquals(_client, socket))
                                _client = null;
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        socket.Dispose();
                        lock (_syncLock)
                        {
                            if (ReferenceEquals(_client, socket))
                                _client = null;
                        }

                        Log("❌ Connect failed: " + ex.Message);
                        await Task.Delay(_reconnectDelay, token);
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally
            {
                lock (_syncLock)
                    _isConnecting = false;
            }
        }

        private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    using (var message = new MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await socket.ReceiveAsync(
                                new ArraySegment<byte>(buffer),
                                token);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                Log("🔚 Server closed connection.");
                                await HandleDisconnectAsync(socket, token);
                                return;
                            }

                            message.Write(buffer, 0, result.Count);
                        }
                        while (!result.EndOfMessage);

                        _lastReceivedTime = DateTime.UtcNow;
                        OnMessageReceived?.Invoke(Encoding.UTF8.GetString(message.ToArray()));
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex)
            {
                if (IsCurrentSocket(socket))
                    Log("⚠️ Receive error: " + ex.Message);
            }

            await HandleDisconnectAsync(socket, token);
        }

        private async Task KeepAliveLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (socket.State != WebSocketState.Open)
                {
                    await HandleDisconnectAsync(socket, token);
                    return;
                }

                if (DateTime.UtcNow - _lastReceivedTime > _receiveTimeout)
                {
                    Log("⏰ Receive timeout. Network lost?");
                    await HandleDisconnectAsync(socket, token);
                    return;
                }

                var ping = JsonConvert.SerializeObject(new
                {
                    Event = 15,
                    Chanel = "dencanhbao_cd_dap",
                    MessageText = "ping"
                });

                try
                {
                    await SendMessageAsync(ping, token, socket);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    await Task.Delay(_pingInterval, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        /* ======================= RECONNECT ======================= */

        private async Task HandleDisconnectAsync(ClientWebSocket socket, CancellationToken token)
        {
            bool ownsReconnect = false;
            try
            {
                await _sendLock.WaitAsync(token);
                try
                {
                    lock (_syncLock)
                    {
                        if (_isReconnecting ||
                            token.IsCancellationRequested ||
                            !ReferenceEquals(_client, socket))
                            return;

                        _isReconnecting = true;
                        ownsReconnect = true;
                        _client = null;
                    }

                    OnDisconnected?.Invoke();
                    Log("🔁 Reconnecting...");
                    socket.Dispose();
                }
                finally
                {
                    _sendLock.Release();
                }

                await Task.Delay(_reconnectDelay, token);
                await ConnectLoopAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally
            {
                if (ownsReconnect)
                {
                    lock (_syncLock)
                        _isReconnecting = false;
                }
            }
        }

        private bool IsCurrentSocket(ClientWebSocket socket)
        {
            lock (_syncLock)
                return ReferenceEquals(_client, socket);
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
