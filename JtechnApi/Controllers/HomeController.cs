using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using JtechnApi.Shares;
using System.Text.Json;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using JtechnApi.Departments.Repositories;
using JtechnApi.Employees.Repositories;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System.IO;
using System;
using JtechnApi.UploadKTNQ.Dtos;
using JtechnApi.UploadKTNQ.Models;
using JtechnApi.Shares.Connects;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("/")] // <-- Root path
    public class HomeController : BaseApiController
    {
        private readonly OracleConnection _conn_oracle;
        private readonly ILogger<HomeController> _logger;
        private readonly AdoJtecDB_Khoa _db;
        private readonly DBContext _context;
        private readonly RedisService _redisService;
        private readonly IDepartmentRepository _dept;
        private readonly IEmployeeRepository _emp;
        private readonly IEmployeeDepartmentRepository _emp_dept;

        [ActivatorUtilitiesConstructor]
        public HomeController(ILogger<HomeController> logger,AdoJtecDB_Khoa db, OracleConnection conn_oracle, RedisService redisService, DBContext context, IDepartmentRepository dept, IEmployeeRepository emp, IEmployeeDepartmentRepository emp_dept)
        {
            _logger = logger;
            _conn_oracle = conn_oracle;
            _redisService = redisService;
            _db = db;
            _context = context;
            _dept = dept;
            _emp = emp;
            _emp_dept = emp_dept;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet("/")]
        public IActionResult Index()
        {
            ViewData["Message"] = "Chào mừng đến Web API + View1";
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Đăng nhập";
            return View();
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return RedirectToAction(nameof(Login));
            }

            return Redirect("/admin");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Redirect("/login");
        }

        [HttpGet("admin")]
        [HttpGet("admin/dashboard")]
        public IActionResult AdminDashboard()
        {
            ViewData["Title"] = "Trang quản trị";
            return View();
        }

        [HttpGet("api/admin/dashboard/data")]
        [HttpGet("admin/dashboard/data")]
        [HttpGet("dashboard/data")]
        public IActionResult DashboardData()
        {
            return Ok(new
            {
                stats = new[]
                {
                    new { label = "Nhân sự", value = "1,284", trend = "+8.2% so với tháng trước", tone = "success" },
                    new { label = "Sản lượng", value = "84.6K", trend = "+5.4% so với tuần trước", tone = "success" },
                    new { label = "Đơn hàng", value = "328", trend = "12 cần xử lý", tone = "warning" },
                    new { label = "Báo cáo", value = "96", trend = "4 sự cố cần chú ý", tone = "danger" }
                },
                production = new[] { 55, 72, 68, 86, 92, 80, 98 },
                orders = new[]
                {
                    new { code = "J-2048", department = "Sản xuất", status = "Hoàn tất", date = "24/09/2026" },
                    new { code = "J-2047", department = "Lắp ráp", status = "Đang xử lý", date = "24/09/2026" },
                    new { code = "J-2046", department = "Kho vật tư", status = "Chờ duyệt", date = "23/09/2026" },
                    new { code = "J-2045", department = "Kiểm tra chất lượng", status = "Hoàn tất", date = "23/09/2026" }
                }
            });
        }
        //public IActionResult Index()
        //{
        //    var cmd = _conn_oracle.CreateCommand();
        //    cmd.CommandText = "SELECT 場所c,棚番 FROM TAD_Z60M WHERE 品目C = 'W FLRY-B0.5RB'";
        //
        //    var reader = cmd.ExecuteReader();
        //    var table = new DataTable();
        //    table.Load(reader);
        //    // Duyệt từng dòng (nếu muốn hiển thị)
        //    foreach (DataRow row in table.Rows)
        //    {
        //        var i= row;
        //    }
        //
        //    DataAccess ac = new DataAccess();
        //    ViewData["Message"] = "Chào mừng đến Web API + View";
        //    string querry = "SELECT TOP 10 [id] FROM[SmartManagement].[dbo].[Control_ProgramPlug_Visualize]";
        //    var dt = ac.RunQuery(querry);
        //    return View();
        //}
        [HttpGet("config/jtechn")]
        public async Task<IActionResult> GetConfig([FromQuery] int type = 1, string emps = null)
        {
            var dept = await _dept.GetAllAsync();
            var emp = await _emp.GetAll();
            var numbers = new List<int>();
            var emp_dept = new object();
            if (emps != null && emps != "")
            {
                numbers = emps.Split(',').Select(int.Parse).ToList();
                emp_dept = await _emp_dept.GetListByIdEmp(numbers);
            }
            return ApiResponseResult<object>(true, "Lấy dữ liệu ok", new { config = Helper.ConfigRequiredByType(1), department = dept, emp_dept = emp_dept, emp = emp });
        }
        [HttpGet("config/jtechn/workGroup")]
        public async Task<IActionResult> GetWorkGroup([FromQuery] int type = 2, string emps = null)
        {
            var emp = await _emp.GetAll();
            return ApiResponseResult<object>(true, "Lấy dữ liệu ok", new { config = Helper.ConfigRequiredByType(type), emp = emp });
        }
        [HttpGet("jtechn/test")]
        public IActionResult TestData()
        {
              string sql = $@"SELECT * FROM khsx WHERE masp IN ('{string.Join("','", new[] { "4160665241-01" })}')";
              DataTable result = _db.LoadTable(sql);
              var rows = result.AsEnumerable()
                  .Select(row => result.Columns.Cast<DataColumn>()
                      .ToDictionary(
                          column => column.ColumnName,
                          column => row.IsNull(column) ? null : row[column]))
                  .ToList();

              return Ok(rows);

        }
        // POST api/upload/single
        [HttpPost("upload/single")]
        public async Task<IActionResult> UploadSingle(IFormFile file, [FromForm] string folder)
        {
             var result = await Helper.ProcessFileAsync(file, folder);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest("Có lỗi xảy ra.");
        }

        // POST api/upload/multiple
        [HttpPost("upload/multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] IFormFile[] files, [FromForm] string folder)
        {
           if (files == null || files.Length == 0)
            return BadRequest(new { success = false, message = "Không có file nào." });

            var tasks = files.Select(file => Helper.ProcessFileAsync(file, folder));
            var results = await Task.WhenAll(tasks);

            var success = results.Where(r => r.Success).ToList();
            var failed  = results.Where(r => !r.Success).ToList();

            if (failed.Count == 0)
                return Ok(success);

            return StatusCode(207, new { success, failed }); // Multi-Status
        }
        [HttpPost("upload/ktnq")]
        public async Task<IActionResult> UploadKTNQ([FromForm] AddKTNQDto AddKTNQDto)
        {
            KTNQ check_ktnq = await _context.KTNQ.FirstOrDefaultAsync(x => x.Code == AddKTNQDto.Code);
            if (check_ktnq != null)
            {
                return ApiResponseResult<object>(false, "Mã KTNQ đã tồn tại", null);
            }
            KTNQ kTNQ = new KTNQ
            {
                Code = AddKTNQDto.Code,
                Content = AddKTNQDto.Content,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now
            };
            _context.KTNQ.Add(kTNQ);
            await _context.SaveChangesAsync();  
            return ApiResponseResult<object>(true, "Thêm KTNQ thành công", kTNQ);
        }
        [HttpGet("upload/ktnq/code")]
        public async Task<IActionResult> GetKTNQ([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ApiResponseResult<object>(false, "Mã KTNQ không được để trống", null);
            }
            var ktnq = await _context.KTNQ.FirstOrDefaultAsync(x => x.Code == code);
            if (ktnq == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy KTNQ với mã: " + code, null);
            }
            return ApiResponseResult<object>(true, "Lấy KTNQ thành công", ktnq);
        }
        /* ---------- helper ---------- */

    }

}
