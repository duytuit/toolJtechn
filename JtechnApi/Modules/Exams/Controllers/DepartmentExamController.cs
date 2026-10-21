using JtechnApi.Controllers;
using JtechnApi.Exams.Dtos;
using JtechnApi.Exams.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Exams.Controllers
{
    [Authorize(Policy = "Permission:DEPARTMENT_EXAMS.VIEW")]
    [Route("api/department-exams")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DepartmentExamController : Controller
    {
        private readonly IDepartmentExamRepository repository;
        public DepartmentExamController(IDepartmentExamRepository repository) => this.repository = repository;
        [HttpGet("/admin/department-exams")]
        public IActionResult Index() => View("~/Modules/Exams/Views/DepartmentExams.cshtml");
        [HttpGet("/js/exam-question-editor.js")]
        public IActionResult QuestionEditorScript([FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env) =>
            PhysicalFile(Path.Combine(env.ContentRootPath, "Modules", "Exams", "Views", "exam-question-editor.js"), "text/javascript; charset=utf-8");
        [HttpGet("/js/department-exams.js")]
        public IActionResult Script([FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env) =>
            PhysicalFile(Path.Combine(env.ContentRootPath, "Modules", "Exams", "Views", "department-exams.js"), "text/javascript; charset=utf-8");
        [HttpGet] public async Task<IActionResult> List(int? departmentId) => Ok(await repository.List(departmentId));
        [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id) => (await repository.GetById(id)).ToActionResult(this);
        [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(1500000)]
        [Authorize(Policy = "Permission:DEPARTMENT_EXAMS.CREATE")]
        public async Task<IActionResult> Create([FromBody] DepartmentExamInput input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Save(null, input)).ToActionResult(this);
        }
        [HttpPut("{id:int}"), ValidateAntiForgeryToken, RequestSizeLimit(1500000)]
        [Authorize(Policy = "Permission:DEPARTMENT_EXAMS.UPDATE")]
        public async Task<IActionResult> Edit(int id, [FromBody] DepartmentExamInput input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Save(id, input)).ToActionResult(this);
        }
        [HttpDelete("{id:int}"), ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:DEPARTMENT_EXAMS.DELETE")]
        public async Task<IActionResult> Delete(int id) => (await repository.Delete(id)).ToActionResult(this);
    }
}
