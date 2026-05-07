using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Repositories;
using Microsoft.AspNetCore.Authorization;
using Vudaco.Employees.Repositories;
using Vudaco.Departments.Repositories;
using System.Threading;
using Vudaco.Employees.Dtos;
using Vudaco.Departments.Dtos;
using Vudaco.PayrollPeriods.Dtos;
namespace Vudaco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // 👈 thêm dòng này
    public class DataShareController : BaseApiController
    {
        private readonly IEmployeeRepository _repoEmployee;
        private readonly IDepartmentRepositories _repoDepartment;
        private readonly ILogger<DataShareController> _logger;
        private readonly VudacoDBContext _context;
        private readonly FcmService _fcmService;
        private readonly IWebHostEnvironment _env;
        private readonly IFcmQueue _fcmQueue;
        public int userId => (int)HttpContext.Items["UserId"];

        public DataShareController(ILogger<DataShareController> logger, VudacoDBContext context, IWebHostEnvironment env, FcmService fcmService, IFcmQueue fcmQueue, IEmployeeRepository repoEmployee, IDepartmentRepositories repoDepartment)
        {
            _logger = logger;
            _context = context;
            _fcmService = fcmService;
            _fcmQueue = fcmQueue;
            _repoEmployee = repoEmployee;
            _repoDepartment = repoDepartment;
            _env = env;
        }
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendNotifyToUserDto dto)
        {
         
            await _fcmQueue.EnqueueAsync(new FcmJobDto
            {
                UserIds = new List<int> { dto.UserId },
                Title = dto.Title,
                Body = dto.Body,
                StorageId = 1,
                PostId = 1,
                Type = dto.Type,
                Screen = dto.Screen,
                Data = dto.Data
            });

            return Ok(new
            {
                message = "Sent",
               
            });
        }
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployee(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] EmployeeDto EmployeeDto = null )
        {
            // test
            var result = await _repoEmployee.GetObjectTaskAsync(EmployeeDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartment(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DepartmentDto DepartmentDto = null)
        {
            // test
            var result = await _repoDepartment.GetObjectTaskAsync(DepartmentDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetByCycleName")]
        public async Task<IActionResult> GetByCycleName(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] PayrollPeriodDetailDto  payrollPeriodDetailDto = null)
        {
            var _result = await _context.PayrollPeriodDetails.FirstOrDefaultAsync(x => x.CycleName == payrollPeriodDetailDto.CycleName && x.EmployeeId == payrollPeriodDetailDto.EmployeeId && x.StorageId == payrollPeriodDetailDto.StorageId);
            if (_result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var _employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == _result.EmployeeId);
            var _emp_dep = await _context.EmployeeDepartments.FirstOrDefaultAsync(x => x.EmployeeId == _result.EmployeeId);
            if (_emp_dep == null)
            {
                return ApiResponseResult<object>(false, "Nhân viên không thuộc phòng ban nào", null);
            }
            var _department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == _emp_dep.DepartmentId);
            if (_department == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy phòng ban", null);
            }
            _result.DepartmentName = _department.Name;
            _result.Employee = _employee;
            return ApiResponseResult(true, "Lấy dữ liệu thành công", _result);
        }
    }
}
