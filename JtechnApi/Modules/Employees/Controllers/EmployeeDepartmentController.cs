using JtechnApi.Controllers;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Employees.Controllers
{
    [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
    [Route("api/employees")]
    public class EmployeeDepartmentController : Controller
    {
        private readonly IEmployeeDepartmentRepository repository;
        public EmployeeDepartmentController(IEmployeeDepartmentRepository repository) => this.repository = repository;

        [HttpGet("department-search")]
        public async Task<IActionResult> SearchEmployees(string q = null)
        {
            if (q?.Length > 150) return BadRequest(new { message = "Từ khóa tối đa 150 ký tự." });
            return Ok(await repository.SearchEmployees(q));
        }

        [HttpDelete("/api/departments/{departmentId:int}/employees/{employeeId:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int departmentId, int employeeId) =>
            (await repository.Remove(departmentId, employeeId)).ToActionResult(this);

        [HttpPost("sync-departments")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Synchronize() => (await repository.Synchronize()).ToActionResult(this);

        [HttpGet("/api/departments/{departmentId:int}/employees")]
        public async Task<IActionResult> DepartmentEmployees(int departmentId) =>
            (await repository.GetDepartmentEmployees(departmentId)).ToActionResult(this);

        [HttpGet("/admin/departments/{departmentId:int}/employees")]
        public async Task<IActionResult> DepartmentEmployeesPage(int departmentId, [FromServices] IAuthorizationService authorization)
        {
            ViewData["DepartmentId"] = departmentId;
            ViewData["CanUpdate"] = (await authorization.AuthorizeAsync(User, "Permission:EMPLOYEES.UPDATE")).Succeeded;
            return View("~/Modules/Departments/Views/Employees.cshtml");
        }

        [HttpGet("/admin/employee-departments")]
        public async Task<IActionResult> Index([FromServices] IAuthorizationService authorization)
        {
            ViewData["CanUpdate"] = (await authorization
                .AuthorizeAsync(User, "Permission:EMPLOYEES.UPDATE")).Succeeded;
            return View("~/Modules/Employees/Views/EmployeeDepartment.cshtml");
        }

        [HttpGet("/js/employee-departments.js")]
        public IActionResult Script([FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment) =>
            PhysicalFile(Path.Combine(environment.ContentRootPath, "Modules", "Employees", "Views", "employee-departments.js"), "text/javascript; charset=utf-8");

        [HttpGet("department-options")]
        public async Task<IActionResult> Options() => Ok(await repository.GetOptions());

        [HttpGet("{employeeId:int}/departments")]
        public async Task<IActionResult> Get(int employeeId) => (await repository.Get(employeeId)).ToActionResult(this);

        [HttpPut("{employeeId:int}/departments")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int employeeId, [FromBody] EmployeeDepartmentInput input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Save(employeeId, input)).ToActionResult(this);
        }
    }
}
