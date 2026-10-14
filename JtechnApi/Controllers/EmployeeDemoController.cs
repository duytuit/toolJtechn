using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JtechnApi.Controllers
{
    public class EmployeeDemoController : Controller
    {
        private static readonly object SyncRoot = new object();
        private static int NextId = 4;
        private static readonly List<DemoEmployee> Employees = new List<DemoEmployee>
        {
            new DemoEmployee { Id = 1, Code = "NV-001", Name = "Nguyễn Minh Anh", Email = "anh.nguyen@jtechn.vn", Department = "Sản xuất", Status = "Đang làm việc" },
            new DemoEmployee { Id = 2, Code = "NV-002", Name = "Trần Quốc Bảo", Email = "bao.tran@jtechn.vn", Department = "Lắp ráp", Status = "Đang làm việc" },
            new DemoEmployee { Id = 3, Code = "NV-003", Name = "Lê Thu Hà", Email = "ha.le@jtechn.vn", Department = "Kho vật tư", Status = "Tạm nghỉ" }
        };

        [HttpGet("admin/employees")]
        [HttpGet("employees")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý nhân viên";
            return View("~/Views/Home/Employees.cshtml");
        }

        [HttpGet("api/admin/employees")]
        [HttpGet("api/demo/employees")]
        public IActionResult Get([FromQuery] string keyword = null)
        {
            lock (SyncRoot)
            {
                var query = Employees.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var value = keyword.Trim();
                    query = query.Where(employee =>
                        employee.Code.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        employee.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        employee.Email.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        employee.Department.Contains(value, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(query.OrderBy(employee => employee.Id).ToList());
            }
        }

        [HttpPost("api/admin/employees")]
        [HttpPost("api/demo/employees")]
        public IActionResult Create([FromBody] DemoEmployeeInput input)
        {
            if (!IsValid(input)) return BadRequest(new { message = "Vui lòng nhập đủ mã, họ tên và email." });

            lock (SyncRoot)
            {
                if (Employees.Any(employee => employee.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict(new { message = "Mã nhân viên đã tồn tại." });
                }

                var employee = ToEmployee(input, NextId++);
                Employees.Add(employee);
                return Ok(employee);
            }
        }

        [HttpPut("api/admin/employees/{id:int}")]
        [HttpPut("api/demo/employees/{id:int}")]
        public IActionResult Update(int id, [FromBody] DemoEmployeeInput input)
        {
            if (!IsValid(input)) return BadRequest(new { message = "Vui lòng nhập đủ mã, họ tên và email." });

            lock (SyncRoot)
            {
                var employee = Employees.FirstOrDefault(item => item.Id == id);
                if (employee == null) return NotFound(new { message = "Không tìm thấy nhân viên." });

                if (Employees.Any(item => item.Id != id && item.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict(new { message = "Mã nhân viên đã tồn tại." });
                }

                employee.Code = input.Code.Trim();
                employee.Name = input.Name.Trim();
                employee.Email = input.Email.Trim();
                employee.Department = input.Department?.Trim() ?? "Chưa phân bộ phận";
                employee.Status = input.Status?.Trim() ?? "Đang làm việc";
                return Ok(employee);
            }
        }

        [HttpDelete("api/admin/employees/{id:int}")]
        [HttpDelete("api/demo/employees/{id:int}")]
        public IActionResult Delete(int id)
        {
            lock (SyncRoot)
            {
                var employee = Employees.FirstOrDefault(item => item.Id == id);
                if (employee == null) return NotFound(new { message = "Không tìm thấy nhân viên." });

                Employees.Remove(employee);
                return NoContent();
            }
        }

        private static bool IsValid(DemoEmployeeInput input)
        {
            return input != null &&
                !string.IsNullOrWhiteSpace(input.Code) &&
                !string.IsNullOrWhiteSpace(input.Name) &&
                !string.IsNullOrWhiteSpace(input.Email);
        }

        private static DemoEmployee ToEmployee(DemoEmployeeInput input, int id)
        {
            return new DemoEmployee
            {
                Id = id,
                Code = input.Code.Trim(),
                Name = input.Name.Trim(),
                Email = input.Email.Trim(),
                Department = input.Department?.Trim() ?? "Chưa phân bộ phận",
                Status = input.Status?.Trim() ?? "Đang làm việc"
            };
        }
    }

    public class DemoEmployee
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
    }

    public class DemoEmployeeInput
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
    }
}
