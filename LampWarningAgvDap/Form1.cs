using AdamLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace LampWarningAgvDap
{
    public partial class Form1 : Form
    {
        //Khởi tạo Adam

        AdvantechClient __AdvantechClient = new AdvantechClient();

        //Khởi tạo Websocket

        WebSocket ws = new WebSocket("wss://192.168.217.76:5007/ws");

        bool lamp_1 = false;
        bool lamp_2 = false;
        bool lamp_3 = false;
        bool lamp_4 = false;
        bool random = false;
        int mode = 0;

        string chanel = "dencanhbao_cd_dap";

        // Import the AllocConsole function
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        public Form1()
        {
            InitializeComponent();
            //AllocConsole();
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // button2.BackColor = Color.Lime;
            label2.Text = chanel;
            bool isCOM6 = false;
            string[] PortName = SerialPort.GetPortNames();
            if (PortName.Length > 0)
            {
                foreach (string portname in PortName)
                {
                    if (portname == "COM6")
                    {
                        isCOM6 = true;
                        break;
                    }
                }
            }
            if (!isCOM6)
            {
                MessageBox.Show("Không tìm thấy cổng COM6, thoát chương trình?", "Lỗi!!");
                Application.Exit();
            }
            else
            {
                __AdvantechClient.PortName = "COM6";
                __AdvantechClient.DataBits = 8;
                __AdvantechClient.Open();
                if (!__AdvantechClient.IsOpen)
                {
                    MessageBox.Show("Không tìm kết nối được module ADAM, thoát chương trình?", "Lỗi!!");
                    Application.Exit();
                }
            }

            connectSocket();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        private void connectSocket()
        {
            try
            {
                // Kết nối tới WebSocket server

                Console.WriteLine("Dang ket noi toi WebSocket server...");

                ws.OnOpen += (sender, e) =>
                {
                    Console.WriteLine("Ket noi thanh cong!");
                    var obj = new
                    {
                        Event = 15,
                        Chanel = chanel
                    };
                    string jsonData = JsonConvert.SerializeObject(obj);
                    ws.Send(jsonData);
                };

                ws.OnMessage += (sender, e) => HandleOnMessage(sender, e);

                ws.OnError += (sender, e) => {
                    Console.WriteLine(e.Message);
                };

                ws.OnClose += (sender, e) => {
                    Console.WriteLine("Bi Ngat Ket Noi!");
                };

                ws.Connect();

               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket Loi: {ex.Message}");
                MessageBox.Show("Không kết nối được Socket?", "Lỗi!!");
                Application.Exit();
            }
        }
        private void HandleOnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(e.Data);
           
                if (PropertyExists(obj, "chanel") && obj.chanel == chanel)
                {
                    if (PropertyExists(obj, "messageText") && obj.messageText == "den1")
                    {
                        lamp_1 = obj.status;
                        mode = obj.mode;
                        __AdvantechClient.WriteDO(1, 3, lamp_1);
                        if (lamp_1)
                        {
                            button1.BackColor = Color.Lime;
                        }
                        else
                        {
                            button1.BackColor = Color.LightGray;
                        }
                    }
                    if (PropertyExists(obj, "messageText") && obj.messageText == "den2")
                    {
                        lamp_2 = obj.status;
                        mode = obj.mode;
                        __AdvantechClient.WriteDO(1, 4, lamp_2);
                        if (lamp_2)
                        {
                            button2.BackColor = Color.Lime;
                        }
                        else
                        {
                            button2.BackColor = Color.LightGray;
                        }
                    }
                    if (PropertyExists(obj, "messageText") && obj.messageText == "den3")
                    {
                        lamp_3 = obj.status;
                        mode = obj.mode;
                        __AdvantechClient.WriteDO(1, 5, lamp_3);
                        if (lamp_3)
                        {
                            button3.BackColor = Color.Lime;
                        }
                        else
                        {
                            button3.BackColor = Color.LightGray;
                        }
                    }
                    if (PropertyExists(obj, "messageText") && obj.messageText == "den4")
                    {
                        lamp_4 = obj.status;
                        mode = obj.mode;
                        __AdvantechClient.WriteDO(1, 6, lamp_4);
                        if (lamp_4)
                        {
                            button4.BackColor = Color.Lime;
                        }
                        else
                        {
                            button4.BackColor = Color.LightGray;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private void timer2_Tick(object sender, EventArgs e)// 1000ms
        {
            bool check_connect = ws.Ping();
            if (!check_connect)
            {
                ws.Connect();
            }
           // Console.WriteLine("Check connect: "+check_connect);


        }

        private void timer1_Tick(object sender, EventArgs e)// 900ms
        {
            try
            {
                if (lamp_1 == true)
                {
                    if (mode == 1)
                    {
                        bool check_status = __AdvantechClient.ReadDO(1, 3);
                        if (check_status)
                        {
                            __AdvantechClient.WriteDO(1, 3, false);
                        }
                        else
                        {
                            __AdvantechClient.WriteDO(1, 3, true);
                        }
                    }
                    else
                    {
                        __AdvantechClient.WriteDO(1, 3, true);
                    }
               
                }
                if (lamp_2 == true)
                {
                    if (mode == 1)
                    {
                        bool check_status = __AdvantechClient.ReadDO(1, 4);
                        if (check_status)
                        {
                            __AdvantechClient.WriteDO(1, 4, false);
                        }
                        else
                        {
                            __AdvantechClient.WriteDO(1, 4, true);
                        }
                    }
                    else
                    {
                        __AdvantechClient.WriteDO(1, 4, true);
                    }
                }
                if (lamp_3 == true)
                {
                    if (mode == 1)
                    {
                        bool check_status = __AdvantechClient.ReadDO(1, 5);
                        if (check_status)
                        {
                            button3.BackColor = Color.LightGray;
                            __AdvantechClient.WriteDO(1, 5, false);
                        }
                        else
                        {
                            button3.BackColor = Color.Lime;
                            __AdvantechClient.WriteDO(1,5, true);
                        }
                    }
                    else
                    {
                        __AdvantechClient.WriteDO(1, 5, true);
                    }
                }
                if (lamp_4 == true)
                {
                    if (mode == 1)
                    {
                        bool check_status = __AdvantechClient.ReadDO(1, 6);
                        if (check_status)
                        {
                            button4.BackColor = Color.LightGray;
                            __AdvantechClient.WriteDO(1, 6, false);
                        }
                        else
                        {
                            button4.BackColor = Color.Lime;
                            __AdvantechClient.WriteDO(1, 6, true);
                        }
                    }
                    else
                    {
                        __AdvantechClient.WriteDO(1, 6, true);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lamp_1 == true)
            {
                button1.BackColor = Color.LightGray;
                lamp_1 = false;
                __AdvantechClient.WriteDO(1, 3, false);
            }
            else
            {
                button1.BackColor = Color.Lime;
                lamp_1 = true;
                __AdvantechClient.WriteDO(1, 3, true);
            }
            var obj = new
            {
                Event = 15,
                Chanel = chanel,
                Status = lamp_1 ? 1 : 0,
                messageText = "den1"
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            ws.Send(jsonData);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lamp_2 == true)
            {
                button2.BackColor = Color.LightGray;
                lamp_2 = false;
                __AdvantechClient.WriteDO(1, 4, false);
            }
            else
            {
                button2.BackColor = Color.Lime;
                lamp_2 = true;
                __AdvantechClient.WriteDO(1, 4, true);
            }
            var obj = new
            {
                Event = 15,
                Chanel = chanel,
                Status = lamp_2 ? 1 : 0,
                messageText = "den2"
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            ws.Send(jsonData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (lamp_3 == true)
            {
                button3.BackColor = Color.LightGray;
                lamp_3 = false;
                __AdvantechClient.WriteDO(1, 5, false);
            }
            else
            {
                button3.BackColor = Color.Lime;
                lamp_3 = true;
                __AdvantechClient.WriteDO(1, 5, true);
            }
            var obj = new
            {
                Event = 15,
                Chanel = chanel,
                Status = lamp_3 ? 1 : 0,
                messageText = "den3"
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            ws.Send(jsonData);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (lamp_4 == true)
            {
                button4.BackColor = Color.LightGray;
                lamp_4 = false;
                __AdvantechClient.WriteDO(1, 6, false);
            }
            else
            {
                button4.BackColor = Color.Lime;
                lamp_4 = true;
                __AdvantechClient.WriteDO(1, 6, true);
            }
            var obj = new
            {
                Event = 15,
                Chanel = chanel,
                Status = lamp_4 ? 1 : 0,
                messageText = "den4"
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            ws.Send(jsonData);
        }
        public static bool PropertyExists(dynamic obj, string name)
        {
            if (obj == null) return false;
            if (obj is ExpandoObject)
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            if (obj is IDictionary<string, object> dict1)
                return dict1.ContainsKey(name);
            if (obj is IDictionary<string, JToken> dict2)
                return dict2.ContainsKey(name);
            return obj.GetType().GetProperty(name) != null;
        }
    }
}
