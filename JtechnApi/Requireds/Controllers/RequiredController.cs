
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequiredController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly IRequiredRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;

        public RequiredController(ILogger<ProductionPlanController> logger, ConnectionStrings c, IRequiredRepository r)
        {
            _logger = logger;
            con = c;
            repo = r;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, int pageSize = 50, [FromQuery] RequestRequiredDto RequestRequiredDto = null)
        {
            var result = await repo.GetPaginatedAsync(RequestRequiredDto, page, pageSize);
            if (result == null || result.Items.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }   
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("task/create")]
        public async Task<IActionResult> Create([FromForm] TaskRequiredDto TaskRequiredDto)
        {
            
            Required rs_check = await repo.CheckDuplicateTitle(TaskRequiredDto.Title, RequiredRepository.from_type_task, TaskRequiredDto.Created_client);
            if (rs_check != null)
            {
                return ApiResponseResult<object>(false, "Tiêu đề đã tồn tại", null);
            }
            string requireCode = "R_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var fromDeptJson = JsonSerializer.Deserialize<JsonElement>(Helper.ConfigFormType(1)).EnumerateArray().FirstOrDefault().GetProperty("to_dept");
            string jsonArray = JsonSerializer.Serialize(fromDeptJson);
            Required required = new Required
            {
                Code_required = requireCode,
                Code = TaskRequiredDto.Code, // email from user
                Content = TaskRequiredDto.Content,
                Content_form = TaskRequiredDto.Content_form,
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
            if (result != null)
            {
                return ApiResponseResult<object>(true, "Thêm mới thành công", result);
            }
            else
            {
                return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
            }
        }
    }
}
