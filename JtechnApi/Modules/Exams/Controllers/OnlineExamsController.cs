using System.Security.Claims;
using JtechnApi.Exams.Models;
using JtechnApi.Exams.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Exams.Controllers
{
    [Authorize]
    [Route("api/online-exams")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class OnlineExamsController : Controller
    {
        private readonly IOnlineExamRepository repository;
        private string EmployeeId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        public OnlineExamsController(IOnlineExamRepository repository) => this.repository = repository;

        [HttpGet("/js/online-exams.js")]
        public IActionResult Script([FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
        {
            var path = Path.Combine(environment.ContentRootPath, "Modules", "Exams", "Views", "online-exams.js");
            return PhysicalFile(path, "text/javascript; charset=utf-8");
        }

        [HttpGet("/admin/exams")]
        public IActionResult Index() => View("~/Modules/Exams/Views/Index.cshtml");

        [HttpGet]
        public async Task<IActionResult> List(int? examId = null)
        {
            try { return Ok(await repository.Catalog(EmployeeId, examId)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("start")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start([FromBody] OnlineExamStartInput input, [FromQuery] int? examId = null)
        {
            if (input == null || !ModelState.IsValid) return BadRequest(new { message = "Vui lòng chọn ngày thi." });
            try { return Ok(await repository.StartCandidate(EmployeeId, User.FindFirstValue("FullName") ?? User.Identity.Name, examId, input)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet("attempts/{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await repository.GetCandidate(id, EmployeeId);
                return result == null ? NotFound() : Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPost("attempts/{id:guid}/{actionName:regex(^(save|submit)$)}")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(65536)]
        public async Task<IActionResult> Save(Guid id, string actionName, [FromBody] OnlineExamAnswers input)
        {
            if (input == null || !ModelState.IsValid) return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            try
            {
                var a = await repository.SaveCandidate(id, EmployeeId, input.Answers, actionName == "submit");
                return a == null ? NotFound() : Ok(a);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}
