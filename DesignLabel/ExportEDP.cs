using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace WindowsFormsApp5
{
    public partial class ExportEDP : Form
    {
        private readonly OracleHelper _oracle;
        public ExportEDP()
        {
            InitializeComponent();
            _oracle = new OracleHelper("User Id=JTC_USER;" + "Password=EX;" + "Data Source=192.168.207.14:1521/ORCL;");
        }

        private void rbTach_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbGop_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void rtbMSP_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var seqNos = rtbMSP.Lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
            if (seqNos.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập mỗi mã hàng trên một dòng.");
                return;
            }

            var seqList = string.Join("','", seqNos.Select(x => "3" + x.Replace("'", "''")));

            string sql = $@"
                   SELECT 
                        HINCD AS ""品番"",
                        HAN,
                        SENBAN AS ""線番"",
                        JUN,
                        NOUKI,
                        SIJINO,
                        KBN,
                        MARKNO AS ""マーク№"",
                        SENSYU AS ""線種"",
                        SENTYO AS ""線長"",
                        SURYO,
                        TABA AS ""束"",
                        TWIST1,
                        TWIST2 AS ""ツイスト後"",
                        TWIST3,
                        TASCDA AS ""端子Ａ"",
                        TASMEIA,
                        DOUBLEA1,
                        DOUBLEA2,
                        DOUBLEA3,
                        DOUBLEA4,
                        DOUBLEA5,
                        JOINTGA,
                        SYORIA1,
                        SYORIA2,
                        ZAISCDA,
                        PAIA,
                        NAGASAA,
                        GAIA,
                        SIA,
                        KBNA,
                        TASCDB AS ""端子Ｂ"",
                        TASMEIB,
                        DOUBLEB1,
                        DOUBLEB2,
                        DOUBLEB3,
                        DOUBLEB4,
                        DOUBLEB5,
                        JOINTGB,
                        SYORIB1,
                        SYORIB2,
                        ZAISCDB,
                        PAIB,
                        NAGASAB,
                        GAIB,
                        SIB,
                        KBNB,
                        BIKO,
                        INSDATE,
                        INSTIME,
                        INSTAN,
                        INSPC,
                        UPDDATE,
                        UPDTIME,
                        UPDTAN,
                        UPDPC,
                        KAWAA AS ""端子Ａカワムキ寸法"",
                        KAWAB AS ""端子Ｂカワムキ寸法"",
                        GUMCDA AS ""端子Ａゴム栓スリーブ"",
                        GUMCDB AS ""端子Ｂゴム栓スリーブ"",
                        GUMA,
                        GUMB,
                        HYOJIKBN,
                        RENBAN,
                        INFOCDA1,
                        INFOMEIA1 AS ""端子Ａ処理事項１"",
                        INFOCDA2,
                        INFOMEIA2 AS ""端子Ａ処理事項２"",
                        INFOCDB1,
                        INFOMEIB1 AS ""端子Ｂ処理事項１"",
                        INFOCDB2,
                        INFOMEIB2 AS ""端子Ｂ処理事項２"",
                        TWIST4,
                        TWIST5,
                        WUTIA,
                        WUTIB
                        FROM TDCJSIJI WHERE HINCD IN ('{seqList}')";
            DataTable result = _oracle.ExecuteDataTable(sql);

            if (result.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu EDP cho các mã hàng đã nhập.");
                return;
            }

            try
            {
                if (rbGop.Checked)
                {
                    ExportMerged(result);
                }
                else
                {
                    ExportSeparate(result, seqNos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xuất Excel thất bại: " + ex.Message);
            }
        }

        private void ExportMerged(DataTable result)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true,
                FileName = "EDP_Gop.xlsx"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                SaveDataTableToExcel(result, dialog.FileName, "EDP");
                MessageBox.Show("Đã xuất file Excel gộp thành công.");
            }
        }

        private void ExportSeparate(DataTable result, List<string> seqNos)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Chọn thư mục lưu các file Excel"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var exportedCount = 0;
                foreach (var seqNo in seqNos)
                {
                    var code = "3" + seqNo;
                    var rows = result.AsEnumerable()
                        .Where(row => string.Equals(Convert.ToString(row["品番"]).Trim(), code, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (rows.Count == 0)
                    {
                        continue;
                    }

                    var table = result.Clone();
                    foreach (var row in rows)
                    {
                        table.ImportRow(row);
                    }

                    var filePath = Path.Combine(dialog.SelectedPath, SanitizeFileName(code) + ".xlsx");
                    SaveDataTableToExcel(table, filePath, "EDP");
                    exportedCount++;
                }

                MessageBox.Show("Đã xuất " + exportedCount + " file Excel.");
            }
        }

        private static void SaveDataTableToExcel(DataTable table, string filePath, string worksheetName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(worksheetName);
                worksheet.Cell(1, 1).InsertTable(table, true);
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidCharacter, '_');
            }

            return fileName;
        }

    }
}
