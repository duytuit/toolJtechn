using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
using Vudaco.Controllers;
using Vudaco.PayrollPeriods.Dtos;
using Vudaco.PayrollPeriods.Models;
using Vudaco.PayrollPeriods.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.PayrollPeriods.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollPeriodController : BaseApiController
    {
        private readonly IPayrollPeriodRepositories _repoPayrollPeriod;
        private readonly IPayrollPeriodDetailRepositories _repoPayrollPeriodDetail;
        private readonly ILogger<PayrollPeriodController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public PayrollPeriodController(ILogger<PayrollPeriodController> logger, IPayrollPeriodDetailRepositories repoPayrollPeriodDetail, VudacoDBContext context)
        {
            _logger = logger;
            _repoPayrollPeriodDetail = repoPayrollPeriodDetail;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] PayrollPeriodDetailDto  payrollPeriodDetailDto = null)
        {
            // test
            var result = await _repoPayrollPeriodDetail.GetObjectTaskAsync(payrollPeriodDetailDto, page, pageSize, cancellationToken);
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
            return ApiResponseResult(true, "Lấy dữ liệu thành công", _result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PayrollPeriodDetailDto PayrollPeriodDetailDto)
        {
             var now = DateTime.Now;
             var _payrollPeriodDetail = await _context.PayrollPeriodDetails.FirstOrDefaultAsync(x => x.CycleName == PayrollPeriodDetailDto.CycleName && x.EmployeeId == PayrollPeriodDetailDto.EmployeeId && x.StorageId == PayrollPeriodDetailDto.StorageId);
             if (_payrollPeriodDetail != null)
             {  
                _payrollPeriodDetail.DeletedAt = now;
                _payrollPeriodDetail.DeletedBy = userId;
                _context.PayrollPeriodDetails.Update(_payrollPeriodDetail);
                await _context.SaveChangesAsync();
             }
            try
            {
                 var payrollPeriodDetail = new PayrollPeriodDetail
                 {
                     StorageId = PayrollPeriodDetailDto.StorageId,
                     CycleName = PayrollPeriodDetailDto.CycleName,
                     EmployeeId = PayrollPeriodDetailDto.EmployeeId,
                     Status = PayrollPeriodDetailDto.Status,
                     LuongCung = PayrollPeriodDetailDto.LuongCung,
                     SoNgayLam = PayrollPeriodDetailDto.SoNgayLam,
                     TroCapKhac = PayrollPeriodDetailDto.TroCapKhac,
                     ChiKhac = PayrollPeriodDetailDto.ChiKhac,
                     BaoHiemXaHoi = PayrollPeriodDetailDto.BaoHiemXaHoi,
                     PhepTon = PayrollPeriodDetailDto.PhepTon,
                     TongUng = PayrollPeriodDetailDto.TongUng,
                     TongTru = PayrollPeriodDetailDto.TongTru,
                     Thuong = PayrollPeriodDetailDto.Thuong,
                     TienAn = PayrollPeriodDetailDto.TienAn,
                     TienVe = PayrollPeriodDetailDto.TienVe,
                     DienThoai = PayrollPeriodDetailDto.DienThoai,
                     QuaDem = PayrollPeriodDetailDto.QuaDem,
                     Luat = PayrollPeriodDetailDto.Luat,
                     LuongHangVe = PayrollPeriodDetailDto.LuongHangVe,
                     LuongThucNhan = PayrollPeriodDetailDto.LuongThucNhan,
                     LamThemNgayThuong = PayrollPeriodDetailDto.LamThemNgayThuong,
                     LamThemNgayNghi = PayrollPeriodDetailDto.LamThemNgayNghi,
                     LamThemNgayLe = PayrollPeriodDetailDto.LamThemNgayLe,
                     TyLeHuongLuong = PayrollPeriodDetailDto.TyLeHuongLuong,
                     DoanhSo = PayrollPeriodDetailDto.DoanhSo,
                     NghiPhep = PayrollPeriodDetailDto.NghiPhep,
                     NghiKhongLuong = PayrollPeriodDetailDto.NghiKhongLuong,
                     GhiChu = PayrollPeriodDetailDto.GhiChu,
                     ChiTietDebit = PayrollPeriodDetailDto.ChiTietDebit,
                     ChiTietNghiPhep = PayrollPeriodDetailDto.ChiTietNghiPhep,
                     ChiTietPhieuChi = PayrollPeriodDetailDto.ChiTietPhieuChi,
                     ChiTietKhoanChi = PayrollPeriodDetailDto.ChiTietKhoanChi,
                     CreatedBy = userId,
                     CreatedAt = now,
                     UpdatedAt = now,
                 };
                 _context.PayrollPeriodDetails.Add(payrollPeriodDetail);
                 await _context.SaveChangesAsync();

                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.Message, null);
            }
        }
        [HttpPost("salary")]
        public IActionResult GetSalary([FromQuery] PayrollPeriodDto payrollPeriodDto = null)
        {
             var fileUrl = "https://admin.vudaco.online/salary/SalaryByCycleName";

             return ApiResponseResult(true, "Lấy dữ liệu thành công", new { fileUrl });
        }
        [HttpPost("SalaryByCycleName")]
        public IActionResult SalaryByCycleName([FromQuery] PayrollPeriodDto payrollPeriodDto = null)
        {
             var fileUrl = "https://admin.vudaco.online/salary/SalaryByCycleName";

             return ApiResponseResult(true, "Lấy dữ liệu thành công", new { fileUrl });
        }
      
    }
}
