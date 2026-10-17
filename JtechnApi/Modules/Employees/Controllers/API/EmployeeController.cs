using JtechnApi.Controllers;
using JtechnApi.Employees.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Employees.Controllers.API
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository repository;

        public EmployeeController(IEmployeeRepository repository) => this.repository = repository;


        [HttpGet]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> Get()
        {
            return (await repository.Get()).ToActionResult(this);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> GetById(int id)
        {
            return (await repository.GetById(id)).ToActionResult(this);
        }

        [HttpPost]
        [Authorize(Policy = "Permission:EMPLOYEES.CREATE")]
        public async Task<IActionResult> Create([FromBody] EmployeeDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Create(input)).ToActionResult(this);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.Update(id, input)).ToActionResult(this);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            return (await repository.Delete(id)).ToActionResult(this);
        }
    }
}
