using JtechnApi.Controllers;
using JtechnApi.Departments.Repositories;
using JtechnApi.Departments.Dtos;
using JtechnApi.Departments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Departments.Controllers
{
    [Authorize]
    [Route("api/departments")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository repository;

        public DepartmentController(IDepartmentRepository repository) => this.repository = repository;

        [HttpGet("/js/departments.js")]
        public IActionResult Script([FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
        {
            var path = Path.Combine(environment.ContentRootPath, "Modules", "Departments", "Views", "departments.js");
            return PhysicalFile(path, "text/javascript; charset=utf-8");
        }


        [HttpGet("/admin/departments")]

        public IActionResult Index() => View("~/Modules/Departments/Views/Index.cshtml");

        [HttpGet("/admin/departments/create")]
        public IActionResult CreatePage() => View("~/Modules/Departments/Views/CreateUpdate.cshtml");

        [HttpGet("/admin/departments/{id:int}/edit")]
        public IActionResult EditPage(int id)
        {
            ViewData["DepartmentId"] = id;
            return View("~/Modules/Departments/Views/CreateUpdate.cshtml");
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            return (await repository.All()).ToActionResult(this);
        }

        [HttpGet]

        public async Task<IActionResult> List(string search = null, int page = 1, int pageSize = 20)
        {
            return (await repository.List(search, page, pageSize)).ToActionResult(this);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            return (await repository.GetById(id)).ToActionResult(this);
        }

        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] DepartmentInput input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Create(input)).ToActionResult(this);
        }

        [HttpPut("{id:int}")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromBody] DepartmentInput input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Edit(id, input)).ToActionResult(this);
        }

        [HttpDelete("{id:int}")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            return (await repository.Delete(id)).ToActionResult(this);
        }
    }
}
