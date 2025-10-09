using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdamLibrary;
using LampWarningAgvDap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace LampWarningAgvDap
{
    public partial class Form1 : Form
    {
		private AdvantechClient __AdvantechClient = new AdvantechClient();

		private WebSocket ws = new WebSocket("wss://192.168.207.6:5007/ws");

		private bool lamp_1 = false;

		private bool lamp_2 = false;

		private bool lamp_3 = false;

		private bool lamp_4 = false;

		private bool random = false;

		private int mode = 0;

		private int lamp_1_count = 0;

		private int lamp_2_count = 0;

		private int lamp_3_count = 0;

		private int lamp_4_count = 0;

		private string chanel = "dencanhbao_cd_dap";

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AllocConsole();

		public Form1()
		{
			InitializeComponent();
			Rectangle workingArea = Screen.GetWorkingArea(this);
			base.Location = new Point(workingArea.Right - base.Size.Width, workingArea.Bottom - base.Size.Height);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.WindowState = FormWindowState.Minimized;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (base.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			label2.Text = chanel;
			bool isCOM6 = false;
			string[] PortName = SerialPort.GetPortNames();
			if (PortName.Length != 0)
			{
				string[] array = PortName;
				foreach (string portname in array)
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
			Show();
			base.WindowState = FormWindowState.Normal;
		}

		private void connectSocket()
		{
			try
			{
				Console.WriteLine("Dang ket noi toi WebSocket server...");
				ws.OnOpen += delegate
				{
					Console.WriteLine("Ket noi thanh cong!");
					var value = new
					{
						Event = 15,
						Chanel = chanel
					};
					string data = JsonConvert.SerializeObject(value);
					ws.Send(data);
				};
				ws.OnMessage += delegate (object sender, MessageEventArgs e)
				{
					HandleOnMessage(sender, e);
				};
				ws.OnError += delegate (object sender, ErrorEventArgs e)
				{
					Console.WriteLine(e.Message);
				};
				ws.OnClose += delegate
				{
					Console.WriteLine("Bi Ngat Ket Noi!");
				};
				ws.Connect();
			}
			catch (Exception ex)
			{
				Console.WriteLine("WebSocket Loi: " + ex.Message);
				MessageBox.Show("Không kết nối được Socket?", "Lỗi!!");
				Application.Exit();
			}
		}

		private void HandleOnMessage(object sender, MessageEventArgs e)
		{
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(e.Data);
				if (!((PropertyExists(obj, "chanel") && obj.chanel == chanel) ? true : false))
				{
					return;
				}
				if (PropertyExists(obj, "messageText") && obj.messageText == "den1")
				{
					lamp_1 = obj.status;
					mode = obj.mode;
					Task.Delay(500);
					__AdvantechClient.WriteDO(1, 5, lamp_1);
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
					Task.Delay(500);
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
					Task.Delay(500);
					__AdvantechClient.WriteDO(1, 3, lamp_3);
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
					Task.Delay(500);
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
			catch (Exception)
			{
			}
		}

		private void timer2_Tick(object sender, EventArgs e)
		{
			try
			{
				if (!ws.Ping())
				{
					ws.Connect();
				}
			}
			catch (Exception)
			{
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{
				if (lamp_1)
				{
					lamp_1_count++;
					if (lamp_1_count == 600)
					{
						lamp_1_count = 0;
						lamp_1 = false;
						__AdvantechClient.WriteDO(1, 5, state: false);
					}
					if (mode == 1)
					{
						if (__AdvantechClient.ReadDO(1, 5))
						{
							__AdvantechClient.WriteDO(1, 5, state: false);
						}
						else
						{
							__AdvantechClient.WriteDO(1, 5, state: true);
						}
					}
					else
					{
						__AdvantechClient.WriteDO(1, 5, state: true);
					}
				}
				if (lamp_2)
				{
					lamp_2_count++;
					if (lamp_2_count == 600)
					{
						lamp_2_count = 0;
						lamp_2 = false;
						__AdvantechClient.WriteDO(1, 4, state: false);
					}
					if (mode == 1)
					{
						if (__AdvantechClient.ReadDO(1, 4))
						{
							__AdvantechClient.WriteDO(1, 4, state: false);
						}
						else
						{
							__AdvantechClient.WriteDO(1, 4, state: true);
						}
					}
					else
					{
						__AdvantechClient.WriteDO(1, 4, state: true);
					}
				}
				if (lamp_3)
				{
					lamp_3_count++;
					if (lamp_3_count == 600)
					{
						lamp_3_count = 0;
						lamp_3 = false;
						__AdvantechClient.WriteDO(1, 3, state: false);
					}
					if (mode == 1)
					{
						if (__AdvantechClient.ReadDO(1, 3))
						{
							button3.BackColor = Color.LightGray;
							__AdvantechClient.WriteDO(1, 3, state: false);
						}
						else
						{
							button3.BackColor = Color.Lime;
							__AdvantechClient.WriteDO(1, 3, state: true);
						}
					}
					else
					{
						__AdvantechClient.WriteDO(1, 3, state: true);
					}
				}
				if (!lamp_4)
				{
					return;
				}
				lamp_4_count++;
				if (lamp_4_count == 600)
				{
					lamp_4_count = 0;
					lamp_4 = false;
					__AdvantechClient.WriteDO(1, 6, state: false);
				}
				if (mode == 1)
				{
					if (__AdvantechClient.ReadDO(1, 6))
					{
						button4.BackColor = Color.LightGray;
						__AdvantechClient.WriteDO(1, 6, state: false);
					}
					else
					{
						button4.BackColor = Color.Lime;
						__AdvantechClient.WriteDO(1, 6, state: true);
					}
				}
				else
				{
					__AdvantechClient.WriteDO(1, 6, state: true);
				}
			}
			catch (Exception)
			{
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Task.Delay(500);
			if (lamp_1)
			{
				button1.BackColor = Color.LightGray;
				lamp_1 = false;
				__AdvantechClient.WriteDO(1, 5, state: false);
			}
			else
			{
				button1.BackColor = Color.Lime;
				lamp_1 = true;
				__AdvantechClient.WriteDO(1, 5, state: true);
			}
			var obj = new
			{
				Event = 15,
				Chanel = chanel,
				Status = (lamp_1 ? 1 : 0),
				messageText = "den1"
			};
			string jsonData = JsonConvert.SerializeObject(obj);
			ws.Send(jsonData);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Task.Delay(500);
			if (lamp_2)
			{
				button2.BackColor = Color.LightGray;
				lamp_2 = false;
				__AdvantechClient.WriteDO(1, 4, state: false);
			}
			else
			{
				button2.BackColor = Color.Lime;
				lamp_2 = true;
				__AdvantechClient.WriteDO(1, 4, state: true);
			}
			var obj = new
			{
				Event = 15,
				Chanel = chanel,
				Status = (lamp_2 ? 1 : 0),
				messageText = "den2"
			};
			string jsonData = JsonConvert.SerializeObject(obj);
			ws.Send(jsonData);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			Task.Delay(500);
			if (lamp_3)
			{
				button3.BackColor = Color.LightGray;
				lamp_3 = false;
				__AdvantechClient.WriteDO(1, 3, state: false);
			}
			else
			{
				button3.BackColor = Color.Lime;
				lamp_3 = true;
				__AdvantechClient.WriteDO(1, 3, state: true);
			}
			var obj = new
			{
				Event = 15,
				Chanel = chanel,
				Status = (lamp_3 ? 1 : 0),
				messageText = "den3"
			};
			string jsonData = JsonConvert.SerializeObject(obj);
			ws.Send(jsonData);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			Task.Delay(500);
			if (lamp_4)
			{
				button4.BackColor = Color.LightGray;
				lamp_4 = false;
				__AdvantechClient.WriteDO(1, 6, state: false);
			}
			else
			{
				button4.BackColor = Color.Lime;
				lamp_4 = true;
				__AdvantechClient.WriteDO(1, 6, state: true);
			}
			var obj = new
			{
				Event = 15,
				Chanel = chanel,
				Status = (lamp_4 ? 1 : 0),
				messageText = "den4"
			};
			string jsonData = JsonConvert.SerializeObject(obj);
			ws.Send(jsonData);
		}

		public static bool PropertyExists(dynamic obj, string name)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is ExpandoObject)
			{
				return ((IDictionary<string, object>)obj).ContainsKey(name);
			}
			if (obj is IDictionary<string, object> dict1)
			{
				return dict1.ContainsKey(name);
			}
			if (obj is IDictionary<string, JToken> dict2)
			{
				return dict2.ContainsKey(name);
			}
			return obj.GetType().GetProperty(name) != null;
		}

	}
}
