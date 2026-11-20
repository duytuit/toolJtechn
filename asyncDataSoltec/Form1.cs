using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.Globalization;

namespace asyncDataSoltec
{
    public partial class Form1 : Form
    {
        string file_path = ConfigurationManager.AppSettings["path_soltec"];
		string file_browse = ConfigurationManager.AppSettings["path_browse"];
		static string Machine = System.Environment.MachineName;
        private readonly SemaphoreSlim _syncGate = new SemaphoreSlim(1, 1);
		private int _snapshotInProgress;

        public Form1()
        {
            InitializeComponent();
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.WindowState = FormWindowState.Minimized;
		}

        private async void timer1_Tick(object sender, EventArgs e)
        {
		    if (Interlocked.Exchange(ref _snapshotInProgress, 1) != 0)
		        return;

		    try
		    {
		        await Task.Run(() =>
		        {
		            CaptureDataSnapshot();
		            UploadPendingFileToServer();
		        });
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show("Lỗi sao lưu file dữ liệu: " + ex.Message, "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
		    }
		    finally
		    {
		        Interlocked.Exchange(ref _snapshotInProgress, 0);
		    }
        }

        private void CaptureDataSnapshot()
        {
		    if (string.IsNullOrWhiteSpace(file_path) || !File.Exists(file_path))
		        return;

		    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
		    string temporaryPath = Path.Combine(appDirectory, Guid.NewGuid().ToString("N") + ".tmp");
		    try
		    {
		        DateTime sourceLastWriteTime;
		        if (!CopyStableFile(file_path, temporaryPath, out sourceLastWriteTime))
		            return;

		        string baseName = sourceLastWriteTime.ToString("HH_mm_dd_MM_yyyy", CultureInfo.InvariantCulture) + "_temp";
		        string destinationPath = Path.Combine(appDirectory, baseName + ".bin");
		        if (File.Exists(destinationPath) && FilesAreEqual(temporaryPath, destinationPath))
		            return;

		        int suffix = 1;
		        while (File.Exists(destinationPath))
		        {
		            destinationPath = Path.Combine(appDirectory, baseName + "_" + suffix + ".bin");
		            suffix++;
		        }

		        File.Move(temporaryPath, destinationPath);
		    }
		    finally
		    {
		        if (File.Exists(temporaryPath))
		            File.Delete(temporaryPath);
		    }
		}

		private static bool FilesAreEqual(string firstPath, string secondPath)
		{
		    FileInfo firstInfo = new FileInfo(firstPath);
		    FileInfo secondInfo = new FileInfo(secondPath);
		    if (firstInfo.Length != secondInfo.Length)
		        return false;

		    using (FileStream first = File.OpenRead(firstPath))
		    using (FileStream second = File.OpenRead(secondPath))
		    {
		        byte[] firstBuffer = new byte[81920];
		        byte[] secondBuffer = new byte[81920];
		        int firstRead;
		        while ((firstRead = first.Read(firstBuffer, 0, firstBuffer.Length)) > 0)
		        {
		            int secondRead = second.Read(secondBuffer, 0, secondBuffer.Length);
		            if (firstRead != secondRead || !firstBuffer.Take(firstRead).SequenceEqual(secondBuffer.Take(secondRead)))
		                return false;
		        }

		        return second.ReadByte() == -1;
		    }
		}

		private async void timer2_Tick(object sender, EventArgs e)
        {
			DateTime currentDateTime = DateTime.Now;
		    if (currentDateTime.Hour != 7 || !_syncGate.Wait(0))
		        return;

		    try
            {
		        await Task.Run(() => asyncData());

		        DateTime cutoffDateTime = currentDateTime.AddDays(-7);
		        string archiveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "complete");
		        if (Directory.Exists(archiveDirectory))
		        {
		            foreach (string file in Directory.GetFiles(archiveDirectory, "*.bin"))
		            {
		                if (File.GetLastWriteTime(file) < cutoffDateTime)
		                    File.Delete(file);
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show("Lỗi đồng bộ dữ liệu: " + ex.Message, "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
		    }
		    finally
		    {
		        _syncGate.Release();
		    }

		}
		private void asyncData()
		{
			// xử lý đồng bộ
			string folderPath = AppDomain.CurrentDomain.BaseDirectory;
			string[] files = Directory.GetFiles(folderPath, "*.bin", SearchOption.TopDirectoryOnly);
			HashSet<Record_st> set = new HashSet<Record_st>();
			List<string> parsedFiles = new List<string>();
			List<string> failures = new List<string>();
			string folderPath_bin_complete = Path.Combine(folderPath, "complete");
			Directory.CreateDirectory(folderPath_bin_complete);
			DataAccess ac = new DataAccess();
			foreach (string file in files)
			{
				string csvFilePath = Path.Combine(folderPath, Guid.NewGuid().ToString("N") + ".dat");
				if (File.Exists(file))
				{
					HashSet<Record_st> fileRecords = new HashSet<Record_st>();
					FileStream fileStream = null;
					try
					{
						ConvertBinToDat(file, csvFilePath);
						fileStream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read);
						byte[] a = new byte[897];
						if (!ReadExactly(fileStream, a, 64))
							throw new InvalidDataException("File không có header đầy đủ.");
						byte[] b = Encoding.UTF8.GetBytes("CFMLHEAD");
						if (!IsMemcmp(ref a, ref b, 8))
							throw new InvalidDataException("Header file không hợp lệ.");
						if (BitConverter.ToUInt16(a, 8) != 64)
							throw new InvalidDataException("Kích thước header không hợp lệ.");
						if (a[10] != 0 || a[11] != 3)
							throw new InvalidDataException("Phiên bản file không được hỗ trợ.");
						if (BitConverter.ToUInt16(a, 12) != 896)
							throw new InvalidDataException("Kích thước record không hợp lệ.");
						int num = BitConverter.ToUInt16(a, 14);
						if (num < 64 || num > fileStream.Length)
							throw new InvalidDataException("Vị trí bắt đầu record không hợp lệ.");
						fileStream.Seek(num, SeekOrigin.Begin);
						CRC32B cRC32B = new CRC32B();
						while (true)
						{
							Record_st record_st = new Record_st();
							record_st._tolerance = new short[8];
							if (!ReadExactly(fileStream, a, 896))
								break;
							b = Encoding.UTF8.GetBytes("CFMLRCRD");
							if (!IsMemcmp(ref a, ref b, 8))
								throw new InvalidDataException("Record không hợp lệ.");
							uint num2 = BitConverter.ToUInt32(a, 892);
							if (cRC32B.Calc(a, 892) != num2)
								throw new InvalidDataException("CRC record không hợp lệ.");
							record_st._no = BitConverter.ToUInt16(a, 8);
							record_st._date = new DateTime(2000 + a[10], a[11], a[12], a[13], a[14], a[15]);
							if (a[16] != 0 && a[16] != 1)
								throw new InvalidDataException("Loại record không được hỗ trợ.");
							if (a[16] == 0 || a[16] == 1)
							{
								record_st._type = a[16];
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
								fileRecords.Add(record_st);
								continue;
							}
						}

						set.UnionWith(fileRecords);
						parsedFiles.Add(file);
					}
					catch (Exception ex)
					{
						failures.Add(Path.GetFileName(file) + ": " + ex.Message);
						continue;
					}
					finally
					{
						if (fileStream != null)
							fileStream.Dispose();
						if (File.Exists(csvFilePath))
							File.Delete(csvFilePath);
					}
                }
                else
                {
					continue;
				}
						
			}
			DateTime now = DateTime.Now;
			var distinctSet = new HashSet<Record_st>(
				set.GroupBy(r => new { r._no, r._date })
				   .Select(g => g.Last())
			);
			if (distinctSet.Count > 0)
			{
				ac.ExecuteTransaction((connection, transaction) =>
				{
					foreach (Record_st item in distinctSet)
						UpsertRecord(connection, transaction, "[STOCKMANAGEMENT].[dbo].[ReportSeltec_DAP]", item, now);
				});
			}

			foreach (string parsedFile in parsedFiles)
				ArchiveFile(parsedFile, folderPath_bin_complete);

			if (failures.Count > 0)
				throw new IOException("Một số file chưa đồng bộ thành công:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
		}

        private static void UpsertRecord(SqlConnection connection, SqlTransaction transaction, string tableName, Record_st record, DateTime syncDate)
        {
            string sql = @"
IF EXISTS (
    SELECT 1
    FROM " + tableName + @" WITH (UPDLOCK, HOLDLOCK)
    WHERE machine = @machine AND [no] = @no AND created_at = @recordDate
)
    UPDATE " + tableName + @"
    SET [data] = @data, warning = @warning, good = @good, bad = @bad, peak = @peak,
        meas = @meas, T1 = @T1, T2 = @T2, T3 = @T3, TD = @TD, shift = @shift, data_date = @syncDate
    WHERE machine = @machine AND [no] = @no AND created_at = @recordDate;
ELSE
    INSERT INTO " + tableName + @" ([data], [machine], [warning], [good], [bad], [peak], [meas],
        [T1], [T2], [T3], [TD], [shift], [data_date], [no], [created_at])
    VALUES (@data, @machine, @warning, @good, @bad, @peak, @meas,
        @T1, @T2, @T3, @TD, @shift, @syncDate, @no, @recordDate);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 1800;
                command.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(record, Formatting.Indented));
                command.Parameters.AddWithValue("@machine", Machine);
                command.Parameters.AddWithValue("@warning", record._warning);
                command.Parameters.AddWithValue("@good", record._good);
                command.Parameters.AddWithValue("@bad", record._bad);
                command.Parameters.AddWithValue("@peak", record._peak);
                command.Parameters.AddWithValue("@meas", record._meas);
                command.Parameters.AddWithValue("@T1", record._T1);
                command.Parameters.AddWithValue("@T2", record._T2);
                command.Parameters.AddWithValue("@T3", record._T3);
                command.Parameters.AddWithValue("@TD", record._TD);
                command.Parameters.AddWithValue("@shift", record._shift);
                command.Parameters.AddWithValue("@syncDate", syncDate);
                command.Parameters.AddWithValue("@no", record._no);
                command.Parameters.AddWithValue("@recordDate", record._date);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertRecord(SqlConnection connection, SqlTransaction transaction, Record_st record, DateTime syncDate)
        {
            const string sql = @"
INSERT INTO [STOCKMANAGEMENT].[dbo].[temp_report_seltec_dap]
    ([data], [machine], [warning], [good], [bad], [peak], [meas], [T1], [T2], [T3], [TD], [shift], [data_date], [no], [created_at])
VALUES
    (@data, @machine, @warning, @good, @bad, @peak, @meas, @T1, @T2, @T3, @TD, @shift, @syncDate, @no, @recordDate);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 1800;
                command.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(record, Formatting.Indented));
                command.Parameters.AddWithValue("@machine", Machine);
                command.Parameters.AddWithValue("@warning", record._warning);
                command.Parameters.AddWithValue("@good", record._good);
                command.Parameters.AddWithValue("@bad", record._bad);
                command.Parameters.AddWithValue("@peak", record._peak);
                command.Parameters.AddWithValue("@meas", record._meas);
                command.Parameters.AddWithValue("@T1", record._T1);
                command.Parameters.AddWithValue("@T2", record._T2);
                command.Parameters.AddWithValue("@T3", record._T3);
                command.Parameters.AddWithValue("@TD", record._TD);
                command.Parameters.AddWithValue("@shift", record._shift);
                command.Parameters.AddWithValue("@syncDate", syncDate);
                command.Parameters.AddWithValue("@no", record._no);
                command.Parameters.AddWithValue("@recordDate", record._date);
                command.ExecuteNonQuery();
            }
        }

        private static bool ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = stream.Read(buffer, totalRead, count - totalRead);
                if (bytesRead == 0)
                {
                    if (totalRead == 0)
                        return false;

                    throw new EndOfStreamException("File kết thúc giữa một record.");
                }

                totalRead += bytesRead;
            }

