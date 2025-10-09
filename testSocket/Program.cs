using Newtonsoft.Json;
using System;
using WebSocketSharp;

namespace testSocket
{
    class Program
    {
        WebSocket ws;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.connectSocket();

            Console.WriteLine("Nhan phim bat ky de thoat...");
            Console.ReadKey(); // giữ chương trình chạy
        }

        private void connectSocket()
        {
            try
            {
                Console.WriteLine("Dang ket noi toi WebSocket server...");

                ws = new WebSocket("wss://192.168.217.76:5007/ws");

                // Nếu server dùng chứng chỉ tự ký
                ws.SslConfiguration.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                ws.OnOpen += (sender, e) =>
                {
                    Console.WriteLine("Ket noi thanh cong!");
                    var obj = new
                    {
                        Event = 15,
                        Chanel = "dencanhbao_cd_dap"
                    };
                    string jsonData = JsonConvert.SerializeObject(obj);
                    ws.Send(jsonData);
                };

                ws.OnMessage += (sender, e) =>
                {
                    Console.WriteLine("Nhan du lieu: " + e.Data);
                };

                ws.OnError += (sender, e) =>
                {
                    Console.WriteLine("Loi: " + e.Message);
                };

                ws.OnClose += (sender, e) =>
                {
                    Console.WriteLine("Bi ngat ket noi!");
                };

                ws.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket Loi: {ex.Message}");
            }
        }
    }
}
