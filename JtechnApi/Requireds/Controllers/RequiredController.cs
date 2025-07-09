


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Employees.Repositories;
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequiredController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly IRequiredRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;
        private readonly IEmployeeRepository _emp;
        private readonly ISignatureSubmissionRepository _signature;
        private readonly DBContext _context;

        public RequiredController(ILogger<ProductionPlanController> logger, ConnectionStrings c, IRequiredRepository r, IEmployeeRepository emp, ISignatureSubmissionRepository signature, DBContext context)
        {
            _logger = logger;
            con = c;
            repo = r;
            _emp = emp;
            _signature = signature;
            _context = context;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, int pageSize = 50, [FromQuery] RequestRequiredDto RequestRequiredDto = null)
        {
            var result = await repo.GetPaginatedAsync(RequestRequiredDto, page, pageSize);
            if (result == null || result.TotalItems == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet]
        [Route("task")]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] RequestRequiredDto RequestRequiredDto = null )
        {

            if (RequestRequiredDto.Fields != null && RequestRequiredDto.Fields.Trim() != "")
            {
                // Nếu có trường Fields, gọi GetObjectTaskAsync
                var result = await repo.GetObjectTaskAsync(RequestRequiredDto, page, pageSize, cancellationToken);
                if (result == null || result.TotalItems == 0)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
            }
            else
            {
                var result = await repo.GetTaskAsync(RequestRequiredDto, page, pageSize);
                if (result == null || result.TotalItems == 0)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
            }
        }
        [HttpPost]
        [Route("task/create")]
        public async Task<IActionResult> Create([FromForm] TaskRequiredDto TaskRequiredDto)
        {

            int rs_check = await repo.CheckDuplicateTitle(TaskRequiredDto.Title, RequiredRepository.from_type_task, TaskRequiredDto.Created_client);
            if (rs_check > 0)
            {
                return ApiResponseResult<object>(false, "Tiêu đề đã tồn tại", null);
            }
            string requireCode = "R_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var firstDict = Helper.ConfigFormType(1);
            var mergedUsers = new List<string>();
            if (firstDict != null && firstDict.Count > 0)
            {
                var firstItem = firstDict[0];

                foreach (var pair in firstItem)
                {
                    if (pair.Key.StartsWith("user_") && pair.Value is List<string> users)
                    {
                        mergedUsers.AddRange(users);
                    }
                }

                // Loại bỏ trùng lặp nếu cần
                mergedUsers = mergedUsers.Distinct().ToList();
            }

            List<SelectEmployeeDto> rs_users = await _emp.GetByListCode(mergedUsers);

            var Content_form = new{info_users = rs_users};
            
            var toDeptJson = firstDict.FirstOrDefault()?.Where(pair => pair.Key == "to_dept")
                .Select(pair => pair.Value as List<int>)
                .FirstOrDefault() ?? new List<int>();
            string jsonArray = JsonSerializer.Serialize(toDeptJson);
            Required required = new Required
            {
                Code_required = requireCode,
                Code = TaskRequiredDto.Code, // email from user
                Content = TaskRequiredDto.Content,
                Content_form =  JsonSerializer.Serialize(Content_form),
                Attach = TaskRequiredDto.Attach,
                Title = TaskRequiredDto.Title,
                From_type = RequiredRepository.from_type_task,
                Required_department_id = 0,
                Receiving_department_ids = jsonArray,
                Type = 0,
                Order = 0,
                Quantity = 0,
                Unit_price = 0,
                Size = 0,
                Usage_status = 0,
                Status = 0,
                Created_client = TaskRequiredDto.Created_client,
            };
          
            var result = await repo.CreateRequiredAsync(required);
            foreach (var user in rs_users)
            {
                SignatureSubmission signatureSubmission = new SignatureSubmission
                {
                    Required_id = result.Id,
                    Department_id = user.SelectEmployeeDepartmentDto.Department_id,
                    Content = "",
                    Positions = 0,
                    Approve_id = JsonSerializer.Serialize(new List<int> { user.Id }),
                    Signature_id = user.Id,
                    Status = 0, // Chưa duyệt

                };
                var signatureResult = await _signature.CreateSignatureSubmissiondAsync(signatureSubmission);
            }
           
            if (result != null)
            {
                return ApiResponseResult<object>(true, "Thêm mới thành công", result);
            }else
            {
                return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
            }
        }
        [HttpGet("task/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Required required = await repo.show(id);
            if (required == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult<object>(true, "Lấy dữ liệu thành công", required);
        }
    }
}