            return true;
        }

        private static void ArchiveFile(string file, string archiveDirectory)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);
            string destination = Path.Combine(archiveDirectory, fileName + extension);
            int suffix = 1;

            while (File.Exists(destination))
            {
                destination = Path.Combine(archiveDirectory, fileName + "_" + suffix + extension);
                suffix++;
            }

            File.Move(file, destination);
        }

        private static bool CopyStableFile(string sourcePath, string destinationPath, out DateTime sourceLastWriteTime)
        {
            FileInfo sourceInfo = new FileInfo(sourcePath);
            sourceInfo.Refresh();
            long originalLength = sourceInfo.Length;
            DateTime originalWriteTimeUtc = sourceInfo.LastWriteTimeUtc;
            sourceLastWriteTime = sourceInfo.LastWriteTime;

            File.Copy(sourcePath, destinationPath, false);

            sourceInfo.Refresh();
            if (sourceInfo.Length != originalLength || sourceInfo.LastWriteTimeUtc != originalWriteTimeUtc)
            {
                File.Delete(destinationPath);
                return false;
            }

            return true;
        }

        private void UploadPendingFileToServer()
        {
            UploadFileToServer(file_path, true);
        }

        private bool UploadFileToServer(string sourcePath, bool deleteSourceAfterUpload)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return false;

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string temporaryPath = Path.Combine(appDirectory, Guid.NewGuid().ToString("N") + ".dat");
            try
            {
                DateTime sourceLastWriteTime;
                if (!CopyStableFile(sourcePath, temporaryPath, out sourceLastWriteTime))
                    return false;

                string destinationDirectory = Path.Combine(
                    @"\\192.168.207.6\jtecdata\SYSTEM-JTECTOPICS\30. Data Transfer\10.Soltec Data",
                    Machine);
                Directory.CreateDirectory(destinationDirectory);

                string destinationPath = Path.Combine(
                    destinationDirectory,
                    sourceLastWriteTime.ToString("dd_MM_yyyy", CultureInfo.InvariantCulture) + ".dat");
                string uploadPath = Path.Combine(destinationDirectory, Guid.NewGuid().ToString("N") + ".upload");
                try
                {
                    File.Copy(temporaryPath, uploadPath, false);
                    if (File.Exists(destinationPath))
                        File.Replace(uploadPath, destinationPath, null);
                    else
                        File.Move(uploadPath, destinationPath);
                }
                finally
                {
                    if (File.Exists(uploadPath))
                        File.Delete(uploadPath);
                }

                if (deleteSourceAfterUpload)
                {
                    FileInfo sourceInfo = new FileInfo(sourcePath);
                    sourceInfo.Refresh();
                    if (sourceInfo.Exists &&
                        sourceInfo.Length == new FileInfo(temporaryPath).Length &&
                        sourceInfo.LastWriteTime == sourceLastWriteTime)
                    {
                        File.Delete(sourcePath);
                    }
                }

                return true;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
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
			File.Copy(binFilePath, datFilePath, true);
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
				List<Record_st> records = new List<Record_st>();
				using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
				byte[] a = new byte[897];
				if (!ReadExactly(fileStream, a, 64))
					throw new InvalidDataException("File không có header đầy đủ.");
				byte[] b = Encoding.UTF8.GetBytes("CFMLHEAD");
				if (!IsMemcmp(ref a, ref b, 8))
					throw new InvalidDataException("Header file không hợp lệ.");
				if (BitConverter.ToUInt16(a, 8) != 64)
					throw new InvalidDataException("Kích thước header không hợp lệ.");
				if (a[10] != 0 || a[11] != 3)
					throw new InvalidDataException("Phiên bản file không được hỗ trợ.");
				if (BitConverter.ToUInt16(a, 12) != 896)
					throw new InvalidDataException("Kích thước record không hợp lệ.");
				int num = BitConverter.ToUInt16(a, 14);
				if (num < 64 || num > fileStream.Length)
					throw new InvalidDataException("Vị trí bắt đầu record không hợp lệ.");
				fileStream.Seek(num, SeekOrigin.Begin);
				CRC32B cRC32B = new CRC32B();
				while (true)
				{
					Record_st record_st = new Record_st();
					record_st._tolerance = new short[8];
					if (!ReadExactly(fileStream, a, 896))
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
						records.Add(record_st);
						continue;
					}
					throw new Exception("ファイルレコードが規格外です(判定種別)");
				}

				}

				DateTime syncDate = DateTime.Now;
				DataAccess dataAccess = new DataAccess();
				dataAccess.ExecuteTransaction((connection, transaction) =>
				{
					using (SqlCommand command = new SqlCommand(
						"DELETE FROM [STOCKMANAGEMENT].[dbo].[temp_report_seltec_dap] WHERE machine = @machine",
						connection,
						transaction))
					{
						command.Parameters.AddWithValue("@machine", Machine);
						command.ExecuteNonQuery();
					}

					foreach (Record_st record in records)
						InsertRecord(connection, transaction, record, syncDate);
				});
		}

        private async void timer3_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(file_path) || !File.Exists(file_path) || !_syncGate.Wait(0))
                return;

            try
            {
                await Task.Run(() => loadDat(file_path));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đồng bộ dữ liệu tạm: " + ex.Message, "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _syncGate.Release();
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

        private async void button1_Click(object sender, EventArgs e)
        {
			OpenFileDialog openFileDialog1 = new OpenFileDialog
			{
				InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "complete"),
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
				try
				{
					bool uploaded = await Task.Run(() => UploadFileToServer(file, false));
					if (uploaded)
						MessageBox.Show("Đã gửi dữ liệu lên server thành công.", "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
					else
						MessageBox.Show("File đang được ghi hoặc không còn tồn tại; chưa gửi dữ liệu.", "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Lỗi gửi file dữ liệu lên server: " + ex.Message, "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		  public void checkUpdate()
        {
            try
            {
                #region Update chuong trinh moi (Neu co)
                //Đọc đường dẫn cập nhật từ PathUpdate.txt cạnh file chạy chương trình
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pathFile = Path.Combine(appDirectory, "PathUpdate.txt");
                string pathsource = File.Exists(pathFile)
                    ? File.ReadAllText(pathFile).Trim()
                    : null;
                if (string.IsNullOrWhiteSpace(pathsource))
                    return;

                string localExe = Process.GetCurrentProcess().MainModule.FileName;
                string targetExecutable = Path.GetFileName(localExe);
                string sourceExe = Directory.Exists(pathsource)
                    ? Path.Combine(pathsource, targetExecutable)
                    : pathsource;
                if (!Path.GetFileName(sourceExe).Equals(targetExecutable, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!File.Exists(sourceExe) || !File.Exists(localExe))
                    return;

                DateTime sourceLastWriteTime = File.GetLastWriteTimeUtc(sourceExe);
                DateTime localLastWriteTime = File.GetLastWriteTimeUtc(localExe);
                if (sourceLastWriteTime > localLastWriteTime)
                {
                    DialogResult dialogResult = MessageBox.Show("Đã có phiên bản mới, bạn có muốn cập nhật?", "Cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Process currentProcess = Process.GetCurrentProcess();
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Path.Combine(appDirectory, "AutoUpdate.exe"),
                            Arguments = "--wait-for-pid " + currentProcess.Id + " --target-exe " + targetExecutable,
                            UseShellExecute = true
                        });
                        Application.Exit();
                        currentProcess.Kill();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra cập nhật: " + ex.Message, "Cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
			checkUpdate();
            try
            {
                await Task.Run(() => UploadPendingFileToServer());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi file dữ liệu lên server: " + ex.Message, "Đồng bộ dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
		}
    }
}
