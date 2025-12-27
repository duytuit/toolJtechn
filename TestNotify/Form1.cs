using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestNotify
{
    public partial class Form1 : Form
    { 
          private WebSocketClient _client;
         private CancellationTokenSource _cts;
        public Form1()
        {
            InitializeComponent();
            connectSocket();
        }
        private async void connectSocket()
        {

            _cts = new CancellationTokenSource();
            _client = new WebSocketClient("wss://192.168.207.6:5007/ws");

            //_client.OnLog += (msg) => Invoke((Action)(() => richTextBox1.AppendText(msg + "\n")));
            _client.OnMessageReceived += (msg) => Invoke((Action)(() => {
               
            }));
            // _client.OnConnected += () => Invoke((Action)(() => labelStatus.Text = "🟢 Connected"));
            // _client.OnDisconnected += () => Invoke((Action)(() => labelStatus.Text = "🔴 Disconnected"));

            await _client.StartAsync(_cts.Token);
            var obj = new
            {
                Event = 15,
                Chanel = "dencanhbao_cd_dap",
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            await _client.SendMessageAsync(jsonData, _cts.Token);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://192.168.207.6:8088/storeDataUmesenVer2"
            );

            //request.Headers.Add(
            //    "Cookie",
            //    "jtec_hn_session=Iu0KAvgUUujYJjlAQdnTUxckO3EJJPFyxrLX0VKa"
            //);

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("content", "@2025-12-26 13:44:08|...|...|...|...|...|...|...|...|0|0"));
            collection.Add(new KeyValuePair<string, string>("arrayImage", "b"));

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var obj = new
            {
                Event = 15,
                Chanel = "dencanhbao_cd_dap",
                MessageText = JsonConvert.SerializeObject(new
                {
                    job_id = 123,
                    app = "task",
                    job_name = "34ACVEG6",
                    code = "240404",
                    link = "http://192.168.207.6:8088/admin",
                    status = 1
                })
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            await _client.SendMessageAsync(jsonData, _cts.Token);
        }
    }
}
