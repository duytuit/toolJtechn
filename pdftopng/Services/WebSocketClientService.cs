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
        private readonly ClientWebSocket _client;
        private readonly Uri _serverUri;
   
        public WebSocketClientService(Uri serverUri)
        {
            _client = new ClientWebSocket();
            _serverUri = serverUri ?? throw new ArgumentNullException(nameof(serverUri));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           Console.WriteLine("Connecting to WebSocket server...");
           
           try
           {
               _client.Options.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
               await _client.ConnectAsync(_serverUri, stoppingToken);
               Console.WriteLine("Connected to WebSocket server!");
               var obj = new
               {
                   Event = 15,
                   Chanel = "dencanhbao_cd_dap"
               };
               string jsonData = JsonConvert.SerializeObject(obj);
               await SendMessage(jsonData, stoppingToken);
               _ = Task.Run(() => ReceiveMessages(stoppingToken), stoppingToken); // Nhận tin nhắn từ server
           
               while (!stoppingToken.IsCancellationRequested && _client.State == WebSocketState.Open)
               {
                   Console.Write("Enter message: ");
                   string message = Console.ReadLine();
                   if (string.IsNullOrEmpty(message)) break;
           
                   await SendMessage(message, stoppingToken);
               }
           
               await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
               Console.WriteLine("Connection closed.");
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Error: {ex.Message}");
           }
        }

        public async Task SendMessage(string message, CancellationToken stoppingToken)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, stoppingToken);
            Console.WriteLine($"Sent: {message}");
        }

        private async Task ReceiveMessages(CancellationToken stoppingToken)
        {
            byte[] buffer = new byte[1024];

            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Server requested close connection");
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");
            }
        }
    }
}
