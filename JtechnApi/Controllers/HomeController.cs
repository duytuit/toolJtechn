using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("/")] // <-- Root path
    public class HomeController : BaseApiController
    {
        private readonly OracleConnection _conn_oracle;
        private readonly ILogger<HomeController> _logger;
        private readonly DBContext _context;
        private readonly RedisService _redisService;
        private readonly IDepartmentRepository _dept;
        private readonly IEmployeeRepository _emp;
        private readonly IEmployeeDepartmentRepository _emp_dept;

        public HomeController(ILogger<HomeController> logger, OracleConnection conn_oracle, RedisService redisService, DBContext context, IDepartmentRepository dept, IEmployeeRepository emp, IEmployeeDepartmentRepository emp_dept)
        {
            _logger = logger;
            _conn_oracle = conn_oracle;
            _redisService = redisService;
            _dept = dept;
            _emp = emp;
            _emp_dept = emp_dept;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            return "Chào mừng bạn đến với chúng tôi....";
            //var value = _redisService.GetAsync("jtec_hn_database_update_AsyncKTNQ");
            //return value.Result ?? "No value found in Redis";
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
        public async Task<IActionResult> GetWorkGroup([FromQuery] int type = 1, string emps = null)
        {
            var emp = await _emp.GetAll();
            return ApiResponseResult<object>(true, "Lấy dữ liệu ok", new { config = Helper.ConfigRequiredByType(2), emp = emp });
        }
        // POST api/upload/single
        [HttpPost("upload/single")]
        public async Task<IActionResult> UploadSingle(IFormFile file)
        {
             var result = await Helper.ProcessFileAsync(file);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest("Có lỗi xảy ra.");
        }

        // POST api/upload/multiple
        [HttpPost("upload/multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] IFormFile[] files)
        {
           if (files == null || files.Length == 0)
            return BadRequest(new { success = false, message = "Không có file nào." });

            var tasks = files.Select(file => Helper.ProcessFileAsync(file));
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
