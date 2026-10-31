using JtechnApi.Exams.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.Security.Claims;
using System.IO;

namespace JtechnApi.Exams.Controllers
{
    [Authorize]
    [Route("api/exam-results")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public sealed class ExamReportingController : Controller
    {
        private readonly IExamReportingRepository repository;

        public ExamReportingController(IExamReportingRepository repository) => this.repository = repository;

        [HttpGet("/admin/exam-results")]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public IActionResult ResultsPage() => View("~/Modules/Exams/Views/Results.cshtml");

        [HttpGet("filters")]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public async Task<IActionResult> ResultFilters() => Ok(await repository.ResultFilterOptions());

        [HttpGet]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public async Task<IActionResult> Results(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? deptId = null,
            [FromQuery] int? examId = null,
            [FromQuery] string search = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100 ||
                (deptId.HasValue && deptId.Value <= 0) ||
                (examId.HasValue && examId.Value <= 0))
                return BadRequest(new { message = "Trang hoặc số dòng mỗi trang không hợp lệ." });
            return Ok(await repository.Results(page, pageSize, deptId, examId, search));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Permission:TEST_RESULTS.DELETE")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var deletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await repository.DeleteResult(id, deletedBy) ? NoContent() : NotFound();
        }

        [HttpGet("export")]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public async Task<IActionResult> ExportResults(
            [FromQuery] int? deptId = null,
            [FromQuery] int? examId = null,
            [FromQuery] string search = null)
        {
            if ((deptId.HasValue && deptId.Value <= 0) || (examId.HasValue && examId.Value <= 0))
                return BadRequest(new { message = "Bộ lọc không hợp lệ." });

            var results = await repository.ExportResults(deptId, examId, search);
            if (results.Count > 1_048_575)
                return BadRequest(new { message = "Số kết quả vượt giới hạn 1.048.575 dòng của một trang tính Excel." });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Kết quả bài test");
            var headers = new[]
            {
                "Ngày thi", "Kỳ thi", "Nhân viên", "Mã nhân viên", "Bộ phận", "Mã bộ phận",
                "Bài thi", "Lần thi", "Số câu đúng", "Tổng câu hỏi", "Điểm", "Kết quả",
                "Nhân viên mới", "Thời gian làm bài (giây)", "Thời điểm nộp"
            };
            for (var column = 0; column < headers.Length; column++)
                worksheet.Cell(1, column + 1).Value = headers[column];

            for (var index = 0; index < results.Count; index++)
            {
                var result = results[index];
                var row = index + 2;
                if (result.ExamDate.HasValue) worksheet.Cell(row, 1).Value = result.ExamDate.Value;
                worksheet.Cell(row, 2).Value = result.CycleName ?? string.Empty;
                worksheet.Cell(row, 3).Value = result.EmployeeName ?? string.Empty;
                worksheet.Cell(row, 4).Value = result.EmployeeCode ?? string.Empty;
                worksheet.Cell(row, 5).Value = result.DepartmentName ?? string.Empty;
                worksheet.Cell(row, 6).Value = result.DepartmentId;
                worksheet.Cell(row, 7).Value = result.ExamName ?? string.Empty;
                worksheet.Cell(row, 8).Value = result.Mission;
                if (result.CorrectAnswers.HasValue) worksheet.Cell(row, 9).Value = result.CorrectAnswers.Value;
                if (result.TotalQuestions.HasValue) worksheet.Cell(row, 10).Value = result.TotalQuestions.Value;
                if (result.Score.HasValue) worksheet.Cell(row, 11).Value = result.Score.Value;
                worksheet.Cell(row, 12).Value = result.ResultStatus ?? string.Empty;
                worksheet.Cell(row, 13).Value = result.Newbie ? "Có" : "Không";
                if (result.CountingTime.HasValue) worksheet.Cell(row, 14).Value = result.CountingTime.Value;
                if (result.SubmittedAt.HasValue) worksheet.Cell(row, 15).Value = result.SubmittedAt.Value;
            }

            StyleWorksheet(worksheet, headers.Length, results.Count + 1);

            if (results.Count > 0)
            {
                worksheet.Range(2, 1, results.Count + 1, 1).Style.DateFormat.Format = "dd/mm/yyyy";
                worksheet.Range(2, 11, results.Count + 1, 11).Style.NumberFormat.Format = "0.####";
                worksheet.Range(2, 15, results.Count + 1, 15).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
            }
            worksheet.Columns().AdjustToContents();
            worksheet.Column(3).Width = Math.Min(35, Math.Max(worksheet.Column(3).Width, 18));
            worksheet.Column(7).Width = Math.Min(40, Math.Max(worksheet.Column(7).Width, 18));

            var filename = $"ket-qua-bai-test-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
            return ExcelFile(workbook, filename);
        }

        [HttpGet("quick-statistics")]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public async Task<IActionResult> QuickStatistics(
            [FromQuery] int? deptId = null,
            [FromQuery] int? examId = null)
        {
            if ((deptId.HasValue && deptId.Value <= 0) || (examId.HasValue && examId.Value <= 0))
                return BadRequest(new { message = "Bộ lọc không hợp lệ." });
            return Ok(await repository.QuickStatistics(deptId, examId));
        }

        [HttpGet("quick-statistics/export")]
        [Authorize(Policy = "Permission:TEST_RESULTS.VIEW")]
        public async Task<IActionResult> ExportQuickStatistics(
            [FromQuery] int period,
            [FromQuery] string category,
            [FromQuery] int? deptId = null,
            [FromQuery] int? examId = null)
        {
            var categories = new[] { "total", "passed", "failed", "retake", "retrain", "nottaken" };
            if (period is < 1 or > 2 ||
                string.IsNullOrWhiteSpace(category) ||
                !categories.Contains(category, StringComparer.OrdinalIgnoreCase) ||
                (deptId.HasValue && deptId.Value <= 0) ||
                (examId.HasValue && examId.Value <= 0))
                return BadRequest(new { message = "Thống kê hoặc bộ lọc không hợp lệ." });

            var results = await repository.ExportQuickStatistics(period, category, deptId, examId);
            if (results.Count > 1_048_575)
                return BadRequest(new { message = "Số nhân viên vượt giới hạn 1.048.575 dòng của một trang tính Excel." });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Thống kê nhanh");
            var headers = new[]
            {
                "Nhân viên", "Mã nhân viên", "Bộ phận", "Mã bộ phận", "Bài thi", "Ngày thi",
                "Kỳ thi", "Lần thi", "Số câu đúng", "Tổng câu hỏi", "Điểm", "Kết quả",
                "Thời gian làm bài (giây)", "Thời điểm nộp"
            };
            for (var column = 0; column < headers.Length; column++)
                worksheet.Cell(1, column + 1).Value = headers[column];

            for (var index = 0; index < results.Count; index++)
            {
                var result = results[index];
                var row = index + 2;
                worksheet.Cell(row, 1).Value = result.EmployeeName ?? string.Empty;
                worksheet.Cell(row, 2).Value = result.EmployeeCode ?? string.Empty;
                worksheet.Cell(row, 3).Value = result.DepartmentName ?? string.Empty;
                worksheet.Cell(row, 4).Value = result.DepartmentId;
                worksheet.Cell(row, 5).Value = result.ExamName ?? string.Empty;
                if (result.ExamDate.HasValue) worksheet.Cell(row, 6).Value = result.ExamDate.Value;
                worksheet.Cell(row, 7).Value = result.CycleName ?? string.Empty;
                if (result.Mission.HasValue) worksheet.Cell(row, 8).Value = result.Mission.Value;
                if (result.CorrectAnswers.HasValue) worksheet.Cell(row, 9).Value = result.CorrectAnswers.Value;
                if (result.TotalQuestions.HasValue) worksheet.Cell(row, 10).Value = result.TotalQuestions.Value;
                if (result.Score.HasValue) worksheet.Cell(row, 11).Value = result.Score.Value;
                worksheet.Cell(row, 12).Value = result.ResultStatus ?? string.Empty;
                if (result.CountingTime.HasValue) worksheet.Cell(row, 13).Value = result.CountingTime.Value;
                if (result.SubmittedAt.HasValue) worksheet.Cell(row, 14).Value = result.SubmittedAt.Value;
            }

            StyleWorksheet(worksheet, headers.Length, results.Count + 1);

            if (results.Count > 0)
            {
                worksheet.Range(2, 6, results.Count + 1, 6).Style.DateFormat.Format = "dd/mm/yyyy";
                worksheet.Range(2, 11, results.Count + 1, 11).Style.NumberFormat.Format = "0.####";
                worksheet.Range(2, 14, results.Count + 1, 14).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
            }
            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = Math.Min(35, Math.Max(worksheet.Column(1).Width, 18));
            worksheet.Column(5).Width = Math.Min(40, Math.Max(worksheet.Column(5).Width, 18));

            var filename = $"thong-ke-nhanh-dot-{period}-{category}-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
            return ExcelFile(workbook, filename);
        }

        private static void StyleWorksheet(IXLWorksheet worksheet, int columnCount, int lastRow)
        {
            var header = worksheet.Range(1, 1, 1, columnCount);
            header.Style.Font.Bold = true;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            worksheet.SheetView.FreezeRows(1);
            worksheet.Range(1, 1, lastRow, columnCount).SetAutoFilter();
        }

        private FileContentResult ExcelFile(XLWorkbook workbook, string filename)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                filename);
        }

        [HttpGet("/admin/exam-statistics")]
        [Authorize(Policy = "Permission:TEST_STATISTICS.VIEW")]
        public IActionResult StatisticsPage() => View("~/Modules/Exams/Views/Statistics.cshtml");

        [HttpGet("statistics")]
        [Authorize(Policy = "Permission:TEST_STATISTICS.VIEW")]
        public async Task<IActionResult> Statistics(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string search = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Trang hoặc số dòng mỗi trang không hợp lệ." });
            return Ok(await repository.Statistics(page, pageSize, search));
        }
    }
}
