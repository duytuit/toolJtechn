


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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
       // private readonly IDbContextTransaction  _dbcontext;
        private readonly DBContext _context;
        private readonly RedisService _redis;

        public RequiredController(ILogger<ProductionPlanController> logger, RedisService redis, ConnectionStrings c, IRequiredRepository r, IEmployeeRepository emp, ISignatureSubmissionRepository signature, DBContext context)
        {
            _logger = logger;
            con = c;
            repo = r;
            _emp = emp;
            _signature = signature;
            _context = context;
            _redis = redis;
            // _dbcontext = dbContext;
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

            var result = await repo.GetObjectTaskAsync(RequestRequiredDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet]
        [Route("task/v2/getTeam")]
        public async Task<IActionResult> GetTeam(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] RequestTeamSubLeaderDto RequestTeamSubLeaderDto = null)
        {
            try
            {
                var productionPlanCamUser = await _redis.GetAsync("jtec_hn_database_productionPlanCamUser");

                // Chuyển sang list các dictionary
                var list = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(productionPlanCamUser);
                // Mảng giá trị cần lọc
                var listValue = RequestTeamSubLeaderDto.Code.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => x.Trim())
                      .ToList();
                var codes = list
                .Where(x => listValue.Contains(GetValue(x["ma_sp"])))
                .Select(x => GetValue(x["code"])) // Lấy giá trị code
                .Distinct()                       // Loại trùng
                .ToList();
                var emp = _context.Employee.Where(x => codes.Contains(x.Code)).Select(x => x.Id).ToList();

                var _results = new PaginatedResultVue<object>
                {
                    Current_page = page,
                    Per_page = pageSize,
                    Last_page = 0,
                    Total = 0
                };
                _results.Extra["emp_ids"] = emp;

                if (_results == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", _results);
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message, null);
            }
           
        }
        // [HttpPost]
        // [Route("task/create")]
        // public async Task<IActionResult> Create([FromForm] TaskRequiredDto TaskRequiredDto)
        // {

        //     int rs_check = await repo.CheckDuplicateTitle(TaskRequiredDto.Title, RequiredRepository.from_type_task, TaskRequiredDto.Created_client);
        //     if (rs_check > 0)
        //     {
        //         return ApiResponseResult<object>(false, "Tiêu đề đã tồn tại", null);
        //     }
        //     string requireCode = "R_" + DateTime.Now.ToString("yyyyMMddHHmmss");

        //     var firstDict = Helper.ConfigFormType(1);
        //     var mergedUsers = new List<string>();
        //     if (firstDict != null && firstDict.Count > 0)
        //     {
        //         var firstItem = firstDict[0];

        //         foreach (var pair in firstItem)
        //         {
        //             if (pair.Key.StartsWith("user_") && pair.Value is List<string> users)
        //             {
        //                 mergedUsers.AddRange(users);
        //             }
        //         }

        //         // Loại bỏ trùng lặp nếu cần
        //         mergedUsers = mergedUsers.Distinct().ToList();
        //     }

        //     List<SelectEmployeeDto> rs_users = await _emp.GetByListCode(mergedUsers);

        //     var Content_form = new{info_users = rs_users};

        //     var toDeptJson = firstDict.FirstOrDefault()?.Where(pair => pair.Key == "to_dept")
        //         .Select(pair => pair.Value as List<int>)
        //         .FirstOrDefault() ?? new List<int>();
        //     string jsonArray = JsonSerializer.Serialize(toDeptJson);
        //     Required required = new Required
        //     {
        //         Code_required = requireCode,
        //         Code = TaskRequiredDto.Code, // email from user
        //         Content = TaskRequiredDto.Content,
        //         Content_form =  JsonSerializer.Serialize(Content_form),
        //         Attach = TaskRequiredDto.Attach,
        //         Title = TaskRequiredDto.Title,
        //         From_type = RequiredRepository.from_type_task,
        //         Required_department_id = 0,
        //         Receiving_department_ids = jsonArray,
        //         Type = 0,
        //         Order = 0,
        //         Quantity = 0,
        //         Unit_price = 0,
        //         Size = 0,
        //         Usage_status = 0,
        //         Status = 0,
        //         Created_client = TaskRequiredDto.Created_client,
        //     };

        //       /* 1️⃣  Lấy execution‑strategy */
        //     var strategy = _context.Database.CreateExecutionStrategy();

        //     /* 2️⃣  Thực thi toàn bộ trong strategy.Execute */
        //     return strategy.Execute(() =>
        //     {
        //         using var tx = _context.Database.BeginTransaction();  // sync
        //         try
        //         {
        //             var result = repo.CreateRequiredAsync(required);
        //             foreach (var user in rs_users)
        //             {
        //                 SignatureSubmission signatureSubmission = new SignatureSubmission
        //                 {
        //                     Required_id = result.Id,
        //                     Department_id = user.SelectEmployeeDepartmentDto.Department_id,
        //                     Content = "",
        //                     Positions = 0,
        //                     Approve_id = JsonSerializer.Serialize(new List<int> { user.Id }),
        //                     Signature_id = user.Id,
        //                     Status = 0, // Chưa duyệt

        //                 };
        //                 var signatureResult = _signature.CreateSignatureSubmissiondAsync(signatureSubmission);
        //             }
        //             tx.Commit();
        //             if (result != null)
        //             {
        //                 return ApiResponseResult<object>(true, "Thêm mới thành công", result);
        //             }
        //             else
        //             {
        //                 return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             tx.Rollback();
        //             _logger.LogError(ex, "Lỗi thêm mới Required");
        //             return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
        //         }

        //     });
        // }
        [HttpPost]
        [Route("task/create")]
        public async Task<IActionResult> CreateTask([FromForm] TaskRequiredDto TaskRequiredDto)
        {

            int rs_check = await repo.CheckDuplicateTitle(TaskRequiredDto.Title, RequiredRepository.from_type_task, TaskRequiredDto.Created_client);
            if (rs_check > 0)
            {
                return ApiResponseResult<object>(false, "Tiêu đề đã tồn tại", null);
            }
            string requireCode = "R_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //List<SelectEmployeeDto> rs_users = await _emp.GetByListCode(mergedUsers);

            dynamic config = Helper.ConfigRequiredByType(1);
            string jsonArray = JsonSerializer.Serialize(config.to_dept);
            var content_form = new
            {
                task_types = TaskRequiredDto.Task_types
            };
            Required required = new Required
            {
                Code_required = requireCode,
                Code = TaskRequiredDto.Code.Trim(), // mã sản phẩm
                Content = TaskRequiredDto.Content,
                Attach = TaskRequiredDto.Attach,
                Title = TaskRequiredDto.Code.Trim(),
                From_type = RequiredRepository.from_type_task,
                Required_department_id = 0,
                Receiving_department_ids = jsonArray,
                Content_form= JsonSerializer.Serialize(content_form),
                Type = 0,
                Order = 0,
                Quantity = 0,
                Unit_price = 0,
                Size = 0,
                Usage_status = 0,
                Status = 0,
                Created_client = TaskRequiredDto.Created_client,
            };
       
              /* 1️⃣  Lấy execution‑strategy */
            var strategy = _context.Database.CreateExecutionStrategy();

            /* 2️⃣  Thực thi toàn bộ trong strategy.Execute */
            return strategy.Execute(() =>
            {
                using var tx = _context.Database.BeginTransaction();  // sync
                try
                {
                    var result = repo.CreateRequiredAsync(required);
                    _logger.LogInformation(result.ToString());
                    var empDepts = JsonSerializer.Deserialize<Dictionary<int, List<JsonElement>>>(TaskRequiredDto.Emp_depts);

                    foreach (var key in empDepts.Keys.ToList())
                    {
                        foreach (var item in empDepts[key].Where(e => e.ValueKind != JsonValueKind.Null).ToList())
                        {
                            // Nếu mỗi item là object có "value" là mã nhân viên
                            int intId = item.GetProperty("value").GetInt32();

                            var existing = _signature.FindByRequired(result.Result.Id, key, intId);
                            if (existing.Result != null) continue;

                            var sig = new SignatureSubmission
                            {
                                Required_id = result.Result.Id,
                                Department_id = key,
                                Approve_id = JsonSerializer.Serialize(new List<int> { intId }),
                                Signature_id = intId,
                                Status = 0,
                                Content = "",
                                Positions = 0,
                            };

                            _signature.CreateSignatureSubmissiondAsync(sig);
                        }
                    }
                    tx.Commit();
                    if (result != null)
                    {
                        return ApiResponseResult(true, "Thêm mới thành công", result.Result);
                    }
                    else
                    {
                        return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
                    }
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "Đã xảy ra lỗi khi thêm mới");
                    return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
                }
              
            });
        }
        [HttpPost]
        [Route("task/v2/create")]
        public async Task<IActionResult> CreateTaskV2([FromForm] TaskRequiredDto TaskRequiredDto)
        {

            int rs_check = await repo.CheckDuplicateTitle(TaskRequiredDto.Title, RequiredRepository.from_type_task, TaskRequiredDto.Created_client);
            if (rs_check > 0)
            {
                return ApiResponseResult<object>(false, "Liên lạc đã tồn tại", null);
            }
            string requireCode = "R_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //List<SelectEmployeeDto> rs_users = await _emp.GetByListCode(mergedUsers);

            dynamic config = Helper.ConfigRequiredByType(1);
            string jsonArray = JsonSerializer.Serialize(config.to_dept);
            var content_form = new
            {
                task_types = TaskRequiredDto.Task_types
            };
            Required required = new Required
            {
                Code_required = requireCode,
                Code = TaskRequiredDto.Code.Trim(), // mã sản phẩm
                Content = TaskRequiredDto.Content,
                Attach = TaskRequiredDto.Attach,
                Title = TaskRequiredDto.Code.Trim(),
                From_type = RequiredRepository.from_type_task,
                Required_department_id = 0,
                Receiving_department_ids = jsonArray,
                Content_form = JsonSerializer.Serialize(content_form),
                Type = 0,
                Order = 0,
                Quantity = 0,
                Unit_price = 0,
                Size = 0,
                Usage_status = 0,
                Status = 0,
                Created_by = TaskRequiredDto.UserId,
                Created_client = TaskRequiredDto.Created_client,
            };

            /* 1️⃣  Lấy execution‑strategy */
            var strategy = _context.Database.CreateExecutionStrategy();

            /* 2️⃣  Thực thi toàn bộ trong strategy.Execute */
            return strategy.Execute(() =>
            {
                using var tx = _context.Database.BeginTransaction();  // sync
                try
                {
                    var result = repo.CreateRequiredAsync(required);
                    _logger.LogInformation(result.ToString());
                    var empDepts = JsonSerializer.Deserialize<List<JsonElement>>(TaskRequiredDto.Emp_depts);

                    foreach (var key in empDepts.ToList())
                    {
                            // Nếu mỗi item là object có "value" là mã nhân viên
                            int intId = key.GetProperty("value").GetInt32();
                            var existing = _signature.FindByRequiredV2(result.Result.Id, intId);
                            if (existing.Result != null) continue;
                            var sig = new SignatureSubmission
                            {
                                Required_id = result.Result.Id,
                                Department_id = 0,
                                Approve_id = JsonSerializer.Serialize(new List<int> { intId }),
                                Signature_id = intId,
                                Status = 0,
                                Content = "",
                                Positions = 0,
                            };
                            _signature.CreateSignatureSubmissiondAsync(sig);
                        var __emp = _context.Employee.Where(x=>x.Id == intId).FirstOrDefault();
                        var newNotify_object = new
                        {
                            id = 0,
                            job_id = result.Result.Id,
                            job_name = result.Result.Code + "lúc:" + result.Result.Created_at,
                            app = "task",
                            code = __emp.Code,
                            link = $"http://192.168.207.6:8088/admin/internalCommunication",
                            status = 0
                        };
                        using (var _db = new clsKetNoi())
                        {
                            _db.UpsertFromObject("Notifycation", newNotify_object);
                        }
                        var notifyRequest = new RemoteLampRequest
                        {
                            Event = 15,
                            Chanel = "dencanhbao_cd_dap",
                            Status = 0,
                            Mode = 1,
                            MessageText = JsonSerializer.Serialize(new
                            {
                                job_id = result.Result.Id,
                                job_name = result.Result.Code + "lúc:"+ result.Result.Created_at,
                                app = "task",
                                code = __emp.Code,
                                link = $"http://192.168.207.6:8088/admin/internalCommunication",
                                status = 0
                            })
                        };
                        string __result = Helper.RemoteLampSync(notifyRequest);
                    }
                    tx.Commit();
                    if (result != null)
                    {
                        return ApiResponseResult(true, "Thêm mới thành công", result.Result);
                    }
                    else
                    {
                        return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
                    }
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "Đã xảy ra lỗi khi thêm mới");
                    return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
                }

            });
        }
        [HttpPost]
        [Route("task/v2/update/{id}")]
        public async Task<IActionResult> UpdateTaskV2([FromForm] TaskRequiredDto TaskRequiredDto,int id)
        {

            Required _required = await repo.show(id);
            if (_required == null)
            {
                return ApiResponseResult<object>(false, "Công việc không tồn tại", null);
            }
            // 1) Parse
            JObject obj = JObject.Parse(_required.Content_form);
            // 2) Sửa task_types
            // var arr = JArray.Parse((string)obj["task_types"]);
            // arr.Add(4);
            obj["task_types"] = TaskRequiredDto.Task_types;     
            // ➕ 3) Thêm thuộc tính mới
            //obj["deadline"] = "2025-12-31";
            _required.Content_form = obj.ToString();
            _required.Content = TaskRequiredDto.Content;
            _required.Attach = TaskRequiredDto.Attach;
            _required.Title = TaskRequiredDto.Code.Trim();
            _required.Code = TaskRequiredDto.Code.Trim();
            _required.Required_department_id = 0;
            _required.Updated_by = TaskRequiredDto.UserId;
       
              /* 1️⃣  Lấy execution‑strategy */
            var strategy = _context.Database.CreateExecutionStrategy();

            /* 2️⃣  Thực thi toàn bộ trong strategy.Execute */
            return strategy.Execute(() =>
            {
                using var tx = _context.Database.BeginTransaction();  // sync
                try
                {
                    var result = repo.UpdateRequiredAsync(_required);
                    
                    // Deserialize chính xác kiểu dữ liệu
                    var empDepts = JsonSerializer.Deserialize<List<JsonElement>>(TaskRequiredDto.Emp_depts);

                    foreach (var key in empDepts.ToList())
                    {
                            int intId = key.GetProperty("value").GetInt32();
                            // Kiểm tra nếu đã tồn tại
                            var _sig = _signature.FindByRequiredV2(result.Result.Id, intId);
                            if (_sig.Result != null)
                            {
                                continue;
                            }

                            var signatureSubmission = new SignatureSubmission
                            {
                                Required_id = result.Result.Id,
                                Department_id = 0,
                                Content = "",
                                Positions = 0,
                                Approve_id = JsonSerializer.Serialize(new List<int> { intId }),
                                Signature_id = intId,
                                Status = 0
                            };

                             _signature.CreateSignatureSubmissiondAsync(signatureSubmission);
                        var __emp = _context.Employee.Where(x => x.Id == intId).FirstOrDefault();
                        var notifyRequest = new RemoteLampRequest
                        {
                            Event = 15,
                            Chanel = "dencanhbao_cd_dap",
                            Status = 0,
                            Mode = 1,
                            MessageText = JsonSerializer.Serialize(new
                            {
                                job_id = result.Result.Id,
                                job_name = result.Result.Code + "lúc:" + result.Result.Created_at,
                                app = "task",
                                code = __emp.Code,
                                link = $"http://192.168.207.6:8088/admin/internalCommunication",
                                status = 0
                            })
                        };
                        string __result = Helper.RemoteLampSync(notifyRequest);
                    }
                    tx.Commit();
                    if (result != null)
                    {
                        return ApiResponseResult(true, "Cập nhật thành công", result.Result);
                    }
                    else
                    {
                        return ApiResponseResult<object>(false, "Cập nhật thất bại", null);
                    }
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "Đã xảy ra lỗi khi cập nhật");
                    return ApiResponseResult<object>(false, "Cập nhật thất bại", null);
                }
              
            });
        }
        [HttpPost]
        [Route("task/update/{id}")]
        public async Task<IActionResult> UpdateTask([FromForm] TaskRequiredDto TaskRequiredDto, int id)
        {

            Required _required = await repo.show(id);
            if (_required == null)
            {
                return ApiResponseResult<object>(false, "Công việc không tồn tại", null);
            }
            // 1) Parse
            JObject obj = JObject.Parse(_required.Content_form);
            // 2) Sửa task_types
            // var arr = JArray.Parse((string)obj["task_types"]);
            // arr.Add(4);
            obj["task_types"] = TaskRequiredDto.Task_types;
            // ➕ 3) Thêm thuộc tính mới
            //obj["deadline"] = "2025-12-31";
            _required.Content_form = obj.ToString();
            _required.Content = TaskRequiredDto.Content;
            _required.Attach = TaskRequiredDto.Attach;
            _required.Title = TaskRequiredDto.Code.Trim();
            _required.Code = TaskRequiredDto.Code.Trim();
            _required.Required_department_id = 0;

            /* 1️⃣  Lấy execution‑strategy */
            var strategy = _context.Database.CreateExecutionStrategy();

            /* 2️⃣  Thực thi toàn bộ trong strategy.Execute */
            return strategy.Execute(() =>
            {
                using var tx = _context.Database.BeginTransaction();  // sync
                try
                {
                    var result = repo.UpdateRequiredAsync(_required);

                    // Deserialize chính xác kiểu dữ liệu
                    var empDepts = JsonSerializer.Deserialize<Dictionary<int, List<JsonElement>>>(TaskRequiredDto.Emp_depts);

                    foreach (var key in empDepts.Keys.ToList())
                    {
                        foreach (var item in empDepts[key].Where(e => e.ValueKind != JsonValueKind.Null).ToList())
                        {
                            int intId = item.GetProperty("value").GetInt32();

                            // Kiểm tra nếu đã tồn tại
                            var _sig = _signature.FindByRequired(result.Result.Id, key, intId);
                            if (_sig.Result != null)
                            {
                                continue;
                            }

                            var signatureSubmission = new SignatureSubmission
                            {
                                Required_id = result.Result.Id,
                                Department_id = key,
                                Content = "",
                                Positions = 0,
                                Approve_id = JsonSerializer.Serialize(new List<int> { intId }),
                                Signature_id = intId,
                                Status = 0
                            };

                            _signature.CreateSignatureSubmissiondAsync(signatureSubmission);
                        }
                    }
                    tx.Commit();
                    if (result != null)
                    {
                        return ApiResponseResult(true, "Cập nhật thành công", result.Result);
                    }
                    else
                    {
                        return ApiResponseResult<object>(false, "Cập nhật thất bại", null);
                    }
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "Đã xảy ra lỗi khi cập nhật");
                    return ApiResponseResult<object>(false, "Cập nhật thất bại", null);
                }

            });
        }
        [HttpGet("task/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var required = await repo.detail(id);
            if (required == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", required);
        }
        [HttpDelete("{id}")]
        [Route("task/delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var isDeleted = await repo.DeleteRequiredAsync(id);
            if (!isDeleted)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using (var _db = new clsKetNoi())
            {
                var whereEquals = new Dictionary<string, object>
                {
                    ["job_id"] = id,
                    ["app"] = "task"
                };
                _db.DeleteWhere("Notifycation", whereEquals);
            }
            var notifyRequest = new RemoteLampRequest
            {
                Event = 15,
                Chanel = "dencanhbao_cd_dap",
                Status = 0,
                Mode = 1,
                MessageText = JsonSerializer.Serialize(new
                {
                    job_id = id,
                    app = "task",
                    status = 3
                })
            };
            Helper.RemoteLampSync(notifyRequest);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        string GetValue(JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.String => el.GetString(),
                JsonValueKind.Number => el.GetRawText(),  // Lấy số dạng chuỗi
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }
    }
}
