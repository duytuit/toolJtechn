
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignatureSubmissionController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly ISignatureSubmissionRepository repo;
        private readonly IRequiredRepository _required;
        private readonly ILogger<SignatureSubmissionController> _logger;
        private readonly DBContext _context;

        public SignatureSubmissionController(ILogger<SignatureSubmissionController> logger, ConnectionStrings c, ISignatureSubmissionRepository r, DBContext context,IRequiredRepository required_repo)
        {
            _logger = logger;
            con = c;
            repo = r;
            _context = context;
            _required = required_repo;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await repo.GetPaginatedAsync(page, pageSize);

            return Ok(result);
        }
        [HttpDelete("{id}")]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var isDeleted = await repo.Delete(id);
            if (!isDeleted)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [Route("change/status")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromForm] ChangeStatusTaskDto changeStatusTaskDto)
        {
            SignatureSubmission _sig = await repo.show(changeStatusTaskDto.Signature_Id);
            if (_sig == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu Signature", null);
            }
            Required _required_rs = await _required.show(changeStatusTaskDto.Required_Id);
            if (_required_rs == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu Required", null);
            }
            if(changeStatusTaskDto.Status == 1)
            {
                var __emp = _context.Employee.Where(x => x.Id == changeStatusTaskDto.Created_By).FirstOrDefault();
                var notifyRequest = new RemoteLampRequest
                {
                    Event = 15,
                    Chanel = "dencanhbao_cd_dap",
                    Status = 0,
                    Mode = 1,
                    MessageText = JsonSerializer.Serialize(new
                    {
                        job_id = _required_rs.Id,
                        job_name = _required_rs.Code,
                        app = "task",
                        code = __emp.Code,
                        link = $"http://192.168.207.6:8088/admin/internalCommunication",
                        status = changeStatusTaskDto.Status
                    })
                };
                string __result = Helper.RemoteLampSync(notifyRequest);
            }
            else
            {
                var __emp = _context.Employee.Where(x => x.Id == _sig.Signature_id).FirstOrDefault();
                var notifyRequest = new RemoteLampRequest
                {
                    Event = 15,
                    Chanel = "dencanhbao_cd_dap",
                    Status = 0,
                    Mode = 1,
                    MessageText = JsonSerializer.Serialize(new
                    {
                        job_id = _required_rs.Id,
                        job_name = _required_rs.Code,
                        app = "task",
                        code = __emp.Code,
                        link = $"http://192.168.207.6:8088/admin/internalCommunication",
                        status = changeStatusTaskDto.Status
                    })
                };
                string __result = Helper.RemoteLampSync(notifyRequest);
            }
            _sig.Status = changeStatusTaskDto.Status;
            _sig.Signature_id = changeStatusTaskDto.Status == 1 ? changeStatusTaskDto.Created_By : 0;
            _sig.Updated_at = DateTime.Now;
            _context.SignatureSubmission.Update(_sig);
            _context.SaveChanges();
            
            _sig = _context.SignatureSubmission.Where(u => u.Required_id == changeStatusTaskDto.Required_Id && u.Status == 0).FirstOrDefault();
            if (_sig == null)
            {
                _required_rs.Status = 1;
            }
            else
            {
                _required_rs.Status = 0;
            }
           
            _context.Required.Update(_required_rs);
            _context.SaveChanges();
            return ApiResponseResult<object>(true, "Cập nhật thành công", null);
        }
    }
}
