using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace asyncDataSoltec
{
    public partial class Form1 : Form
    {
        string file_path = ConfigurationManager.AppSettings["path_soltec"];
		string file_browse = ConfigurationManager.AppSettings["path_browse"];
		static string Machine = System.Environment.MachineName;
        public Form1()
        {
            InitializeComponent();
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
			// xử lý convert lưu file.data về server
			// xử lý convert lưu file.data về server
			// xử lý convert lưu file.data về server
			if (File.Exists(file_path))
			{
				FileInfo fileInfo = new FileInfo(file_path);
				DateTime now = DateTime.Now;
				if (fileInfo.LastWriteTime < now)
                {
					string __destinationPath = @"\\192.168.207.6\jtecdata\SYSTEM-JTECTOPICS\30. Data Transfer\10.Soltec Data\" + Machine; // Địa chỉ IP máy chủ + thư mục chia sẻ
					if (!Directory.Exists(__destinationPath))
					{
						Directory.CreateDirectory(__destinationPath); // Tạo thư mục nếu chưa có
					}	
					string destinationPath = @"\\192.168.207.6\jtecdata\SYSTEM-JTECTOPICS\30. Data Transfer\10.Soltec Data\" + Machine + "/" + fileInfo.LastWriteTime.ToString("dd_MM_yyyy") + ".dat";
					string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.LastWriteTime.ToString("HH_mm_ss_dd_MM_yyyy") + "_temp.dat");
					ConvertBinToDat(file_path, destFilePath);
					File.Copy(destFilePath, destinationPath, true);
					File.Delete(file_path);
					File.Delete(destFilePath);
				}
			}
		}

        private void timer1_Tick(object sender, EventArgs e)
        {
			if (File.Exists(file_path))
			{
				FileInfo fileInfo = new FileInfo(file_path);
				string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.LastWriteTime.ToString("HH_mm_dd_MM_yyyy") + "_temp.bin");
				File.Copy(file_path, destFilePath, true);
			}
		}

        private void timer2_Tick(object sender, EventArgs e)
        {
			DateTime currentDateTime = DateTime.Now;
            if (currentDateTime.Hour == 7)
            {
				// nạp dữ liệu vào DB
				asyncData();

				DateTime newDate = currentDateTime.AddDays(-29);
				string folderPath = Path.Combine(Directory.GetCurrentDirectory() + "/complete"); // Thư mục cần quét
				string extension = "*.bin"; // Định dạng file cần lấy (vd: *.txt, *.jpg, *.csv)
				string[] files = Directory.GetFiles(folderPath, extension);
				foreach (string file in files)
				{
					FileInfo fileInfo = new FileInfo(file);

					if (fileInfo.LastWriteTime.ToString("dd_MM_yyyy") == newDate.ToString("dd_MM_yyyy"))
					{
						File.Delete(file);
					}
				}
			}

		}
		private void asyncData()
		{
			// xử lý đồng bộ
			string folderPath = Path.Combine(Directory.GetCurrentDirectory()); // Thư mục cần quét
			string extension = "*.bin"; // Định dạng file cần lấy (vd: *.txt, *.jpg, *.csv)
			string[] files = Directory.GetFiles(folderPath, extension);
			HashSet<Record_st> set = new HashSet<Record_st>();
			DataAccess ac = new DataAccess();
			foreach (string file in files)
			{
				
				string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
				string csvFilePath = fileNameWithoutExt + ".dat"; // Output CSV file
				ConvertBinToDat(file, csvFilePath);
				// di chuyển file .bin vào thư mục complete
				string folderPath_bin_complete = Path.Combine(Directory.GetCurrentDirectory() + "/complete"); // Thư mục cần quét
				if (!Directory.Exists(folderPath_bin_complete))
				{
					Directory.CreateDirectory(folderPath_bin_complete); // Tạo thư mục nếu chưa có
				}
				if (!File.Exists(folderPath_bin_complete + "/" + fileNameWithoutExt + ".bin"))
				{
					File.Move(file, folderPath_bin_complete + "/" + fileNameWithoutExt + ".bin");
					try
					{

						FileStream fileStream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read);
						byte[] a = new byte[897];
						if (fileStream.Read(a, 0, a.Length) < 64)
						{
							continue;
						}
						byte[] b = Encoding.UTF8.GetBytes("CFMLHEAD");
						if (!IsMemcmp(ref a, ref b, 8))
						{
							continue;
						}
						if (BitConverter.ToUInt16(a, 8) != 64)
						{
							continue;
						}
						if (!((a[10] == 0) & (a[11] == 3)))
						{
							continue;
						}
						if (BitConverter.ToUInt16(a, 12) != 896)
						{
							continue;
						}
						int num = BitConverter.ToUInt16(a, 14);
						string fwVer = Encoding.ASCII.GetString(a, 16, 16).TrimEnd(default(char));
						string swVer = Encoding.UTF8.GetString(a, 32, 24).TrimEnd(default(char));
						fileStream.Seek(num, SeekOrigin.Begin);
						CRC32B cRC32B = new CRC32B();
						while (true)
						{
							Record_st record_st = new Record_st();
							record_st._tolerance = new short[8];
							if (fileStream.Read(a, 0, 896) < 896)
							{
								break;
							}
							b = Encoding.UTF8.GetBytes("CFMLRCRD");
							if (!IsMemcmp(ref a, ref b, 8))
							{
								continue;
							}
							uint num2 = BitConverter.ToUInt32(a, 892);
							if (cRC32B.Calc(a, 892) != num2)
							{
								continue;
							}
							record_st._no = BitConverter.ToUInt16(a, 8);
							record_st._date = new DateTime(2000 + a[10], a[11], a[12], a[13], a[14], a[15]);
							if ((a[16] == 0) | (a[16] == 1))
							{
								record_st._type = a[16];
								int num3 = 0;
								do
								{
									num3++;
								}
								while (num3 <= 199);
								int num4 = 0;
								do
								{
									num4++;
								}
								while (num4 <= 199);
								record_st._result = a[817];
								record_st._selTolerance = a[818];
								record_st._tolerance[0] = BitConverter.ToInt16(a, 819);
								record_st._tolerance[1] = BitConverter.ToInt16(a, 821);
								record_st._tolerance[2] = BitConverter.ToInt16(a, 823);
								record_st._tolerance[3] = BitConverter.ToInt16(a, 825);
								record_st._tolerance[4] = BitConverter.ToInt16(a, 827);
								record_st._tolerance[5] = BitConverter.ToInt16(a, 829);
								record_st._tolerance[6] = BitConverter.ToInt16(a, 831);
								record_st._cnt = a[833];
								record_st._max = a[834];
								record_st._warning = a[835];
								record_st._good = BitConverter.ToUInt32(a, 836);
								record_st._bad = BitConverter.ToUInt32(a, 840);
								record_st._peak = BitConverter.ToUInt16(a, 844);
								record_st._meas = BitConverter.ToUInt16(a, 846);
								record_st._T1 = BitConverter.ToInt16(a, 848);
								record_st._T2 = BitConverter.ToInt16(a, 850);
								record_st._T3 = BitConverter.ToInt16(a, 852);
								record_st._TD = BitConverter.ToUInt16(a, 854);
								record_st._shift = BitConverter.ToInt16(a, 856);
								record_st._L1 = a[858];
								record_st._L2 = a[859];
								record_st._L3 = a[860];
								record_st._L4 = a[861];
								record_st._MeasTimeMode = a[862];
								record_st._Trigger = a[863];
								record_st._TriggerLevel = BitConverter.ToInt16(a, 864);
								record_st._TriggerDelay = BitConverter.ToInt16(a, 866);
								record_st._AutoMeas = BitConverter.ToInt16(a, 868);
								record_st._Alignment = a[870];
								record_st._RightAt = a[871];
								record_st._LeftAt = a[872];
								record_st._Adaptive = a[873];
								record_st._Reset = a[874];
								record_st._T1Start = a[875];
								record_st._T2Start = a[876];
								record_st._T3Start = a[877];
								record_st._T3End = a[878];
								record_st._TeachSample = a[879];
								if (record_st._type == 0)
								{
									record_st._unit = a[880];
									record_st._calibration = BitConverter.ToUInt16(a, 881);
								}
								else
								{
									record_st._masterPeak = BitConverter.ToUInt16(a, 880);
									record_st._cpk = BitConverter.ToInt16(a, 882);
									record_st._unit = a[884];
									record_st._calibration = BitConverter.ToUInt16(a, 885);
								}
								set.Add(record_st);
								continue;
							}
							continue;
						}

						fileStream.Close();
					}
					catch (Exception ex)
					{
						continue;
					}
					File.Delete(file);
					File.Delete(csvFilePath);
                }
                else
                {
					continue;
				}
						
			}
			DateTime now = DateTime.Now;
			foreach (Record_st item in set)
            {
				string jsonString = JsonConvert.SerializeObject(item, Formatting.Indented);
				string InsertData = string.Format("INSERT INTO  [STOCKMANAGEMENT].[dbo].[ReportSeltec_DAP] ([data],[machine],[warning],[good],[bad],[peak],[meas],[T1],[T2],[T3],[TD],[shift],[created_at],[no],[data_date]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}');", jsonString, Machine, item._warning.ToString(), item._good.ToString(), item._bad.ToString(), item._peak.ToString(), item._meas.ToString(), item._T1.ToString(), item._T2.ToString(), item._T3.ToString(), item._TD.ToString(), item._shift.ToString(), now, item._no, item._date);
				ac.RunQuery(InsertData);
			}
			
			// MessageBox.Show(_destFilePath.Length.ToString());
			// MessageBox.Show(fileInfo.LastWriteTime.ToString("HH_mm_dd_MM_yyyy"));
			// MessageBox.Show(fileInfo.Length.ToString());
		}
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
		public static void ConvertBinToDat(string binFilePath, string datFilePath)
		{
			// Read the binary data from the .bin file
			byte[] binData = File.ReadAllBytes(binFilePath);

			// Optionally, process or modify the binary data if needed
			// For example, if you want to change the data structure or content, do it here

			// Write the binary data to a new .dat file
			File.WriteAllBytes(datFilePath, binData);
		}
		private static bool IsMemcmp(ref byte[] a, ref byte[] b, int size)
		{
			checked
			{
				bool result;
				try
				{
					int num = size - 1;
					int num2 = 0;
					while (true)
					{
						if (num2 <= num)
						{
							if (a[num2] != b[num2])
							{
								result = false;
								break;
							}
							num2++;
							continue;
						}
						result = true;
						break;
					}

				}
				catch (Exception ex)
				{
					result = false;
				}
				return result;
			}
		}
		public class CRC32B
		{
			private uint[] _baseTable;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <remarks>CRC-32テーブルの作成</remarks>
			public CRC32B()
			{
				_baseTable = new uint[256];
				uint num = 3988292384u;
				int num2 = 0;
				checked
				{
					do
					{
						uint num3 = (uint)num2;
						uint num4 = 0u;
						do
						{
							num3 = (((unchecked((ulong)num3) & 1uL) == 0L) ? (num3 >> 1) : ((num3 >> 1) ^ num));
							num4++;
						}
						while (num4 <= 7);
						_baseTable[num2] = num3;
						num2++;
					}
					while (num2 <= 255);
				}
			}

			/// <summary>
			/// CRC-32の算出(計算部のみ分割)
			/// </summary>
			/// <param name="Crc32">CRC-32値の受け取り</param>
			/// <param name="Buffer">対象データ</param>
			/// <param name="BufferLength">対象データのサイズ</param>
			/// <remarks></remarks>
			public void CalcDivide(ref uint Crc32, byte[] Buffer, int BufferLength)
			{
				checked
				{
					int num = BufferLength - 1;
					for (int i = 0; i <= num; i++)
					{
						byte b = (byte)(Crc32 & 0xFFu);
						byte b2 = unchecked((byte)(Buffer[i] ^ b));
						Crc32 = (Crc32 >> 8) ^ _baseTable[b2];
					}
				}
			}

			/// <summary>
			/// CRC-32の算出
			/// </summary>
			/// <returns>CRC-32値</returns>
			/// <param name="Buffer">対象データ</param>
			/// <param name="BufferLength">対象データのサイズ</param>
			/// <remarks></remarks>
			public uint Calc(byte[] Buffer, int BufferLength)
			{
				uint Crc = uint.MaxValue;
				CalcDivide(ref Crc, Buffer, BufferLength);
				return ~Crc;
			}
		}
		public class Record_st
		{
			public byte _type;

			public ushort _no;

			public DateTime _date;

			public ushort[] _standard;

			public ushort[] _wave;

			public byte _result;

			public byte _selTolerance;

			public short[] _tolerance;

			public byte _cnt;

			public byte _max;

			public byte _warning;

			public uint _good;

			public uint _bad;

			public ushort _peak;

			public ushort _meas;

			public short _T1;

			public short _T2;

			public short _T3;

			public ushort _TD;

			public short _shift;

			public byte _L1;

			public byte _L2;

			public byte _L3;

			public byte _L4;

			public byte _MeasTimeMode;

			public byte _Trigger;

			public short _TriggerLevel;

			public short _TriggerDelay;

			public short _AutoMeas;

			public byte _Alignment;

			public byte _RightAt;

			public byte _LeftAt;

			public byte _Adaptive;

			public byte _Reset;

			public byte _T1Start;

			public byte _T2Start;

			public byte _T3Start;

			public byte _T3End;

			public byte _TeachSample;

			public ushort _standardMax;

			public ushort _waveMax;

			public ushort _masterPeak;

			public short _cpk;

			public byte _unit;

			public ushort _calibration;

			public override string ToString() => $"_no: {_no}";

			public override bool Equals(object obj)
			{
				return obj is Record_st p && p._no == this._no && p._date == this._date;
			}

			public override int GetHashCode()
			{
				return _no.GetHashCode();
			}
		}

        private void label3_Click(object sender, EventArgs e)
        {

        }
		public static void loadDat(string path)
		{
				DataAccess ac = new DataAccess();
				FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
				byte[] a = new byte[897];
				if (fileStream.Read(a, 0, a.Length) < 64)
				{
					throw new Exception("ファイルサイズが規格より小さいです");
				}
				byte[] b = Encoding.UTF8.GetBytes("CFMLHEAD");
				if (!IsMemcmp(ref a, ref b, 8))
				{
					throw new Exception("ファイルヘッダーが規格外です(識別子)");
				}
				if (BitConverter.ToUInt16(a, 8) != 64)
				{
					throw new Exception("ファイルヘッダーが規格外です(ファイルヘッダーサイズ)");
				}
				if (!((a[10] == 0) & (a[11] == 3)))
				{
					throw new Exception("サポート外のファイルです(ファイルバージョン)");
				}
				if (BitConverter.ToUInt16(a, 12) != 896)
				{
					throw new Exception("ファイルヘッダーが規格外です(ファイルレコードサイズ)");
				}
				int num = BitConverter.ToUInt16(a, 14);
				string fwVer = Encoding.ASCII.GetString(a, 16, 16).TrimEnd(default(char));
				string swVer = Encoding.UTF8.GetString(a, 32, 24).TrimEnd(default(char));
				fileStream.Seek(num, SeekOrigin.Begin);
				CRC32B cRC32B = new CRC32B();
				while (true)
				{
					Record_st record_st = new Record_st();
					record_st._tolerance = new short[8];
					if (fileStream.Read(a, 0, 896) < 896)
					{
						break;
					}
					b = Encoding.UTF8.GetBytes("CFMLRCRD");
					if (!IsMemcmp(ref a, ref b, 8))
					{
						throw new Exception("ファイルレコードが規格外です(識別子)");
					}
					uint num2 = BitConverter.ToUInt32(a, 892);
					if (cRC32B.Calc(a, 892) != num2)
					{
						throw new Exception("ファイルレコードが規格外です(CRC不一致)");
					}
					record_st._no = BitConverter.ToUInt16(a, 8);
					record_st._date = new DateTime(2000 + a[10], a[11], a[12], a[13], a[14], a[15]);
					if ((a[16] == 0) | (a[16] == 1))
					{
						record_st._type = a[16];
						int num3 = 0;
						do
						{
							num3++;
						}
						while (num3 <= 199);
						int num4 = 0;
						do
						{
							num4++;
						}
						while (num4 <= 199);
						record_st._result = a[817];
						record_st._selTolerance = a[818];
						record_st._tolerance[0] = BitConverter.ToInt16(a, 819);
						record_st._tolerance[1] = BitConverter.ToInt16(a, 821);
						record_st._tolerance[2] = BitConverter.ToInt16(a, 823);
						record_st._tolerance[3] = BitConverter.ToInt16(a, 825);
						record_st._tolerance[4] = BitConverter.ToInt16(a, 827);
						record_st._tolerance[5] = BitConverter.ToInt16(a, 829);
						record_st._tolerance[6] = BitConverter.ToInt16(a, 831);
						record_st._cnt = a[833];
						record_st._max = a[834];
						record_st._warning = a[835];
						record_st._good = BitConverter.ToUInt32(a, 836);
						record_st._bad = BitConverter.ToUInt32(a, 840);
						record_st._peak = BitConverter.ToUInt16(a, 844);
						record_st._meas = BitConverter.ToUInt16(a, 846);
						record_st._T1 = BitConverter.ToInt16(a, 848);
						record_st._T2 = BitConverter.ToInt16(a, 850);
						record_st._T3 = BitConverter.ToInt16(a, 852);
						record_st._TD = BitConverter.ToUInt16(a, 854);
						record_st._shift = BitConverter.ToInt16(a, 856);
						record_st._L1 = a[858];
						record_st._L2 = a[859];
						record_st._L3 = a[860];
						record_st._L4 = a[861];
						record_st._MeasTimeMode = a[862];
						record_st._Trigger = a[863];
						record_st._TriggerLevel = BitConverter.ToInt16(a, 864);
						record_st._TriggerDelay = BitConverter.ToInt16(a, 866);
						record_st._AutoMeas = BitConverter.ToInt16(a, 868);
						record_st._Alignment = a[870];
						record_st._RightAt = a[871];
						record_st._LeftAt = a[872];
						record_st._Adaptive = a[873];
						record_st._Reset = a[874];
						record_st._T1Start = a[875];
						record_st._T2Start = a[876];
						record_st._T3Start = a[877];
						record_st._T3End = a[878];
						record_st._TeachSample = a[879];
						if (record_st._type == 0)
						{
							record_st._unit = a[880];
							record_st._calibration = BitConverter.ToUInt16(a, 881);
						}
						else
						{
							record_st._masterPeak = BitConverter.ToUInt16(a, 880);
							record_st._cpk = BitConverter.ToInt16(a, 882);
							record_st._unit = a[884];
							record_st._calibration = BitConverter.ToUInt16(a, 885);
						}
						//_list.Add(record_st);
						string jsonString = JsonConvert.SerializeObject(record_st, Formatting.Indented);
						DateTime now = DateTime.Now;
						string InsertData = string.Format("INSERT INTO  [STOCKMANAGEMENT].[dbo].[temp_report_seltec_dap] ([data],[machine],[warning],[good],[bad],[peak],[meas],[T1],[T2],[T3],[TD],[shift],[created_at],[no],[data_date]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}');", jsonString, Machine, record_st._warning.ToString(), record_st._good.ToString(), record_st._bad.ToString(), record_st._peak.ToString(), record_st._meas.ToString(), record_st._T1.ToString(), record_st._T2.ToString(), record_st._T3.ToString(), record_st._TD.ToString(), record_st._shift.ToString(), now, record_st._no, record_st._date);
						ac.RunQuery(InsertData);
						continue;
					}
					throw new Exception("ファイルレコードが規格外です(判定種別)");
				}

				fileStream.Close();
		}

        private void timer3_Tick(object sender, EventArgs e)
        {
			
			DataAccess _ac = new DataAccess();
			string query = $"DELETE FROM [STOCKMANAGEMENT].[dbo].[temp_report_seltec_dap] WHERE machine = '{Machine}'";
			string InsertData = string.Format(query);
			_ac.RunQuery(InsertData);
			if (File.Exists(file_path))
			{
				string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "/temp");
				if (!Directory.Exists(destFilePath))
				{
					Directory.CreateDirectory(destFilePath); // Tạo thư mục nếu chưa có
				}
				destFilePath = Path.Combine(Directory.GetCurrentDirectory(), "temp/temp.bin");
				File.Copy(file_path, destFilePath, true);
				string csvFilePath = "/temp/output.dat"; // Output CSV file
				ConvertBinToDat(destFilePath, csvFilePath);
				loadDat(csvFilePath);
				File.Delete(csvFilePath);
			}

		}
		private DateTime GetDateByFileName(string file_path)
		{
			string fileName = Path.GetFileName(file_path);
			string[] words = fileName.Split('_');
			return DateTime.Parse(words[4]+"-"+ words[3] + "-"+ words[2] + " "+ words[0] + ":"+ words[1]);
		}
		private string GetFileNameByFullDate(DateTime date)
		{
			return date.ToString("HH_mm_dd_MM_yyyy") + "_temp.bin";
		}
		private string GetFileNameByDate(DateTime date)
		{
			return date.ToString("dd_MM_yyyy") + "_temp.bin";
		}

        private void button1_Click(object sender, EventArgs e)
        {
			OpenFileDialog openFileDialog1 = new OpenFileDialog
			{
				InitialDirectory = Environment.CurrentDirectory+ "\\complete",
				Title = "Browse Text Files",

				CheckFileExists = true,
				CheckPathExists = true,

				DefaultExt = "bin",
				Filter = "bin files (*.bin)|*.bin",
				FilterIndex = 2,
				RestoreDirectory = true,

				ReadOnlyChecked = true,
				ShowReadOnly = true
			};

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				txtFilePath.Text = openFileDialog1.FileName;
				string file = openFileDialog1.FileName;
				FileInfo fileInfo = new FileInfo(file);
				string destinationPath = @"\\192.168.207.6\jtecdata\SYSTEM-JTECTOPICS\30. Data Transfer\10.Soltec Data\" + Machine + "/" + fileInfo.LastWriteTime.ToString("dd_MM_yyyy") + ".dat";
				string destFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.LastWriteTime.ToString("HH_mm_ss_dd_MM_yyyy") + "_temp.dat");
				ConvertBinToDat(file, destFilePath);
				File.Copy(destFilePath, destinationPath, true);
				File.Delete(destFilePath);
				MessageBox.Show("Đã lấy dữ liệu về server thành công. Đường dẫn: "+ destinationPath);
			}
		}
		public void checkUpdate()
		{
			try
			{
				#region Update chuong trinh moi (Neu co)
				//Đọc file PathUpdate.txt để lấy đường dẫn
				StreamReader strReader = new StreamReader(File.OpenRead(Directory.GetCurrentDirectory() + @"\PathUpdate.txt"));
				string pathsource = strReader.ReadLine();
				strReader.Close();
				string[] getIp = pathsource.Split('\\');
				if (PingToAddress(getIp[2]))
				{
					string datemodifypathsource = "";
					string datemodifypathdes = "";
					//ktra datemodify chuong trinh tren server
					DateTime modifysource = File.GetLastWriteTime(pathsource);
					datemodifypathsource = modifysource.ToString();
					//ktra chuong trinh tai may client
					//Lay tên chuong trinh hien tai
					string nameprogram = "";
					DirectoryInfo diDirectoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
					FileInfo[] fileInfo = diDirectoryInfo.GetFiles("*.exe");
					//Duyet cac file
					for (int i = 0; i < fileInfo.Length; i++)
					{
						if (fileInfo[i].Name.ToString() != "AutoUpdate.exe")
						{
							nameprogram = fileInfo[i].Name.ToString();
							break;
						}
					}
					DateTime modifydes = File.GetLastWriteTime(Directory.GetCurrentDirectory() + @"\" + nameprogram);
					datemodifypathdes = modifydes.ToString();
					if (modifysource > modifydes)//Neu datemodify khác nhau thì chay chuong trình copy file lên và thoát chuong trình
					{
						DialogResult dialogResult = MessageBox.Show("Đã có phiên bản mới, bạn có muốn cập nhật?", "Cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
						if (dialogResult == DialogResult.Yes)
						{
							////chay chuong trình copy
							System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + @"\" + "AutoUpdate.exe");
							Application.Exit();
							foreach (Process clsProcess in Process.GetProcesses())
							{
							    if (clsProcess.ProcessName.Equals("asyncDataSoltec"))
							    {
							        clsProcess.Kill();
							        break;
							    }
							}
						}
					}
				}
				#endregion
			}
			catch
			{

			}
		}
		public static bool PingToAddress(string IP)
		{
			try
			{
				System.Net.NetworkInformation.Ping PingSender = new System.Net.NetworkInformation.Ping();
				int TimeOut = 120;
				string PingData = "aaaa";
				byte[] Buffer = System.Text.Encoding.ASCII.GetBytes(PingData);
				PingReply PingReply = PingSender.Send(IP, TimeOut, Buffer);
				if (PingReply.Status == IPStatus.Success)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch { return false; }
		}

        private void Form1_Load(object sender, EventArgs e)
        {
			checkUpdate();
		}
    }
}
