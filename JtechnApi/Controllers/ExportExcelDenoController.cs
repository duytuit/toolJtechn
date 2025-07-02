using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportExcelDenoController : ControllerBase
    {

        private readonly OracleConnection _conn_oracle;
        private readonly ILogger<ExamController> _logger;

        public ExportExcelDenoController(ILogger<ExamController> logger, OracleConnection conn_oracle)
        {
            _logger = logger;
            _conn_oracle = conn_oracle;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        //{
        //   // var result = await repo.GetPaginatedAsync(page, pageSize);
        //   // return Ok(result);
        //}
        //[HttpGet("export-order-chart")]
        //public IActionResult ExportOrderChart()
        //{
        //    
        //}
        [HttpGet("combo-chart")]
        public IActionResult ExportComboChart()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Báo cáo");

                // Tạo dữ liệu mẫu
                CreateSampleData(ws);

                // Tạo biểu đồ cột chính
                var chart = ws.Drawings.AddChart("comboChart", eChartType.ColumnClustered);
                chart.Title.Text = "Báo cáo doanh thu năm 2025";
                chart.SetPosition(15, 0, 0, 0);
                chart.SetSize(800, 400);

                // Thêm series cột (Doanh thu)
                var revenueSeries = chart.Series.Add(
                    ws.Cells["B2:B13"],
                    ws.Cells["A2:A13"]);
                revenueSeries.Header = "Doanh thu";
                revenueSeries.Fill.Color = Color.Blue;

                // Thêm series cột (Chi phí)
                var costSeries = chart.Series.Add(
                    ws.Cells["C2:C13"],
                    ws.Cells["A2:A13"]);
                costSeries.Header = "Chi phí";
                costSeries.Fill.Color = Color.Red;

                // Tạo biểu đồ đường RIÊNG BIỆT
                var lineChart = (ExcelLineChart)ws.Drawings.AddChart("lineChart", eChartType.Line);

                // Thêm series đường (Lợi nhuận)
                var profitSeries = lineChart.Series.Add(
                    ws.Cells["D2:D13"],
                    ws.Cells["A2:A13"]);
                profitSeries.Header = "Lợi nhuận";
                profitSeries.Border.Fill.Color = Color.Green;
                profitSeries.Border.Width = 2;

                // SAO CHÉP series đường sang biểu đồ chính
                foreach (var series in lineChart.Series)
                {
                    var newSeries = chart.Series.Add(series.Series, series.XSeries);
                    newSeries.Header = series.Header;
                    newSeries.Border.Fill.Color = series.Border.Fill.Color;
                    newSeries.Border.Width = series.Border.Width;
                }

                // Xóa biểu đồ đường riêng
                // ws.Drawings.Remove(lineChart);

                // Cấu hình chung
                chart.Legend.Position = eLegendPosition.Right;
                chart.YAxis.Title.Text = "Số tiền (VND)";
                chart.XAxis.Title.Text = "Tháng";

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ComboChart_Fixed.xlsx");
            }
        }

        private void CreateSampleData(ExcelWorksheet ws)
        {
            // Header
            ws.Cells["A1"].Value = "Tháng";
            ws.Cells["B1"].Value = "Doanh thu";
            ws.Cells["C1"].Value = "Chi phí";
            ws.Cells["D1"].Value = "Lợi nhuận";

            // Dữ liệu
            var rnd = new Random();
            var startDate = new DateTime(2025, 1, 1);

            for (int i = 0; i < 12; i++)
            {
                ws.Cells[i + 2, 1].Value = startDate.AddMonths(i).ToString("MM/yyyy");
                ws.Cells[i + 2, 2].Value = rnd.Next(50000, 100000);
                ws.Cells[i + 2, 3].Value = rnd.Next(20000, 50000);
                ws.Cells[i + 2, 4].Value = (int)ws.Cells[i + 2, 2].Value - (int)ws.Cells[i + 2, 3].Value;
            }
        }
        [HttpGet("data")]
        public IActionResult GetReportData()
        {
            var data = GenerateSampleData();
            return Ok(data);
        }

        private List<ReportData> GenerateSampleData()
        {
            var rnd = new Random();
            var list = new List<ReportData>();
            var startDate = new DateTime(2025, 1, 1);

            for (int i = 0; i < 12; i++)
            {
                var dt = rnd.Next(50000, 100000);
                var cp = rnd.Next(20000, 50000);
                list.Add(new ReportData
                {
                    Thang = startDate.AddMonths(i).ToString("MM/yyyy"),
                    DoanhThu = dt,
                    ChiPhi = cp,
                    LoiNhuan = dt - cp
                });
            }

            return list;
        }
    }
    public class ReportData
    {
        public string Thang { get; set; }  // "MM/yyyy"
        public int DoanhThu { get; set; }
        public int ChiPhi { get; set; }
        public int LoiNhuan { get; set; }
    }
}
