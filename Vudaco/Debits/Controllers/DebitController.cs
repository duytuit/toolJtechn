using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Models;
using Vudaco.Controllers;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Debits.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;
using System.Text.Json;
using Vudaco.ContractFiles.Repositories;
using Vudaco.ContractFiles.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Receipts.Repositories;
using System.IO;
using ClosedXML.Excel;
using Vudaco.Shares;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Dynamic;
using System.Data;
using Vudaco.Notifys.Repositories;
using Vudaco.Notifys.Dtos;

namespace Vudaco.Debits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitController : BaseApiController
    {
        private readonly IDebitRepositories _repoDebit;
        private readonly IContractFileDetailRepository _repoContractFileDetail;
        private readonly IContractFileRepository _repoContractFile;
        private readonly ILogger<DebitController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IFcmQueue _fcmQueue;
        private readonly IConfiguration _configuration;
        public int userId => (int)HttpContext.Items["UserId"];

        public DebitController(ILogger<DebitController> logger,  IContractFileDetailRepository repoContractFileDetail,IContractFileRepository repoContractFile,IConfiguration configuration, IDebitRepositories repoDebit, VudacoDBContext context, IFcmQueue fcmQueue)
        {
            _logger = logger;
            _repoDebit = repoDebit;
            _context = context;
            _configuration = configuration;
            _fcmQueue = fcmQueue;
            _repoContractFileDetail = repoContractFileDetail;
            _repoContractFile = repoContractFile;
        }
        [HttpGet("noDebitNoFileNCC")]
        public async Task<IActionResult> GetNoDebitNoFileNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectNoDebitNoFileNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectBaoCaoDoanhThuAsync")]
        public async Task<IActionResult> GetObjectBaoCaoDoanhThuAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectBaoCaoDoanhThuAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("hasDebitNoFileNCC")]
        public async Task<IActionResult> GetHasDebitNoFileNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectHasDebitNoFileNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        
        [HttpGet("noDebitNoFileDispatchKH")]
        public async Task<IActionResult> GetNoDebitNoFileDispatchKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectNoDebitDispatchNoFileKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("hasDebitNoFileDispatchKH")]
        public async Task<IActionResult> GetHasDebitNoFileDispatchKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectHasDebitDispatchNoFileKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congNoGiaoNhan")]
        public async Task<IActionResult> GetCongNoGiaoNhan(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitGiaoNhanAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congNoLaiXe")]
        public async Task<IActionResult> GetCongNoLaiXe(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitLaiXeAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectDebitLaiXeTinhLuongAsync")]
        public async Task<IActionResult> GetObjectDebitLaiXeTinhLuongAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitLaiXeTinhLuongAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectTheoDoiNhacNoKHAsync")]
        public async Task<IActionResult> GetObjectTheoDoiNhacNoKHAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectTheoDoiNhacNoKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congnotonghopkh")]
        public async Task<IActionResult> GetCongNoTongHopKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitTongHopKHAsync(DebitDto, page, pageSize, cancellationToken);

            var data = ((IEnumerable<dynamic>)result.Extra["congnotonghop_kh"]).Select(x => new
                            {
                                x.customer_detail_id,
                                x.debit_total,
                                x.receipt_total,
                                type = (int)x.type 
                            }).ToList();
            var data_dv = data.Where(x => new[] { 0, 1, 4, 5, 8 }.Contains(x.type))
                            .GroupBy(x => x.customer_detail_id)
                            .Select(g => new
                            {
                                customer_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
           
            var data_ch = data.Where(x => new[] { 2, 3, 6 }.Contains(x.type))
                            .GroupBy(x => x.customer_detail_id)
                            .Select(g => new
                            {
                                customer_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
             var data_dk = ((IEnumerable<dynamic>)result.Extra["congnotonghop_dk_kh"]).Select(x => new
                            {
                                x.customer_detail_id,
                                x.debit_total,
                                x.receipt_total,
                                type = (int)x.type 
                            }).ToList();
            var data_dv_dk = data_dk.Where(x => new[] { 0, 1, 4, 5, 8 }.Contains(x.type))
                            .GroupBy(x => x.customer_detail_id)
                            .Select(g => new
                            {
                                customer_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
            var data_ch_dk = data_dk.Where(x => new[] { 2, 3, 6 }.Contains(x.type))
                            .GroupBy(x => x.customer_detail_id)
                            .Select(g => new
                            {
                                customer_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
              // Kiểm tra receipt liên quan
         
           var customers = await _context.PartnerDetails
                .Where(d => d.Status == 1 && d.StorageId == DebitDto.StorageId)
                .Join(
                    _context.Partners,
                    pd => pd.PartnerId,
                    p => p.Id,
                    (pd, p) => new
                    {
                        pd.Id,
                        p.Abbreviation,
                        p.Name
                    }
                )
                .ToListAsync();
            List<CongNoTongHopDto> cnth = new List<CongNoTongHopDto>();

           foreach (var item in customers)
            {
                var _data_dv = data_dv.FirstOrDefault(x=>x.customer_detail_id == item.Id);
                var _data_ch = data_ch.FirstOrDefault(x=>x.customer_detail_id == item.Id);
                var _data_dv_dk = data_dv_dk.FirstOrDefault(x=>x.customer_detail_id == item.Id);
                var _data_ch_dk = data_ch_dk.FirstOrDefault(x=>x.customer_detail_id == item.Id);

                var DVDK = _data_dv_dk?.debit_total ?? 0;
                var CHDK = _data_ch_dk?.debit_total ?? 0;
                var TTDVDK = _data_dv_dk?.receipt_total ?? 0;
                var TTCHDK = _data_ch_dk?.receipt_total ?? 0;

                var DVTK = _data_dv?.debit_total ?? 0;
                var CHTK = _data_ch?.debit_total ?? 0;
                var TTDVTK = _data_dv?.receipt_total ?? 0;
                var TTCHTK = _data_ch?.receipt_total ?? 0;
               // nếu tất cả đều = 0 → bỏ
                if (
                    DVDK == 0 && CHDK == 0 &&
                    TTDVDK == 0 && TTCHDK == 0 &&
                    DVTK == 0 && CHTK == 0 &&
                    TTDVTK == 0 && TTCHTK == 0
                )
                    continue;
                cnth.Add(new CongNoTongHopDto
                {
                    Id = item.Id,
                    Abbreviation = item.Abbreviation,
                    Name = item.Name,

                    DVDK = DVDK,
                    CHDK = CHDK,
                    TTDVDK = TTDVDK,
                    TTCHDK = TTCHDK,

                    DVTK = DVTK,
                    CHTK = CHTK,
                    TTDVTK = TTDVTK,
                    TTCHTK = TTCHTK,

                    DVCK = (DVDK - TTDVDK) + (DVTK - TTDVTK),
                    CHCK = (CHDK - TTCHDK) + (CHTK - TTCHTK),
                    CK = (DVDK - TTDVDK) + (DVTK - TTDVTK) + (CHDK - TTCHDK) + (CHTK - TTCHTK)
                });
            }
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", cnth);
        }
        [HttpGet("congnotonghopncc")]
        public async Task<IActionResult> GetObjectDebitTongHopNCCAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitTongHopNCCAsync(DebitDto, page, pageSize, cancellationToken);

            var data = ((IEnumerable<dynamic>)result.Extra["congnotonghop_ncc"]).Select(x => new
                            {
                                x.supplier_detail_id,
                                x.debit_total,
                                x.receipt_total,
                                type = (int)x.type 
                            }).ToList();
            var data_dv = data.Where(x => new[] { 0, 1, 4, 7, 10 ,12 }.Contains(x.type))
                            .GroupBy(x => x.supplier_detail_id)
                            .Select(g => new
                            {
                                supplier_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
           
            var data_ch = data.Where(x => new[] { 2, 3, 11 }.Contains(x.type))
                            .GroupBy(x => x.supplier_detail_id)
                            .Select(g => new
                            {
                                supplier_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
             var data_dk = ((IEnumerable<dynamic>)result.Extra["congnotonghop_dk_ncc"]).Select(x => new
                            {
                                x.supplier_detail_id,
                                x.debit_total,
                                x.receipt_total,
                                type = (int)x.type 
                            }).ToList();
            var data_dv_dk = data_dk.Where(x => new[] { 0, 1, 4, 7, 10 ,12 }.Contains(x.type))
                            .GroupBy(x => x.supplier_detail_id)
                            .Select(g => new
                            {
                                supplier_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
            var data_ch_dk = data_dk.Where(x => new[] { 2, 3, 11 }.Contains(x.type))
                            .GroupBy(x => x.supplier_detail_id)
                            .Select(g => new
                            {
                                supplier_detail_id = g.Key,
                                debit_total = g.Sum(x => x.debit_total),
                                receipt_total = g.Sum(x => x.receipt_total)
                            }).ToList();
              // Kiểm tra receipt liên quan
         
           var customers = await _context.PartnerDetails
                .Where(d => d.Status == 2 && d.StorageId == DebitDto.StorageId)
                .Join(
                    _context.Partners,
                    pd => pd.PartnerId,
                    p => p.Id,
                    (pd, p) => new
                    {
                        pd.Id,
                        p.Abbreviation,
                        p.Name
                    }
                )
                .ToListAsync();
            List<CongNoTongHopDto> cnth = new List<CongNoTongHopDto>();

           foreach (var item in customers)
            {
                var _data_dv = data_dv.FirstOrDefault(x=>x.supplier_detail_id == item.Id);
                var _data_ch = data_ch.FirstOrDefault(x=>x.supplier_detail_id == item.Id);
                var _data_dv_dk = data_dv_dk.FirstOrDefault(x=>x.supplier_detail_id == item.Id);
                var _data_ch_dk = data_ch_dk.FirstOrDefault(x=>x.supplier_detail_id == item.Id);

                var DVDK = _data_dv_dk?.debit_total ?? 0;
                var CHDK = _data_ch_dk?.debit_total ?? 0;
                var TTDVDK = _data_dv_dk?.receipt_total ?? 0;
                var TTCHDK = _data_ch_dk?.receipt_total ?? 0;

                var DVTK = _data_dv?.debit_total ?? 0;
                var CHTK = _data_ch?.debit_total ?? 0;
                var TTDVTK = _data_dv?.receipt_total ?? 0;
                var TTCHTK = _data_ch?.receipt_total ?? 0;
               // nếu tất cả đều = 0 → bỏ
                if (
                    DVDK == 0 && CHDK == 0 &&
                    TTDVDK == 0 && TTCHDK == 0 &&
                    DVTK == 0 && CHTK == 0 &&
                    TTDVTK == 0 && TTCHTK == 0
                )
                    continue;
                cnth.Add(new CongNoTongHopDto
                {
                    Id = item.Id,
                    Abbreviation = item.Abbreviation,
                    Name = item.Name,

                    DVDK = DVDK,
                    CHDK = CHDK,
                    TTDVDK = TTDVDK,
                    TTCHDK = TTCHDK,

                    DVTK = DVTK,
                    CHTK = CHTK,
                    TTDVTK = TTDVTK,
                    TTCHTK = TTCHTK,

                    DVCK = (DVDK - TTDVDK) + (DVTK - TTDVTK),
                    CHCK = (CHDK - TTCHDK) + (CHTK - TTCHTK),
                    CK = (DVDK - TTDVDK) + (DVTK - TTDVTK) + (CHDK - TTCHDK) + (CHTK - TTCHTK)
                });
            }
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", cnth);
        }
        [HttpGet("GetObjectDebitChiTietNoBillKHAsync")]
        public async Task<IActionResult> GetObjectDebitChiTietNoBillKHAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitChiTietNoBillKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectBaoCaoLoiNhuanAsync")]
        public async Task<IActionResult> GetObjectBaoCaoLoiNhuanAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            var group_result = new List<dynamic>();
            var result = await _repoDebit.GetObjectLoiNhuanXeTrongAsync(DebitDto, page, pageSize, cancellationToken);
            group_result.Add(new {
                name = "loinhuanxetrong",
                data = result.Extra
            });
            result = await _repoDebit.GetObjectLoiNhuanXeNgoaiAsync(DebitDto, page, pageSize, cancellationToken);
            group_result.Add(new {
                name = "loinhuanxengoai",
                data = result.Data
            });
            result = await _repoDebit.GetObjectLoiNhuanHaiQuanAsync(DebitDto, page, pageSize, cancellationToken);
            group_result.Add(new {
                name = "loinhuanhaiquan",
                data = result.Data
            });
            result = await _repoDebit.GetObjectLoiNhuanDoanhThuKhacAsync(DebitDto, page, pageSize, cancellationToken);
            group_result.Add(new {
                name = "loinhuandoanhthukhac",
                data = result.Extra
            });
            return ApiResponseResult(true, "Lấy dữ liệu thành công", group_result);
        }
        [HttpGet("GetObjectDebitChiTietHasBillKHAsync")]
        public async Task<IActionResult> GetObjectDebitChiTietHasBillKHAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            var result = await _repoDebit.GetObjectDebitChiTietHasBillKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congnochitietkh")]
        public async Task<IActionResult> GetCongNoChiTietKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitChiTietKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectDebitDuNoDKKHAsync")]
        public async Task<IActionResult> GetObjectDebitDuNoDKKHAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDuNoDKKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectDebitDuNoDKNCCAsync")]
        public async Task<IActionResult> GetObjectDebitDuNoDKNCCAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDuNoDKNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("excel/dieuxe")]
        public async Task<IActionResult> ExportDieuXe(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
         {
              try
                {
                    var result = await _repoContractFileDetail
                        .GetObjectFileHasDispatchAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
                    var data = result.Data.Select(x => (dynamic)x);
                    if (data == null || !data.Any())
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                    var list_kh = await _context.PartnerDetails
                    .Where(x => x.StorageId == FileInfoDetailDto.StorageId)
                    .Join(
                        _context.Partners,
                        pd => pd.PartnerId,
                        p => p.Id,
                        (pd, p) => new
                        {
                            p.Abbreviation,
                            p.Name,
                            pd.Id
                        }
                    ).ToListAsync();
                    var list_employee = await _context.Employees.Where(x => x.StorageId == FileInfoDetailDto.StorageId).ToListAsync();
          
                    // 🔹 List<object> → DataTable
                    var dataTable = Helper.ToDataTable(data);
                    DataTable table = new DataTable();
                    var columnDisplayMap = new Dictionary<string, string>
                    {
                        ["supplier_detail_id"] = "Nhà cung cấp",
                        ["file_info_id"] = "Hồ sơ",
                        ["employee_staff_id"] = "Nhân viên xử lý",
                        ["employee_driver_id"] = "Tài xế",
                        ["storage_id"] = "Kho",
                        ["service_id"] = "Dịch vụ",
                        ["service_detail"] = "Chi tiết dịch vụ",
                        ["type"] = "Loại",
                        ["dispatch_code"] = "Mã điều xe",
                        ["name"] = "Tên",
                        ["accounting_date"] = "Ngày hạch toán",
                        ["service_date"] = "Ngày dịch vụ",
                        ["service_status"] = "Trạng thái dịch vụ",
                        ["purchase_accounting_date"] = "Ngày hạch toán mua",
                        ["purchase_price"] = "Giá mua",
                        ["purchase_vat"] = "VAT mua",
                        ["price"] = "Giá bán",
                        ["vat"] = "VAT bán",
                        ["purchase_com"] = "Hoa hồng mua",
                        ["price_com"] = "Hoa hồng bán",
                        ["driver_fee"] = "Phí tài xế",
                        ["meal_fee"] = "Phí ăn uống",
                        ["ticket_fee"] = "Phí vé",
                        ["overnight_fee"] = "Phí qua đêm",
                        ["penalty_fee"] = "Phí phạt",
                        ["goods_fee"] = "Phí hàng hóa",
                        ["purchase_status"] = "Trạng thái mua",
                        ["status"] = "Trạng thái",
                        ["data"] = "Dữ liệu",
                        ["purchase_bill"] = "Hóa đơn mua",
                        ["bill"] = "Hóa đơn",
                        ["link_bill"] = "Link hóa đơn",
                        ["code_bill"] = "Mã hóa đơn",
                        ["note"] = "Ghi chú",
                        ["purchase_note"] = "Ghi chú mua",
                        ["customer_vehicle_type"] = "Loại xe khách",
                        ["supplier_vehicle_type"] = "Loại xe NCC",
                        ["vehicle_number"] = "Biển số xe",
                        ["approved_by_user"] = "Người duyệt",
                        ["approval_time"] = "Thời gian duyệt",
                        ["cus_bill"] = "Hóa đơn KH",
                        ["cus_bill_date"] = "Ngày hóa đơn KH",
                        ["sup_bill"] = "Hóa đơn NCC",
                        ["sup_bill_date"] = "Ngày hóa đơn NCC",
                        ["created_by"] = "Người tạo",
                        ["updated_by"] = "Người cập nhật",
                        ["deleted_by"] = "Người xóa",
                        ["deleted_at"] = "Ngày xóa",
                        ["created_at"] = "Ngày tạo",
                        ["updated_at"] = "Ngày cập nhật",
                        ["cf_note"] = "Ghi chú xác nhận",
                        ["cf_status"] = "Trạng thái xác nhận",
                        ["cf_status_confirm"] = "Trạng thái duyệt",
                        ["cf_updated_at"] = "Ngày xác nhận",
                        ["cf_updated_by"] = "Người xác nhận",

                        // 🔹 CỘT MAP THÊM
                        ["customer"] = "Khách hàng",
                        ["employee_driver"] = "Tên tài xế",
                        ["fileNumber"] = "Số hồ sơ"
                    };
                var ignoreColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "data"
                };

                foreach (DataColumn col in dataTable.Columns)
                {
                    if (ignoreColumns.Contains(col.ColumnName))
                        continue;

                    table.Columns.Add(col.ColumnName, col.DataType);
                }
                table.Columns.Add("customer", typeof(string));
                    table.Columns.Add("employee_driver", typeof(string));
                    table.Columns.Add("fileNumber", typeof(string));
                    // table: bảng mới sau khi map (đã tạo sẵn cột)
                    // 🔹 Lookup cho nhanh 
                    var khDict = list_kh.ToDictionary( x => x.Id, x => $"{x.Abbreviation}" ); 
                    var employeeDict = list_employee.ToDictionary( x => x.Id, x => $"{x.LastName} {x.FirstName}" );// đổi field nếu khác );
                foreach (DataRow item in dataTable.Rows)
                {
                    var row = table.NewRow();

                    // ✅ copy cột gốc, BỎ cột data
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        if (col.ColumnName.Equals("data", StringComparison.OrdinalIgnoreCase))
                            continue;

                        row[col.ColumnName] = item[col.ColumnName];
                    }

                    // 🔹 MAP CUSTOMER
                    if (item.Table.Columns.Contains("customer_detail_id") &&
                        item["customer_detail_id"] != DBNull.Value &&
                        khDict.TryGetValue(Convert.ToInt32(item["customer_detail_id"]), out var customer))
                    {
                        row["customer"] = customer;
                    }
                    else
                    {
                        row["customer"] = "";
                    }

                    // 🔹 MAP EMPLOYEE
                    if (item.Table.Columns.Contains("employee_driver_id") &&
                        item["employee_driver_id"] != DBNull.Value &&
                        employeeDict.TryGetValue(Convert.ToInt32(item["employee_driver_id"]), out var employee))
                    {
                        row["employee_driver"] = employee;
                    }
                    else
                    {
                        row["employee_driver"] = "";
                    }

                    // 🔹 MAP FILE INFO (ASYNC)
                    if (item.Table.Columns.Contains("file_info_id") &&
                        item["file_info_id"] != DBNull.Value)
                    {
                        int fileInfoId = Convert.ToInt32(item["file_info_id"]);

                        var fileInfo = await _repoContractFile
                            .GetFileInfoByIdWithCacheAsync(fileInfoId, cancellationToken);

                        row["fileNumber"] = fileInfo?.FileNumber ?? "";
                    }
                    else
                    {
                        row["fileNumber"] = "";
                    }

                    table.Rows.Add(row);
                }
                using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Điều Xe");

                    // 🔹 Đổ DataTable vào Excel
                    worksheet.Cell(1, 1).InsertTable(table, true);
                    var headerRow = worksheet.FirstRow();
                    foreach (DataColumn col in table.Columns)
                    {
                        if (columnDisplayMap.TryGetValue(col.ColumnName, out var displayName))
                        {
                            headerRow.Cell(col.Ordinal + 1).Value = displayName;
                        }
                    }

                // 🔹 Format header
                worksheet.FirstRow()
                    .Style.Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Columns().AdjustToContents();
                using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DieuXe_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    );
                }
                catch (Exception ex)
                {
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() > 0);

                    var errorMessage = frame != null
                        ? $"Error at {frame.GetFileName()}:{frame.GetFileLineNumber()} in {frame.GetMethod()?.Name} - {ex.Message}"
                        : ex.Message;

                    _logger.LogError(ex, errorMessage);
                    return ApiResponseResult<object>(false, errorMessage, null);
                }

        }
        [HttpGet("excel/congnokh")]
        public async Task<IActionResult> ExportCongNoKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
              try
              {
                // ==== LẤY THÔNG TIN KHÁCH HÀNG ====
               
                var kh = await _context.PartnerDetails.FirstOrDefaultAsync(x => x.Id == DebitDto.CustomerDetailId);
                if (kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu khách hàng", null);
               
                var info_kh = await _context.Partners.FirstOrDefaultAsync(x => x.Id == kh.PartnerId);
                if (info_kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu khách hàng", null);

                var result = await _repoDebit.GetObjectDebitChiTietKHAsync(DebitDto, page, pageSize, cancellationToken);
                if (result == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                // Chuẩn hóa dữ liệu về dynamic để đọc property
                var data = result.Data.Select(x => (dynamic)x);

                var groupedData = data
                .Where(x => x.file_info_id != null)
                .OrderByDescending(x => x.type == 1)   // ⭐ Ưu tiên type = 1 TRƯỚC
                .GroupBy(x => (long?)x.file_info_id)
                .Select(g => new
                {
                    file_info_id = g.Key,
                    Items = g.ToList()
                })
                .ToList();
                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", groupedData);
                // Dữ liệu không có file
                var groupedDataNoFile = data
                    .Where(x => x.file_info_id == null)
                    .ToList();
                // Merge vào chung loại anonymous type
                if (groupedDataNoFile.Any())
                {
                    groupedData.Add(new
                    {
                        file_info_id = (long?)null,         // cùng long?
                        Items = groupedDataNoFile
                    });
                }
                groupedData = groupedData
                .OrderBy(g => g.Items.Min(x => (DateTime)x.accounting_date))
                .ToList();
                var fileIds = groupedData
                    .Where(g => g.file_info_id != null)
                    .Select(g => g.file_info_id.Value)
                    .ToList();

                var list_file = await _context.FileInfos
                    .Where(x => fileIds.Contains(x.Id))
                    .ToListAsync();
                
                // ==== TẠO FILE EXCEL ====
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("bảng kê chi tiết");
                // ==== TIÊU ĐỀ ====
                ws.Range("A1:Q1").Merge();
                ws.Cell("A1").Value = "BẢNG KÊ CHI TIẾT";
                ws.Cell("A1").Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                string tg = string.Format("Từ ngày {0} - Đến ngày {1}", DebitDto.FromDate?.ToString("dd/MM/yyyy"), DebitDto.ToDate?.ToString("dd/MM/yyyy"));
                ws.Range("A2:Q2").Merge();
                ws.Cell("A2").Value = tg;
                ws.Cell("A2").Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Range("A3:K3").Merge(); // merge trống theo mẫu

                // ==== THÔNG TIN ĐƠN VỊ ====

                var cell_benban = ws.Cell("A5");
                cell_benban.Clear();
                cell_benban.GetRichText().AddText("Đơn vị bán hàng: ")
                    .SetBold();
                cell_benban.GetRichText().AddText("Công ty TNHH VUDACO");
                ws.Cell("A6").Value = "Địa chỉ: Số 6C/195 Kiều Hạ, Phường Đông Hải, Thành Phố Hải Phòng, Việt Nam";
                ws.Cell("A7").Value = "MST: 0201723721";

                var cell_benmua = ws.Cell("A9");
                cell_benmua.Clear();
                cell_benmua.GetRichText().AddText("Đơn vị mua hàng: ")
                    .SetBold();
                cell_benmua.GetRichText().AddText(info_kh?.Name ?? "");
                ws.Cell("A10").Value = "Địa chỉ: "+info_kh?.Address?? "";
                ws.Cell("A11").Value = "Mã số thuế: "+info_kh?.TaxCode?? "";


                // ==== HEADER BẢNG ====
                // bắt đầu từ dòng 13, header chiếm 2 dòng: 13 và 14
                int headerRow1 = 13;
                int headerRow2 = 14;

                // merge các cột (dọc 2 dòng)
                ws.Range(headerRow1, 1, headerRow2, 1).Merge().Value = "STT";
                ws.Range(headerRow1, 2, headerRow2, 2).Merge().Value = "NGÀY";
                ws.Range(headerRow1, 3, headerRow2, 3).Merge().Value = "LOẠI XE";
                ws.Range(headerRow1, 4, headerRow2, 4).Merge().Value = "Số xe";
                ws.Range(headerRow1, 5, headerRow2, 5).Merge().Value = "TUYẾN VẬN CHUYỂN";
                ws.Range(headerRow1, 6, headerRow2, 6).Merge().Value = "ĐƠN VỊ";
                ws.Range(headerRow1, 7, headerRow2, 7).Merge().Value = "SỐ LƯỢNG";
                ws.Range(headerRow1, 8, headerRow2, 8).Merge().Value = "ĐƠN GIÁ";
                ws.Range(headerRow1, 9, headerRow2, 9).Merge().Value = "THUẾ SUẤT";
                ws.Range(headerRow1, 10, headerRow2, 10).Merge().Value = "TIỀN THUẾ GTGT";
                ws.Range(headerRow1, 11, headerRow2, 11).Merge().Value = "THÀNH TIỀN";
                ws.Range(headerRow1, 12, headerRow2, 12).Merge().Value = "CHI HỘ";
                ws.Range(headerRow1, 13, headerRow2, 13).Merge().Value = "TỔNG CỘNG";
                ws.Range(headerRow1, 14, headerRow2, 14).Merge().Value = "SỐ FILE";
                ws.Range(headerRow1, 15, headerRow2, 15).Merge().Value = "Số bill/booking (hoặc Số tờ khai)";
                ws.Range(headerRow1, 16, headerRow2, 16).Merge().Value = "Số HĐ";
                ws.Range(headerRow1, 17, headerRow2, 17).Merge().Value = "ND chi hộ";
                ws.Range(headerRow1, 18, headerRow2, 18).Merge().Value = "Ghi chú";

                // format chung cho header
                var headerRange = ws.Range(headerRow1, 1, headerRow2, 18);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                // Set width cho toàn bộ cột từ A → Q
                ws.Column(1).Width = 5;   // STT
                ws.Column(2).Width = 12;  // NGÀY
                ws.Column(3).Width = 15;  // LOẠI XE
                ws.Column(4).Width = 12;  // Số xe
                ws.Column(5).Width = 25;  // TUYẾN VẬN CHUYỂN
                ws.Column(6).Width = 18;  // ĐƠN VỊ
                ws.Column(7).Width = 12;  // SỐ LƯỢNG
                ws.Column(8).Width = 15;  // ĐƠN GIÁ
                ws.Column(9).Width = 12;  // THUẾ SUẤT
                ws.Column(10).Width = 18; // TIỀN THUẾ GTGT
                ws.Column(11).Width = 18; // THÀNH TIỀN
                ws.Column(12).Width = 12; // CHI HỘ
                ws.Column(13).Width = 18; // TỔNG CỘNG
                ws.Column(14).Width = 12; // SỐ FILE
                ws.Column(15).Width = 30; // Số bill/booking (hoặc Số tờ khai)
                ws.Column(16).Width = 15; // Số HĐ
                ws.Column(17).Width = 28; // ND chi hộ
                ws.Column(18).Width = 28; // Ghi chú

                // Cho phép xuống dòng trong ô nếu text dài
                ws.Range(headerRow1, 1, headerRow2, 18).Style.Alignment.WrapText = true;
                // ==== DỮ LIỆU MẪU ====
                int startRow = 15;
                int currentRow = startRow;
                int row = startRow;
                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", groupedData);
                for (int i = 0; i < groupedData.Count; i++)
                {
                    var group = groupedData[i];

                    // Lấy bản ghi đầu tiên trong group (để lấy thông tin chung)
                    var first = group.Items.First();

                    if ( first.file_info_id > 0)
                    {
                         ContractFiles.Models.FileInfo _fileInfo = new ContractFiles.Models.FileInfo();
                        _fileInfo = list_file.FirstOrDefault(x => x.Id == (long)first.file_info_id);

                        int soLuong = group.Items.Where(x=> x.type == 1 && x.file_info_id == first.file_info_id).Count();
                        decimal price = data.Where(x => new int[] { 0,1, 4, 5 ,8}.Contains((int)x.type) && x.file_info_id == first.file_info_id).Sum(x => x.price + x.price_com);
                        decimal price_dv = data
                                .Where(x => new int[] { 0,1, 4, 5 ,8}.Contains((int)x.type) && x.file_info_id == first.file_info_id)
                                                .Sum(x =>
                                                {
                                                    decimal total_price = (decimal)x.price + (decimal)x.price_com;
                                                    decimal vat = (decimal)x.vat;
                                                    return total_price+ (total_price * vat / 100m); // giá + VAT
                                                });
                        decimal price_thue = data.Where(x => x.file_info_id == first.file_info_id).Sum(x =>((decimal)x.price + (decimal)x.price_com) * (decimal)x.vat / 100);
                        decimal price_ch = data.Where(x => new int[] { 2, 3, 6 }.Contains((int)x.type) && x.file_info_id == first.file_info_id)
                                                        .Sum(x =>
                                                        {
                                                            decimal price = (decimal)x.price;
                                                            decimal vat = (decimal)x.vat;
                                                            return price + (price * vat / 100m); // giá + VAT
                                                        });
                        decimal thanhtien = data.Where(x => x.file_info_id == first.file_info_id).Sum(x =>
                        {
                            decimal total_price = (decimal)x.price + (decimal)x.price_com;
                            decimal vat = (decimal)x.vat;
                            return total_price+ (total_price * vat / 100m); // giá + VAT
                        }
                        );
                    // return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                        ws.Cell(row, 1).Value = i + 1; // STT
                        ws.Cell(row, 2).Value = first.accounting_date.ToString("dd/MM/yyyy");
                        ws.Cell(row, 3).Value = first.customer_vehicle_type ?? "";
                        ws.Cell(row, 4).Value = first.vehicle_number ?? "";
                        ws.Cell(row, 5).Value = first.name;
                        ws.Cell(row, 6).Value = "Chuyến";
                        ws.Cell(row, 7).Value = soLuong; // count

                        ws.Cell(row, 8).Value = price;
                        ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(row, 9).Value = first.vat; // thuế suất, nếu cố định thì gán %
                        ws.Cell(row, 10).Value = price_thue;
                        ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(row, 11).Value = price_dv;
                        ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(row, 12).Value = price_ch;
                        ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0";

                        ws.Cell(row, 13).Value = thanhtien;
                        ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(row, 14).Value = _fileInfo != null ? (_fileInfo.FileNumber??"") : first.dispatch_code ?? "";
                        ws.Cell(row, 15).Value = _fileInfo != null ? (_fileInfo.Bill ?? _fileInfo.Declaration ?? "") : "";
                        ws.Cell(row, 16).Value = first.cus_bill ?? "";
                        // Nối TenDichVu + ThanhTien
                        var dichVuStr = string.Join("; ",
                            data.Where(x => new int[] { 2, 3, 6 }.Contains((int)x.type) && x.file_info_id == first.file_info_id)
                                .Select(x =>
                                {
                                    decimal price = (decimal)x.price;
                                    decimal vat = (decimal)x.vat;

                                    decimal total = price + (price * vat / 100m); // giá + VAT

                                    return $"{x.name}: {total:N0}";
                                })
                        );
                        ws.Cell(row, 17).Value = dichVuStr;
                        // Nối GhiChu
                        var ghiChuStr = string.Join("; ",
                                data.Where(x => !string.IsNullOrEmpty((string)x.note) && x.file_info_id == first.file_info_id)
                                .Select(x => (string)x.note)
                        );
                        ws.Cell(row, 18).Value = ghiChuStr;
                        row++;
                    }
                    else
                    {
                        foreach (var item in group.Items)
                        {
                            //check_error= item.toString();
                            //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", item);
                            // Tính giá
                            decimal price_dv = (decimal)item.price + (decimal)item.price_com;
                            decimal price = (decimal)item.price;

                            decimal vatAmount_dv = price_dv * (decimal)item.vat / 100m;
                            decimal vatAmount = price * (decimal)item.vat / 100m;

                            decimal total_price_dv = new int[] { 0,1, 4, 5,8}.Contains((int)item.type)
                                ? (price_dv + vatAmount_dv)
                                : 0;

                            decimal price_thue_dv = vatAmount_dv;

                            decimal price_ch = new int[] { 2, 3, 6 }.Contains((int)item.type)
                                ? (price + vatAmount)
                                : 0;

                            ws.Cell(row, 1).Value = i + 1; // STT
                            ws.Cell(row, 2).Value = item.accounting_date.ToString("dd/MM/yyyy");
                            ws.Cell(row, 3).Value = item.customer_vehicle_type ?? "";
                            ws.Cell(row, 4).Value = item.vehicle_number ?? "";
                            ws.Cell(row, 5).Value = item.name;
                            ws.Cell(row, 6).Value = "Chuyến";
                            ws.Cell(row, 7).Value = 1; // count

                            ws.Cell(row, 8).Value = (item.type == 0 || item.type == 1 || item.type == 4 || item.type == 5|| item.type == 8)? price_dv:0;
                            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 9).Value = item.vat; // thuế suất, nếu cố định thì gán %
                            ws.Cell(row, 10).Value = (item.type == 0 || item.type == 1 || item.type == 4 || item.type == 5|| item.type == 8)? vatAmount_dv:0;
                            ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 11).Value = total_price_dv;
                            ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 12).Value = price_ch;
                            ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 13).Value = price_dv + vatAmount_dv;
                            ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(row, 14).Value = item.dispatch_code ?? "";
                            ws.Cell(row, 15).Value = "";
                            ws.Cell(row, 16).Value = item.cus_bill ?? "";
                            row++;
                        }
                    }
                   //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                        // Tính tổng và count
                }
                int dataStartRow = 15;
                int dataEndRow = ws.LastRowUsed().RowNumber();
                int totalRow = dataEndRow + 1;
                // range dữ liệu (A..Q)
                var dataRange = ws.Range(dataStartRow, 1, dataEndRow, 17);
                // set border all
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                // ghi chữ "TỔNG CỘNG"
                ws.Range(totalRow, 1, totalRow, 7).Merge();
                ws.Cell(totalRow, 1).Value = "TỔNG CỘNG";
                ws.Range(totalRow, 1, totalRow, 7).Style.Font.Bold = true;
                ws.Range(totalRow, 1, totalRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // công thức SUM cho các cột cần cộng
                ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H{dataStartRow}:H{dataEndRow})";   // ĐƠN GIÁ
                ws.Cell(totalRow, 8).Style.NumberFormat.Format = "#,##0"; // format có dấu phẩy
                // ws.Cell(totalRow, 9).FormulaA1 = $"SUM(I{dataStartRow}:I{dataEndRow})";   // THUẾ SUẤT
                ws.Cell(totalRow, 10).FormulaA1 = $"SUM(J{dataStartRow}:J{dataEndRow})";  // TIỀN THUẾ GTGT
                ws.Cell(totalRow, 10).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 11).FormulaA1 = $"SUM(K{dataStartRow}:K{dataEndRow})";  // THÀNH TIỀN
                ws.Cell(totalRow, 11).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 12).FormulaA1 = $"SUM(L{dataStartRow}:L{dataEndRow})";  // CHI HỘ
                ws.Cell(totalRow, 12).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 13).FormulaA1 = $"SUM(M{dataStartRow}:M{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 13).Style.NumberFormat.Format = "#,##0";

                // style cho dòng tổng
                var totalRange = ws.Range(totalRow, 1, totalRow, 18);
                totalRange.Style.Font.Bold = true;
                totalRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                var fullRange = ws.Range(dataStartRow, 1, totalRow, 18);
                fullRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                double tongCong = ws.Cell(totalRow, 13).GetDouble();
                dataEndRow = ws.LastRowUsed().RowNumber();
                // Lấy giá trị tổng cộng ở cột 13 (cột M)
                int textRow = dataEndRow + 1;
                // Thêm tiêu đề "Số tiền bằng chữ"
                ws.Range(textRow, 1, textRow, 7).Merge();
                ws.Cell(textRow, 1).Value = "Số tiền bằng chữ:";
                ws.Range(textRow, 1, textRow, 7).Style.Font.Bold = true;
                ws.Range(textRow, 1, textRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                ws.Range(textRow, 8, textRow, 17).Merge();
                ws.Cell(textRow, 8).Value = Helper.NumberToVietnameseWords(tongCong) + "./.";
                ws.Range(textRow, 8, textRow, 17).Style.Font.Bold = true;
                ws.Range(textRow, 8, textRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                fullRange = ws.Range(textRow, 1, textRow, 17);
                fullRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ==== EXPORT ====
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "BangKeChiTiet.xlsx"
                );
              }
              catch(Exception ex)
              {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // frame đầu tiên có thông tin lỗi
                var line = frame.GetFileLineNumber();
                var file = frame.GetFileName();
                var method = frame.GetMethod()?.Name;

                var errorMessage = $"Error at {file}:{line} in {method} - {ex.Message}";

                _logger.LogError(ex, errorMessage);

                return ApiResponseResult<object>(false, errorMessage, null);
              }

        }
        [HttpGet("excel/congnokh_v1")]
        public async Task<IActionResult> ExportCongNoKHVer1(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
              try
              {
                // ==== LẤY THÔNG TIN KHÁCH HÀNG ====
                var kh = await _context.PartnerDetails.FirstOrDefaultAsync(x => x.Id == DebitDto.CustomerDetailId);
                if (kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu khách hàng", null);

                var info_kh = await _context.Partners.FirstOrDefaultAsync(x => x.Id == kh.PartnerId);
                if (info_kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu khách hàng", null);

                var result = await _repoDebit.GetObjectDebitChiTietKHAsync(DebitDto, page, pageSize, cancellationToken);
                if (result == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                var result_duno_dauky = await _repoDebit.GetObjectDebitDuNoDKKHAsync(DebitDto, page, pageSize, cancellationToken);

                int duno = 0;

                if (result_duno_dauky != null && result_duno_dauky.Any())
                {
                    dynamic duno_dauky = result_duno_dauky.First();

                    int totalDebit = duno_dauky?.total_debit ?? 0;
                    int totalReceipt = duno_dauky?.total_receipt ?? 0;

                    duno = totalDebit - totalReceipt;
                }
                // Chuẩn hóa dữ liệu về dynamic để đọc property
                var data = result.Data.Select(x => (dynamic)x);

                // Lấy toàn bộ file_info_id dạng long? để kiểu đồng nhất
                var groupedData = data
                    .Where(x => x.file_info_id != null)
                    .GroupBy(x => (long?)x.file_info_id)     // ép về long? tại đây
                    .Select(g => new
                    {
                        file_info_id = g.Key,               // luôn là long?
                        Items = g.ToList()
                    })
                    .ToList();

                // Dữ liệu không có file
                var groupedDataNoFile = data
                    .Where(x => x.file_info_id == null)
                    .ToList();

                // Merge vào chung loại anonymous type
                foreach (var item in groupedDataNoFile)
                {
                    groupedData.Add(new
                    {
                        file_info_id = (long?)null,
                        Items = new List<dynamic> { item }
                    });
                }
               
                groupedData = groupedData
                .OrderBy(g => g.Items.Min(x => (DateTime)x.accounting_date))
                .ToList();
                var fileIds = groupedData
                    .Where(g => g.file_info_id != null)
                    .Select(g => g.file_info_id.Value)
                    .ToList();

                var list_file = await _context.FileInfos
                    .Where(x => fileIds.Contains(x.Id))
                    .ToListAsync();
                
                // ==== TẠO FILE EXCEL ====
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("bảng kê chi tiết");
                // ==== TIÊU ĐỀ ====
                ws.Range("A1:Q1").Merge();
                ws.Cell("A1").Value = "BẢNG KÊ CHI TIẾT";
                ws.Cell("A1").Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                string tg = string.Format("Từ ngày {0} - Đến ngày {1}", DebitDto.FromDate?.ToString("dd/MM/yyyy"), DebitDto.ToDate?.ToString("dd/MM/yyyy"));
                ws.Range("A2:Q2").Merge();
                ws.Cell("A2").Value = tg;
                ws.Cell("A2").Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Range("A3:K3").Merge(); // merge trống theo mẫu

                // ==== THÔNG TIN ĐƠN VỊ ====

                var cell_benban = ws.Cell("A4");
                cell_benban.Clear();
                cell_benban.GetRichText().AddText("Đơn vị bán hàng: ")
                    .SetBold();
                cell_benban.GetRichText().AddText("Công ty TNHH VUDACO");
                ws.Cell("A5").Value = "Địa chỉ: Số 6C/195 Kiều Hạ, Phường Đông Hải, Thành Phố Hải Phòng, Việt Nam";
                ws.Cell("A6").Value = "MST: 0201723721";

                var cell_benmua = ws.Cell("A8");
                cell_benmua.Clear();
                cell_benmua.GetRichText().AddText("Đơn vị mua hàng: ")
                    .SetBold();
                cell_benmua.GetRichText().AddText(info_kh?.Name ?? "");
                ws.Cell("A9").Value = "Địa chỉ: "+info_kh?.Address ?? "";
                ws.Cell("A10").Value = "Mã số thuế: "+info_kh?.TaxCode ?? "";

                ws.Range("A12:L12").Merge();
                ws.Range("A12:L12").Merge().Value = "Dư nợ đầu kỳ (0)";
                ws.Range("A12:L12").Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range("A12:L12").Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A12:L12").Merge().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("M12:O12").Merge();
                ws.Range("M12:O12").Merge().Value = duno == 0 ? 0 : duno;
                ws.Range("M12:O12").Merge().Style.NumberFormat.Format = "#,##0";
                ws.Range("M12:O12").Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range("M12:O12").Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range("M12:O12").Merge().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ==== HEADER BẢNG ====
                // bắt đầu từ dòng 13, header chiếm 2 dòng: 13 và 14
                int headerRow1 = 13;
                int headerRow2 = 14;

                // merge các cột (dọc 2 dòng)
                ws.Range(headerRow1, 1, headerRow2, 1).Merge().Value = "STT";
                ws.Range(headerRow1, 2, headerRow2, 2).Merge().Value = "Số file / Mã điều xe (1)";
                ws.Range(headerRow1, 3, headerRow2, 3).Merge().Value = "Số hóa đơn";
                ws.Range(headerRow1, 4, headerRow2, 4).Merge().Value = "Ngày hạch toán (2)";
                ws.Range(headerRow1, 5, headerRow2, 5).Merge().Value = "Số tờ khai (3)";
                ws.Range(headerRow1, 6, headerRow2, 6).Merge().Value = "Số bill (4)";
                ws.Range(headerRow1, 7, headerRow2, 7).Merge().Value = "Số cont";
                ws.Range(headerRow1, 8, headerRow2, 8).Merge().Value = "Số lượng (5)";
                ws.Range(headerRow1, 9, headerRow2, 9).Merge().Value = "Loại xe khách hàng";
                ws.Range(headerRow1, 10, headerRow2, 10).Merge().Value = "Biển số xe";
                ws.Range(headerRow1, 11, headerRow2, 11).Merge().Value = "Ghi chú";
                ws.Range(headerRow1, 12, headerRow2, 12).Merge().Value = "Nội dung (6)";
                ws.Range(headerRow1, 13, headerRow2, 13).Merge().Value = "Số tiền (7)";
                ws.Range(headerRow1, 14, headerRow2, 14).Merge().Value = "VAT (8)";
                ws.Range(headerRow1, 15, headerRow2, 15).Merge().Value = "Tổng cộng (9)";
                ws.Range(headerRow1, 16, headerRow2, 16).Merge().Value = "Chi hộ (10)";

                // format chung cho header
                var headerRange = ws.Range(headerRow1, 1, headerRow2, 16);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                // Set width cho toàn bộ cột từ A → Q
                ws.Column(1).Width = 5;   // STT
                ws.Column(2).Width = 12;  // Số file / Mã điều xe (1)
                ws.Column(3).Width = 15;  // Số hóa đơn
                ws.Column(4).Width = 12;  // Ngày hạch toán (2)
                ws.Column(5).Width = 25;  // Số tờ khai (3)
                ws.Column(6).Width = 18;  // Số bill (4)
                ws.Column(7).Width = 12;  // Số cont
                ws.Column(8).Width = 15;  // Số lượng (5)
                ws.Column(9).Width = 12;  // Loại xe khách hàng 
                ws.Column(10).Width = 18; // Biển số xe
                ws.Column(11).Width = 18; // Ghi chú
                ws.Column(12).Width = 12; // Nội dung (6)
                ws.Column(13).Width = 18; // Số tiền (7)
                ws.Column(14).Width = 12; // VAT (8)
                ws.Column(15).Width = 30; // Tổng cộng (9)
                ws.Column(16).Width = 15; // Chi hộ (10)    

                // Cho phép xuống dòng trong ô nếu text dài
                ws.Range(headerRow1, 1, headerRow2, 16).Style.Alignment.WrapText = true;
                // ==== DỮ LIỆU MẪU ====
                int startRow = 15;
                int currentRow = startRow;
                int row = startRow;
                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", groupedData);
                for (int i = 0; i < groupedData.Count; i++)
                {
                    var group = groupedData[i];

                    // Lấy bản ghi đầu tiên trong group (để lấy thông tin chung)
                    var first = group.Items.First();
                   　//return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                        // Tính tổng và count
                    ContractFiles.Models.FileInfo _fileInfo = new ContractFiles.Models.FileInfo();
                    if (first.file_info_id > 0)
                    {
                        _fileInfo = list_file.FirstOrDefault(x => x.Id == (long)first.file_info_id);
                    }
                  
                   // return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                    ws.Cell(row, 1).Value = i + 1; // STT
                    ws.Cell(row, 2).Value = _fileInfo != null ? (_fileInfo.FileNumber??first.dispatch_code) : first.dispatch_code ?? "";
                    ws.Cell(row, 3).Value = first.cus_bill ?? "";
                    ws.Cell(row, 4).Value = first.accounting_date.ToString("dd/MM/yyyy");
                    ws.Cell(row, 5).Value =  _fileInfo != null ? _fileInfo.Declaration ??"" : "";
                    ws.Cell(row, 6).Value =  _fileInfo != null ? _fileInfo.Bill ?? "" : "";
                    ws.Cell(row, 7).Value =  _fileInfo != null ? _fileInfo.ContainerCode ?? "" : "";
                    ws.Cell(row, 8).Value =  _fileInfo != null ? _fileInfo.Quantity ?? "" : "";
                    ws.Cell(row, 9).Value =  first.customer_vehicle_type ??"";
                   
                    row++;
                    // Lấy danh sách bản ghi của group theo file_info_id
                    if (first.file_info_id > 0)
                    {
                         var serviceList = data
                        .Where(x => x.file_info_id == first.file_info_id)
                        .OrderBy(x=>x.type)
                        .ToList();

                        // Nếu không có bản ghi → bỏ qua
                        if (!serviceList.Any())
                            continue;

                        // DUYỆT TỪNG BẢN GHI TRONG DATA (đúng theo mẫu bạn yêu cầu)
                        foreach (var svc in serviceList)
                        {
                            // Tính giá
                            decimal price_dv = (decimal)svc.price + (decimal)svc.price_com;
                            decimal price = (decimal)svc.price;

                            decimal vatAmount_dv = price_dv * (decimal)svc.vat / 100m;
                            decimal vatAmount = price * (decimal)svc.vat / 100m;

                            decimal total_price_dv = new int[] { 0,1, 4, 5,8}.Contains((int)svc.type)
                                ? (price_dv + vatAmount_dv)
                                : 0;

                            decimal price_thue_dv = vatAmount_dv;

                            decimal price_ch = new int[] { 2, 3, 6 }.Contains((int)svc.type)
                                ? (price + vatAmount)
                                : 0;

                            // --- GHI DÒNG CHI TIẾT ---
                            ws.Cell(row, 10).Value = svc.vehicle_number ??"";
                            ws.Cell(row, 11).Value = svc.note ?? "";
                            ws.Cell(row, 12).Value = svc.name ?? "";

                            ws.Cell(row, 13).Value = (svc.type == 0 || svc.type == 1 || svc.type == 4 || svc.type == 5|| svc.type == 8)? price_dv:0;
                            ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 14).Value = price_thue_dv;
                            ws.Cell(row, 14).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 15).Value = total_price_dv;
                            ws.Cell(row, 15).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 16).Value = price_ch;
                            ws.Cell(row, 16).Style.NumberFormat.Format = "#,##0";

                            row++;
                        }
                    }
                    else
                    {
                         // Tính giá
                            decimal price_dv = (decimal)first.price + (decimal)first.price_com;
                            decimal price = (decimal)first.price;

                            decimal vatAmount_dv = price_dv * (decimal)first.vat / 100m;
                            decimal vatAmount = price * (decimal)first.vat / 100m;

                            decimal total_price_dv = new int[] { 0,1, 4, 5,8}.Contains((int)first.type)
                                ? (price_dv + vatAmount_dv)
                                : 0;

                            decimal price_thue_dv = vatAmount_dv;

                            decimal price_ch = new int[] { 2, 3, 6 }.Contains((int)first.type)
                                ? (price + vatAmount)
                                : 0;

                            // --- GHI DÒNG CHI TIẾT ---
                            ws.Cell(row, 10).Value = first.vehicle_number ?? "";
                            ws.Cell(row, 11).Value = first.note ?? "";
                            ws.Cell(row, 12).Value = first.name ?? "";

                            ws.Cell(row, 13).Value = (first.type == 0 || first.type == 1 || first.type == 4 || first.type == 5|| first.type == 8)? price_dv:0;
                            ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 14).Value = price_thue_dv;
                            ws.Cell(row, 14).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 15).Value = total_price_dv;
                            ws.Cell(row, 15).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(row, 16).Value = price_ch;
                            ws.Cell(row, 16).Style.NumberFormat.Format = "#,##0";

                            row++;
                    }
                   
                                                
                }
          
                int dataStartRow = 15;
                int dataEndRow = ws.LastRowUsed().RowNumber();
                int totalRow = dataEndRow + 1;

                // ghi chữ "TỔNG CỘNG"
                ws.Range(totalRow, 1, totalRow, 12).Merge();
                ws.Cell(totalRow, 1).Value = "Tổng cộng";
                ws.Range(totalRow, 1, totalRow, 12).Style.Font.Bold = true;
                ws.Range(totalRow, 1, totalRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Tổng công nợ phải thu (12) =  (9) + (10)
                ws.Range(totalRow+1, 1, totalRow+1, 12).Merge();
                ws.Cell(totalRow+1, 1).Value = "Tổng công nợ phải thu (12) =  (9) + (10)";
                ws.Range(totalRow+1, 1, totalRow+1, 12).Style.Font.Bold = true;
                ws.Range(totalRow+1, 1, totalRow+1, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(totalRow, 13).FormulaA1 = $"SUM(M{dataStartRow}:M{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 13).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 14).FormulaA1 = $"SUM(N{dataStartRow}:N{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 14).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 15).FormulaA1 = $"SUM(O{dataStartRow}:O{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 15).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 16).FormulaA1 = $"SUM(P{dataStartRow}:P{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 16).Style.NumberFormat.Format = "#,##0";

                 ws.Range(totalRow+1, 13, totalRow+1, 15).Merge();
                ws.Range(totalRow+1, 13, totalRow+1, 15).Style.Font.Bold = true;
                ws.Range(totalRow+1, 13, totalRow+1, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(totalRow+1, 13).FormulaA1 = $"SUM(O{dataStartRow}:O{dataEndRow}) + SUM(P{dataStartRow}:P{dataEndRow})";
                ws.Cell(totalRow+1, 13).Style.NumberFormat.Format = "#,##0";
             
                dataEndRow = ws.LastRowUsed().RowNumber();
                totalRow = dataEndRow + 1;


                // range dữ liệu (A..Q)
                var dataRange = ws.Range(dataStartRow, 1, dataEndRow, 16);
                // set border all
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                // ==== EXPORT ====
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "BangKeChiTiet.xlsx"
                );
              }
              catch(Exception ex)
              {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // frame đầu tiên có thông tin lỗi
                var line = frame.GetFileLineNumber();
                var file = frame.GetFileName();
                var method = frame.GetMethod()?.Name;

                var errorMessage = $"Error at {file}:{line} in {method} - {ex.Message}";

                _logger.LogError(ex, errorMessage);

                return ApiResponseResult<object>(false, errorMessage, null);
              }

        }
         [HttpGet("excel/congnoncc_v1")]
        public async Task<IActionResult> ExportCongNoNCCVer1(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
              var check_error = "";
              try
              {
                // ==== LẤY THÔNG TIN NHÀ CUNG CẤP ====
                var kh = await _context.PartnerDetails.FirstOrDefaultAsync(x => x.Id == DebitDto.SupplierDetailId);
                if (kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu nhà cung cấp", null);

                var info_kh = await _context.Partners.FirstOrDefaultAsync(x => x.Id == kh.PartnerId);
                if (info_kh == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu nhà cung cấp", null);

                var result = await _repoDebit.GetObjectDebitChiTietNCCAsync(DebitDto, page, pageSize, cancellationToken);
                if (result == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", null);
                var result_duno_dauky = await _repoDebit.GetObjectDebitDuNoDKNCCAsync(DebitDto, page, pageSize, cancellationToken);
                decimal duno = 0m;

                if (result_duno_dauky != null && result_duno_dauky.Any())
                {
                    dynamic duno_dauky = result_duno_dauky.First();

                    decimal totalDebit = duno_dauky?.total_debit ?? 0m;
                    decimal totalReceipt = duno_dauky?.total_receipt ?? 0m;

                    duno = totalDebit - totalReceipt;
                }
             
                // Chuẩn hóa dữ liệu về dynamic để đọc property
                var data = result.Data.Select(x => (dynamic)x);

                // Lấy toàn bộ file_info_id dạng long? để kiểu đồng nhất
                var groupedData = data
                    .Where(x => x.type == 1 && x.file_info_id != null)
                    .GroupBy(x => (long?)x.file_info_id)     // ép về long? tại đây
                    .Select(g => new
                    {
                        file_info_id = g.Key,               // luôn là long?
                        Items = g.ToList(),
                        type = 0 // có file
                    })
                    .ToList();

                // Dữ liệu không có file
                var groupedDataNoFile = data
                    .Where(x => x.file_info_id == null)
                    .ToList();

                // Merge vào chung loại anonymous type
                foreach (var item in groupedDataNoFile)
                {
                    groupedData.Add(new
                    {
                        file_info_id = (long?)null,
                        Items = new List<dynamic> { item },
                        type = 1 // ko file
                    });
                }
                var list_receipt_detail = await _context.ReceiptDetails
                    .Where(x => x.StorageId == DebitDto.StorageId)
                    .Join(
                        _context.Receipts.Where(r => r.IncomeExpenseCategoryId == 14 && r.Object == 1 && r.ObjectId == DebitDto.SupplierDetailId),
                        pd => pd.ReceiptId,
                        p => p.Id,
                        (pd, p) => new
                        {
                            Id = p.Id,
                            accounting_date = p.AccountingDate,
                            Note = p.Note,
                            Amount = pd.Amount
                        }
                    ).ToListAsync();
                // Merge vào chung loại anonymous type
                foreach (var item in list_receipt_detail)
                {
                    groupedData.Add(new
                    {
                        file_info_id = (long?)null,
                        Items = new List<dynamic> { item },
                        type = 2 // chi tạm ứng tiền cho nhà cung cấp
                    });
                }
                // return ApiResponseResult<object>(false, null, groupedData);
                groupedData = groupedData
                .OrderBy(g => g.Items.Min(x => (DateTime)x.accounting_date))
                .ToList();
                var fileIds = groupedData
                    .Where(g => g.file_info_id != null)
                    .Select(g => g.file_info_id.Value)
                    .ToList();

                var list_file = await _context.FileInfos
                    .Where(x => fileIds.Contains(x.Id))
                    .ToListAsync();
                
                // ==== TẠO FILE EXCEL ====
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("bảng kê chi tiết");
                // ==== TIÊU ĐỀ ====
                ws.Range("A1:Q1").Merge();
                ws.Cell("A1").Value = "BẢNG KÊ CHI TIẾT PHẢI TRẢ";
                ws.Cell("A1").Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                string tg = string.Format("Từ ngày {0} - Đến ngày {1}", DebitDto.FromDate?.ToString("dd/MM/yyyy"), DebitDto.ToDate?.ToString("dd/MM/yyyy"));
                ws.Range("A2:Q2").Merge();
                ws.Cell("A2").Value = tg;
                ws.Cell("A2").Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Range("A3:K3").Merge(); // merge trống theo mẫu

                // ==== THÔNG TIN ĐƠN VỊ ====

                // var cell_benban = ws.Cell("A4");
                // cell_benban.Clear();
                // cell_benban.GetRichText().AddText("Đơn vị bán hàng: ")
                //     .SetBold();
                // cell_benban.GetRichText().AddText("Công ty TNHH VUDACO");
                // ws.Cell("A5").Value = "Địa chỉ: Số 6C/195 Kiều Hạ, Phường Đông Hải, Thành Phố Hải Phòng, Việt Nam";
                // ws.Cell("A6").Value = "MST: 0201723721";
              
                var cell_benmua = ws.Cell("A8");
                cell_benmua.Clear();
                cell_benmua.GetRichText().AddText("Nhà cung cấp: ")
                    .SetBold();
                cell_benmua.GetRichText().AddText(info_kh?.Name ?? "");
                ws.Cell("A9").Value = "Địa chỉ: "+info_kh?.Address ?? "";
                ws.Cell("A10").Value = "Mã số thuế: "+info_kh?.TaxCode ?? "";

                ws.Range("A12:G12").Merge();
                ws.Range("A12:G12").Merge().Value = "Dư nợ đầu kỳ (0)";
                ws.Range("A12:G12").Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range("A12:G12").Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range("A12:G12").Merge().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range("H12:N12").Merge();
                ws.Range("H12:N12").Merge().Value = duno == 0 ? 0 : duno;
                ws.Range("H12:N12").Merge().Style.NumberFormat.Format = "#,##0";
                ws.Range("H12:N12").Merge().Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range("H12:N12").Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range("H12:N12").Merge().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ==== HEADER BẢNG ====
                // bắt đầu từ dòng 13, header chiếm 2 dòng: 13 và 14
                int headerRow1 = 13;
                int headerRow2 = 14;

                // merge các cột (dọc 2 dòng)
                ws.Range(headerRow1, 1, headerRow2, 1).Merge().Value = "STT";
                ws.Range(headerRow1, 2, headerRow2, 2).Merge().Value = "Ngày hạch toán (1)";
                ws.Range(headerRow1, 3, headerRow2, 3).Merge().Value = "Loại xe NCC";
                ws.Range(headerRow1, 4, headerRow2, 4).Merge().Value = "Mã điều xe/số file";
                ws.Range(headerRow1, 5, headerRow2, 5).Merge().Value = "Số tờ khai";
                ws.Range(headerRow1, 6, headerRow2, 6).Merge().Value = "Bill";
                ws.Range(headerRow1, 7, headerRow2, 7).Merge().Value = "Nội dung (3)";
                ws.Range(headerRow1, 8, headerRow2, 8).Merge().Value = "Sô tiền (4)";
                ws.Range(headerRow1, 9, headerRow2, 9).Merge().Value = "VAT (5)";
                ws.Range(headerRow1, 10, headerRow2, 10).Merge().Value = "Tổng cộng (6)";
                ws.Range(headerRow1, 11, headerRow2, 11).Merge().Value = "Phí nâng hạ (7)";
                ws.Range(headerRow1, 12, headerRow2, 12).Merge().Value = "Thanh toán (8)";
                ws.Range(headerRow1, 13, headerRow2, 13).Merge().Value = "Ứng trước";
                ws.Range(headerRow1, 14, headerRow2, 14).Merge().Value = "Còn lại (9)";
                ws.Range(headerRow1, 15, headerRow2, 15).Merge().Value = "Biển số xe";
                ws.Range(headerRow1, 16, headerRow2, 16).Merge().Value = "Số cont";

                // format chung cho header
                var headerRange = ws.Range(headerRow1, 1, headerRow2, 16);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                // Set width cho toàn bộ cột từ A → Q
                ws.Column(1).Width = 5;   // STT
                ws.Column(2).Width = 12;  // Ngày hạch toán (1)
                ws.Column(3).Width = 15;  // Loại xe NCC
                ws.Column(4).Width = 20;  // Mã điều xe/số file
                ws.Column(5).Width = 25;  // Số tờ khai (3)
                ws.Column(6).Width = 18;  // Số bill (4)
                ws.Column(7).Width = 40;  // Nội dung (3)
                ws.Column(8).Width = 15;  // Sô tiền (4)
                ws.Column(9).Width = 12;  // VAT (5)
                ws.Column(10).Width = 18; // Tổng cộng (6)
                ws.Column(11).Width = 18; // Phí nâng hạ (7)
                ws.Column(12).Width = 12; // Thanh toán (8)
                ws.Column(13).Width = 18; // Ứng trước
                ws.Column(14).Width = 12; // Còn lại (9)
                ws.Column(15).Width = 30; // Biển số xe
                ws.Column(16).Width = 15; // Số cont 

                // Cho phép xuống dòng trong ô nếu text dài
                ws.Range(headerRow1, 1, headerRow2, 16).Style.Alignment.WrapText = true;
                // ==== DỮ LIỆU MẪU ====
                int startRow = 15;
                int currentRow = startRow;
                int row = startRow;
               
                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", groupedData);
                for (int i = 0; i < groupedData.Count; i++)
                {
                    var group = groupedData[i];
                    var first = group.Items.First();
                    check_error = JsonSerializer.Serialize(group);
                   
                    if (group.type != 2)
                    {
                        // Lấy bản ghi đầu tiên trong group (để lấy thông tin chung)
                      
                    　  //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                            // Tính tổng và count
                        ContractFiles.Models.FileInfo _fileInfo = new ContractFiles.Models.FileInfo();
                        if (first.file_info_id > 0)
                        {
                            _fileInfo = list_file.FirstOrDefault(x => x.Id == (long)first.file_info_id);
                        }
                        // Lấy danh sách bản ghi của group theo file_info_id
                        if (first.file_info_id > 0)
                        {
                            var serviceList = data
                            .Where(x => x.file_info_id == first.file_info_id)
                            .OrderBy(x=>x.type)
                            .ToList();

                            // Nếu không có bản ghi → bỏ qua
                            if (!serviceList.Any())
                                continue;

                            // DUYỆT TỪNG BẢN GHI TRONG DATA (đúng theo mẫu bạn yêu cầu)
                            foreach (var svc in serviceList)
                            {
                                // Tính giá
                                decimal price_dv = (decimal)svc.purchase_price + (decimal)svc.purchase_com;
                                decimal price = (decimal)svc.purchase_price;

                                decimal vatAmount_dv = price_dv * (decimal)svc.purchase_vat / 100m;
                                decimal vatAmount = price * (decimal)svc.purchase_vat / 100m;

                                decimal total_price_dv = new int[] { 0, 1, 4, 7, 10}.Contains((int)svc.type)
                                    ? (price_dv + vatAmount_dv)
                                    : 0;

                                decimal price_thue_dv = vatAmount_dv;

                                decimal price_ch = new int[] { 2, 3, 11 }.Contains((int)svc.type)
                                    ? (price + vatAmount)
                                    : 0;
                                decimal receiptTotal = svc?.receipt_total ?? 0m;
                                decimal conlai = (total_price_dv + price_ch ) - receiptTotal;
                                // --- GHI DÒNG CHI TIẾT ---
                                   // return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu khách hàng", first);
                                ws.Cell(row, 1).Value = i + 1; // STT
                                ws.Cell(row, 2).Value = svc.accounting_date?.ToString("dd/MM/yyyy") ?? "";
                                ws.Cell(row, 3).Value =  svc.supplier_vehicle_type ??"";
                                ws.Cell(row, 4).Value =  _fileInfo != null ? (_fileInfo.FileNumber??svc.dispatch_code) : svc.dispatch_code ?? "";
                                ws.Cell(row, 5).Value =  _fileInfo != null ? _fileInfo.Declaration ??"" : "";
                                ws.Cell(row, 6).Value =  _fileInfo != null ? _fileInfo.Bill ?? "" : "";
                                ws.Cell(row, 7).Value =  svc.name ?? "";
                                ws.Cell(row, 8).Value =  (svc.type == 0 || svc.type == 1 || svc.type == 4 || svc.type == 7|| svc.type == 10)? price_dv:0;
                                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 9).Value =  (svc.type == 0 || svc.type == 1 || svc.type == 4 || svc.type == 7|| svc.type == 10)? vatAmount_dv:0;
                                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 10).Value = total_price_dv+price_ch;
                                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 11).Value = price_ch;
                                ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 12).Value = receiptTotal;
                                ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 14).Value =  conlai;
                                ws.Cell(row, 14).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 15).Value = svc.vehicle_number ?? "";
                                ws.Cell(row, 16).Value =  _fileInfo != null ? (_fileInfo.ContainerCode?? "") : "";
                                row++;
                            }
                        }
                        else
                        {
                            // Tính giá
                                decimal price_dv = (decimal)first.purchase_price + (decimal)first.purchase_com;
                                decimal price = (decimal)first.purchase_price;

                                decimal vatAmount_dv = price_dv * (decimal)first.purchase_vat / 100m;
                                decimal vatAmount = price * (decimal)first.purchase_vat / 100m;

                                decimal total_price_dv = new int[] { 0, 1, 4, 7, 10}.Contains((int)first.type)
                                    ? (price_dv + vatAmount_dv)
                                    : 0;

                                decimal price_thue_dv = vatAmount_dv;

                                decimal price_ch = new int[] { 2, 3, 11 }.Contains((int)first.type)
                                    ? (price + vatAmount)
                                    : 0;
                                decimal receiptTotal = first?.receipt_total ?? 0m;
                                decimal conlai = (total_price_dv + price_ch ) - receiptTotal;
                                ws.Cell(row, 1).Value = i + 1; // STT
                                ws.Cell(row, 2).Value = first.accounting_date?.ToString("dd/MM/yyyy") ?? "";
                                ws.Cell(row, 3).Value =  first.supplier_vehicle_type ??"";
                                ws.Cell(row, 4).Value =  first.dispatch_code ?? "";
                                ws.Cell(row, 7).Value =  first.name ?? "";
                                ws.Cell(row, 8).Value =  (first.type == 0 || first.type == 1 || first.type == 4 || first.type == 7|| first.type == 10)? price_dv:0;
                                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 9).Value =  (first.type == 0 || first.type == 1 || first.type == 4 || first.type == 7|| first.type == 10)? vatAmount_dv:0;
                                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 10).Value = total_price_dv+price_ch;
                                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 11).Value = price_ch;
                                ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 12).Value = receiptTotal;
                                ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 14).Value = conlai;
                                ws.Cell(row, 14).Style.NumberFormat.Format = "#,##0";
                                ws.Cell(row, 15).Value = first.vehicle_number ??"";
                                row++;
                        }
                    }
                    else
                    {
                        decimal receiptTotal = first?.Amount ?? 0m;
                        ws.Cell(row, 1).Value = i + 1; // STT
                        ws.Cell(row, 2).Value =  first.accounting_date?.ToString("dd/MM/yyyy") ??""; 
                        ws.Cell(row, 7).Value =  first.Note ?? "";
                        ws.Cell(row, 13).Value =  0;
                        ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";
                    }
                                                
                }
              
                int dataStartRow = 15;
                int dataEndRow = ws.LastRowUsed().RowNumber();
                int totalRow = dataEndRow + 1;

                // ghi chữ "TỔNG CỘNG"
                ws.Range(totalRow, 1, totalRow, 7).Merge();
                ws.Cell(totalRow, 1).Value = "Tổng cộng";
                ws.Range(totalRow, 1, totalRow, 7).Style.Font.Bold = true;
                ws.Range(totalRow, 1, totalRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Tổng công nợ phải thu (12) =  (9) + (10)
                ws.Range(totalRow+1, 1, totalRow+1, 7).Merge();
                ws.Cell(totalRow+1, 1).Value = "Tổng công nợ phải trả (11) =  (6) + (7) - (8) - (9)";
                ws.Range(totalRow+1, 1, totalRow+1, 7).Style.Font.Bold = true;
                ws.Range(totalRow+1, 1, totalRow+1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(totalRow, 8).FormulaA1 = $"SUM(H{dataStartRow}:H{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 8).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 9).FormulaA1 = $"SUM(I{dataStartRow}:I{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 9).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 10).FormulaA1 = $"SUM(J{dataStartRow}:J{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 10).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 11).FormulaA1 = $"SUM(K{dataStartRow}:K{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 11).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 12).FormulaA1 = $"SUM(L{dataStartRow}:L{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 12).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 13).FormulaA1 = $"SUM(M{dataStartRow}:M{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 13).Style.NumberFormat.Format = "#,##0";
                ws.Cell(totalRow, 14).FormulaA1 = $"SUM(N{dataStartRow}:N{dataEndRow})";  // TỔNG CỘNG
                ws.Cell(totalRow, 14).Style.NumberFormat.Format = "#,##0";

                ws.Range(totalRow+1, 8, totalRow+1, 14).Merge();
                ws.Range(totalRow+1, 8, totalRow+1, 14).Style.Font.Bold = true;
                ws.Range(totalRow+1, 8, totalRow+1, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(totalRow+1, 8).FormulaA1 = $"SUM(J{dataStartRow}:K{dataEndRow}) - SUM(L{dataStartRow}:M{dataEndRow})";
                ws.Cell(totalRow+1, 8).Style.NumberFormat.Format = "#,##0";
                // Tổng công nợ phải thu (12) =  (9) + (10)
                ws.Range(totalRow+2, 1, totalRow+2, 7).Merge();
                ws.Cell(totalRow+2, 1).Value = "Dư nợ cuối kỳ (12) = (0) + (11)";
                ws.Range(totalRow+2, 1, totalRow+2, 7).Style.Font.Bold = true;
                ws.Range(totalRow+2, 1, totalRow+2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Range(totalRow+2, 8, totalRow+2, 14).Merge();
                ws.Range(totalRow+2, 8, totalRow+2, 14).Style.Font.Bold = true;
                ws.Range(totalRow+2, 8, totalRow+2, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(totalRow+2, 8).FormulaA1 = $"SUM(J{dataStartRow}:K{dataEndRow}) - SUM(L{dataStartRow}:M{dataEndRow}) + {duno}";
                ws.Cell(totalRow+2, 8).Style.NumberFormat.Format = "#,##0";

                dataEndRow = ws.LastRowUsed().RowNumber();
                totalRow = dataEndRow + 1;
               
                // range dữ liệu (A..Q)
                var dataRange = ws.Range(dataStartRow, 1, dataEndRow, 16);
                // set border all
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                // ==== EXPORT ====
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "BangKeChiTiet.xlsx"
                );
              }
              catch(Exception ex)
              {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // frame đầu tiên có thông tin lỗi
                var line = frame.GetFileLineNumber();
                var file = frame.GetFileName();
                var method = frame.GetMethod()?.Name;

                var errorMessage = $"Error at {file}:{line} in {method} - {ex.Message} - {check_error}";

                _logger.LogError(ex, errorMessage);

                return ApiResponseResult<object>(false, errorMessage, null);
              }

        }
        [HttpGet("congnochitietncc")]
        public async Task<IActionResult> GetCongNoChiTietNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitChiTietNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectNoDebitNCCAsync")]
        public async Task<IActionResult> GetObjectNoDebitNCCAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectNoDebitNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetObjectHasDebitNCCAsync")]
        public async Task<IActionResult> GetObjectHasDebitNCCAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectHasDebitNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congnodoitruncc")]
        public async Task<IActionResult> GetCongNoDoiTruNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitBuTruNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congnodoitrukh")]
        public async Task<IActionResult> GetCongNoDoiTruKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitBuTruKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("dispatch")]
        public async Task<IActionResult> GetTaskDispatch(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectFileHasDispatchAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("BaoCaoNhatKyDieuXeAsync")]
        public async Task<IActionResult> BaoCaoNhatKyDieuXeAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.BaoCaoNhatKyDieuXeAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("DispatchByDriver")]
        public async Task<IActionResult> GetObjectFileHasDispatchByDriverAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectFileHasDispatchByDriverAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       
        [HttpGet("muaban")]
        public async Task<IActionResult> GetTaskMuaBan(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitMuaBanAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("cuoctamthu")]
        public async Task<IActionResult> GetTaskCuocTamThu(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitPhiTamThuAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("phicuoc")]
        public async Task<IActionResult> GetTaskPhiCuoc(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitPhiCuocAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("muahangNCC")]
        public async Task<IActionResult> GetTaskMuaHangNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectMuaHangNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("banhangKH")]
        public async Task<IActionResult> GetTaskBanHangKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectBanHangKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("daukykh")]
        public async Task<IActionResult> GetTaskDauKyKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDauKyKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("daukyncc")]
        public async Task<IActionResult> GetTaskDauKyNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDauKyNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectTaskAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] DebitTransportationCostDto DebitDto)
        {
            //if (string.IsNullOrEmpty(DebitDto.VehicleNumber))
            //{
            //    return ApiResponseResult<object>(false, "Chưa nhập biển số xe", null);
            //}
            if (!DebitDto.CustomerDetailId.HasValue || DebitDto.CustomerDetailId <= 0)
            {
                return ApiResponseResult<object>(false, "Không được để trống khách hàng", null);
            }
            //if (!DebitDto.EmployeeDriverId.HasValue || DebitDto.EmployeeDriverId <= 0)
            //{
            //    return ApiResponseResult<object>(false, "Không được để trống lai xe", null);
            //}
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "KS"+DebitDto.AccountingDate.ToString("yyMM"),4);
                var debit = new Debit
                {
                    VehicleId = DebitDto.VehicleId,
                    VehicleNumber = DebitDto.VehicleNumber,
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    EmployeeDriverId = DebitDto.EmployeeDriverId,
                    EmployeeStaffId = DebitDto.EmployeeStaffId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.PhiVanChuyen,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Route,
                    AccountingDate = DebitDto.AccountingDate,
                    ServiceDate = DebitDto.ServiceDate != default(DateTime) ? DebitDto.ServiceDate : DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.PurchasePrice,
                    TransportationCost = JsonSerializer.Serialize(DebitDto.TransportationCost),
                    Price = DebitDto.Price,
                    Vat = DebitDto.Vat,
                    DriverFee = DebitDto.DriverFee,
                    MealFee = DebitDto.MealFee,
                    DeliveryPoint = DebitDto.DeliveryPoint,
                    TicketFee = DebitDto.TicketFee,
                    OvernightFee = DebitDto.OvernightFee,
                    PenaltyFee = DebitDto.PenaltyFee,
                    GoodsFee = DebitDto.GoodsFee,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CustomerVehicleType = DebitDto.CustomerVehicleType,
                    SupplierVehicleType = DebitDto.SupplierVehicleType,
                    PurchaseStatus = DebitDto.PurchaseStatus,
                    PurchaseVat = DebitDto.PurchaseVat,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                if (DebitDto.FileInfoId > 0)debit.FileInfoId = DebitDto.FileInfoId;
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    FileInfoId = DebitDto.FileInfoId,
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDichVu,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                await _context.SaveChangesAsync();
                var getUserDriver = await _context.Employees.FirstOrDefaultAsync(x => x.Id == DebitDto.EmployeeDriverId);
                if (getUserDriver?.UserId != null)
                {
                    await _fcmQueue.EnqueueAsync(new FcmJobDto
                    {
                        UserIds = new List<int> { getUserDriver.UserId.Value },
                        Title = "Bạn nhận được chuyến xe mới",
                        Body = debit.Name,
                        StorageId = debit.StorageId,
                        PostId = debit.Id,
                        Type = 0,
                        Screen = "chuyenxe"
                    });
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
            
        }
       
        [HttpPost]
        [Route("create/muahang")]
        public async Task<IActionResult> CreateMuaHang([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.SupplierDetailId == null || DebitDto.SupplierDetailId == 0)
                return ApiResponseResult<object>(false, "Nhà cung cấp bắt buộc", null);
            if ((DebitDto.IncomeExpenseCategoryId ?? 0) == 0)
                return ApiResponseResult<object>(false, "ly do chi bắt buộc", null);
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "MH" + DebitDto.AccountingDate.ToString("yyMM"), 4);

                var debit = new Debit
                {
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.MuaHangNCC,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Note,
                    AccountingDate = DebitDto.AccountingDate,
                    ServiceDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.MuaHangNCC.Sum(x => x.ThanhTien),
                    Status = ContractFileRepository.statusDebit,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.SupplierDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                var PrefixCode = "PCMH"+DebitDto.AccountingDate.ToString("yyMM");
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", DebitDto.StorageId, PrefixCode , 4);
                var entity_receipt = new Receipt
                {
                    AccountingDate = DebitDto.AccountingDate,
                    StorageId = DebitDto.StorageId,
                    CodeReceipt = code_receipt,
                    Note = DebitDto.Note,
                    FormOfPayment = 1,
                    PurchaseDebitId = debit.Id,
                    Object = ReceiptRepositories.DoiTuongNCC,
                    ObjectId = DebitDto.SupplierDetailId,
                    EmployeeId = DebitDto.EmployeeId,
                    TypeReceipt = ReceiptRepositories.ChiMuaHangNCC,
                    IncomeExpenseCategoryId = DebitDto.IncomeExpenseCategoryId,
                    Status = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };
                _context.Receipts.Add(entity_receipt);
                await _context.SaveChangesAsync();
                foreach (var item in DebitDto.MuaHangNCC)
                {
                     var entity_detail = new ReceiptDetail
                    {
                        ReceiptId = entity_receipt.Id,
                        StorageId = DebitDto.StorageId,
                        AccountingDate = DebitDto.AccountingDate,
                        Bill = item.Bill,
                        Allocation = item.Allocation,
                        VehicleId = item.VehicleId,
                        Amount = item.Amount,
                        Vat = item.Vat,
                        Note = item.Note,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.ReceiptDetails.Add(entity_detail);
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost]
        [Route("create/banhang")]
        public async Task<IActionResult> CreateBanHang([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "BH" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                var debit = new Debit
                {
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.BanHangKH,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Note,
                    AccountingDate = DebitDto.AccountingDate,
                    ServiceDate = DebitDto.AccountingDate,
                    Price = DebitDto.Price,
                    Vat = DebitDto.Vat,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost]
        [Route("create/daukykh")]
        public async Task<IActionResult> CreateDauKyKH([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "DKKH"+DebitDto.AccountingDate.ToString("yyMM"),4);

                var debit = new Debit
                {
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitDto.Type,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Name,
                    AccountingDate = DebitDto.AccountingDate,
                    ServiceDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
            
        }
        [HttpPost]
        [Route("create/daukyncc")]
        public async Task<IActionResult> CreateDauKyNCC([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "DKNCC"+DebitDto.AccountingDate.ToString("yyMM"),4);
                var debit = new Debit
                {
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitDto.Type,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Name,
                    AccountingDate = DebitDto.AccountingDate,
                    ServiceDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.SupplierDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
            
        }
       
        [HttpPost]
        [Route("service/create")]
        public async Task<IActionResult> ServiceCreate([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            var BillCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
            try
            {
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var now = DateTime.Now;
                if (DebitDto.CustomerDetailId > 0)
                {
                    foreach (var item in DebitDto.productChiho)
                    {
                        var debit = new Debit
                        {
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            ServiceId = item.ServiceId,
                            Bill = item.Bill,
                            LinkBill = item.LinkBill,
                            CodeBill = item.CodeBill,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "CH" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiChiHo,
                            Name = item.Name,
                            AccountingDate = DebitDto.AccountingDate,
                            ServiceDate = DebitDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.PurchasePrice,
                            Data = DebitDto.Data,
                            Note = item.Note,
                            ServiceDetail =  JsonSerializer.Serialize(new[] { item }),
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            DebitId = debit.Id,
                            StorageId = DebitDto.StorageId,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (DebitDto.productHaiquan.Count > 0)
                    {
                        var debit = new Debit
                        {
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            FileInfoId = DebitDto.FileInfoId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "HQ" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiHaiQuan,
                            Name = "Chi phí hải quan",
                            AccountingDate = DebitDto.AccountingDate,
                            ServiceDate = DebitDto.AccountingDate,
                            PurchasePrice = DebitDto.productHaiquan.Sum(x => x.PurchasePrice),
                            Data = DebitDto.Data,
                            Note = string.Join(";", DebitDto.productHaiquan.Where(x => !string.IsNullOrWhiteSpace(x?.Note)) .Select(x => x.Note) ),
                            ServiceDetail = JsonSerializer.Serialize(DebitDto.productHaiquan),
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost]
        [Route("nangha/create")]
        public async Task<IActionResult> NangHaCreate([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            var BillCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD"+DebitDto.AccountingDate.ToString("yyMM"), 4);
            try
            {
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var now = DateTime.Now;
                if (DebitDto.CustomerDetailId > 0)
                {
                    foreach (var item in DebitDto.productNangha) 
                    {
                        var debit = new Debit
                        {
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            ServiceId = item.ServiceId,
                            Bill = item.Bill,
                            LinkBill = item.LinkBill,
                            CodeBill = item.CodeBill,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "NH" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiNangHa,
                            Name = item.Name,
                            AccountingDate = DebitDto.AccountingDate,
                            ServiceDate = DebitDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.PurchasePrice,
                            Data = DebitDto.Data,
                            Note = item.Note,
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        debit.SupplierDetailId = (item.SupplierDetailId > 0) ? item.SupplierDetailId : null;
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            DebitId = debit.Id,
                            StorageId = DebitDto.StorageId,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                  
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
          [HttpPost("importDauKy")]
        public async Task<IActionResult> ImportDauKy([FromBody] ImportDauKyDto ImportDauKyDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ImportDauKyDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ImportDauKyDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    string ten_kh = item.GetProperty("ten_kh").GetString();
                    int tien_dv = item.GetProperty("tien_dv").GetInt32();
                    int tien_ch = item.GetProperty("tien_ch").GetInt32();
                    string noi_dung = item.GetProperty("noi_dung").GetString();
                    DateTime ngay = Convert.ToDateTime(item.GetProperty("ngay").GetString());
                    var _kh = await _context.Partners.Where(x => x.Abbreviation.Contains(ten_kh)).FirstOrDefaultAsync();
                    if (_kh == null) continue;
                    var _kh_detail = await _context.PartnerDetails.Where(x => x.PartnerId == _kh.Id && x.Status == 1).FirstOrDefaultAsync();
                    if (_kh_detail == null) continue;
                    int CycleName = int.Parse(ngay.ToString("MMyyyy"));
                    var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ImportDauKyDto.StorageId, "DKKH" + ngay.ToString("yyMM"), 4);

                    if (tien_dv > 0)
                    {
                        var debit = new Debit
                        {
                            CustomerDetailId = _kh_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 5,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            ServiceDate = ngay,
                            Price = tien_dv,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _kh_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (tien_ch > 0)
                    {
                        var debit = new Debit
                        {
                            CustomerDetailId = _kh_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 6,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            ServiceDate = ngay,
                            Price = tien_ch,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _kh_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
         [HttpPost("importDauKyNCC")]
        public async Task<IActionResult> ImportDauKyNCC([FromBody] ImportDauKyDto ImportDauKyDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ImportDauKyDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ImportDauKyDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    string ten_ncc = item.GetProperty("ten_ncc").GetString();
                    int tien_dv = item.GetProperty("tien_dv").GetInt32();
                    int tien_ch = item.GetProperty("tien_ch").GetInt32();
                    string noi_dung = item.GetProperty("noi_dung").GetString();
                    DateTime ngay = Convert.ToDateTime(item.GetProperty("ngay").GetString());
                    var _ncc = await _context.Partners.Where(x => x.Abbreviation.Contains(ten_ncc)).FirstOrDefaultAsync();
                    if (_ncc == null) continue;
                    var _ncc_detail = await _context.PartnerDetails.Where(x => x.PartnerId == _ncc.Id && x.Status == 2).FirstOrDefaultAsync();
                    if (_ncc_detail == null) continue;
                    int CycleName = int.Parse(ngay.ToString("MMyyyy"));
                 
                    var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ImportDauKyDto.StorageId, "DKNCC" + ngay.ToString("yyMM"), 4);

                    if (tien_dv > 0)
                    {
                        var debit = new Debit
                        {
                            SupplierDetailId = _ncc_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 10,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            ServiceDate = ngay,
                            PurchasePrice = tien_dv,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _ncc_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (tien_ch > 0)
                    {
                        var debit = new Debit
                        {
                            SupplierDetailId = _ncc_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 11,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            ServiceDate = ngay,
                            PurchasePrice = tien_ch,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _ncc_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost]
        [Route("updateDebit")]
        public async Task<IActionResult> UpdateDebit([FromBody] DebitTransportationCostDto DebitDto)
        {
            if (!DebitDto.CustomerDetailId.HasValue || DebitDto.CustomerDetailId <= 0)
            {
                return ApiResponseResult<object>(false, "Không được để trống khách hàng", null);
            }
            var debit = await _context.Debits.FirstOrDefaultAsync(x=>x.Id == DebitDto.Id);
            if(debit == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                debit.VehicleId = DebitDto.VehicleId;
                debit.VehicleNumber = DebitDto.VehicleNumber;
                debit.CustomerDetailId = DebitDto.CustomerDetailId;
                debit.SupplierDetailId = DebitDto.SupplierDetailId;
                debit.EmployeeDriverId = DebitDto.EmployeeDriverId;
                debit.EmployeeStaffId = DebitDto.EmployeeStaffId;
                debit.StorageId = DebitDto.StorageId;
                debit.Type = DebitRepositories.PhiVanChuyen;
                debit.TransportationCost = JsonSerializer.Serialize(DebitDto.TransportationCost);
                debit.Name = DebitDto.Route;
                //debit.AccountingDate = DebitDto.AccountingDate;
                debit.ServiceDate = DebitDto.ServiceDate != default(DateTime) ? DebitDto.ServiceDate : DebitDto.AccountingDate;
                debit.PurchasePrice = DebitDto.PurchasePrice;
                debit.Price = DebitDto.Price;
                debit.Vat = DebitDto.Vat;
                debit.DriverFee = DebitDto.DriverFee;
                debit.MealFee = DebitDto.MealFee;
                debit.TicketFee = DebitDto.TicketFee;
                debit.OvernightFee = DebitDto.OvernightFee;
                debit.PenaltyFee = DebitDto.PenaltyFee;
                debit.GoodsFee = DebitDto.GoodsFee;
                debit.DeliveryPoint = DebitDto.DeliveryPoint;
                debit.Data = DebitDto.Data;
                debit.Note = DebitDto.Note;
                debit.CustomerVehicleType = DebitDto.CustomerVehicleType;
                debit.SupplierVehicleType = DebitDto.SupplierVehicleType;
                debit.PurchaseStatus = DebitDto.PurchaseStatus;
                debit.PurchaseVat = DebitDto.PurchaseVat;
                debit.UpdatedAt = now;
                debit.UpdatedBy = userId;
                if (DebitDto.FileInfoId > 0)debit.FileInfoId = DebitDto.FileInfoId;
                _context.Debits.Update(debit);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Sửa thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
            
        }
        [HttpPost]
        [Route("update/daukykh")]
        public async Task<IActionResult> UpdateDauKy([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
               
                entity.Name = DebitDto.Name;
                entity.Type = DebitDto.Type;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.ServiceDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.Price;
                entity.Price = DebitDto.Price;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/daukyncc")]
        public async Task<IActionResult> UpdateDauKyNCC([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                entity.Name = DebitDto.Name;
                entity.Type = DebitDto.Type;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.ServiceDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.Price;
                entity.Price = DebitDto.Price;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/muahang")]
        public async Task<IActionResult> UpdateMuaHang([FromBody] DebitDto DebitDto)
        {
            if ((DebitDto.IncomeExpenseCategoryId ?? 0) == 0)
                return ApiResponseResult<object>(false, "ly do chi bắt buộc", null);
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                entity.Name = DebitDto.Note;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.ServiceDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.MuaHangNCC.Sum(x => x.ThanhTien);
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                bool checkDelete = await deleteReceiptByDebitId(DebitDto.Id);
                if (!checkDelete)
                {
                    await tran.RollbackAsync();
                    return ApiResponseResult<object>(false, "Lỗi khi xóa phiếu thu liên quan", null);
                }
                var PrefixCode = "PCMH"+DebitDto.AccountingDate.ToString("yyMM");
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", DebitDto.StorageId, PrefixCode , 4);
                var entity_receipt = new Receipt
                {
                    AccountingDate = DebitDto.AccountingDate,
                    StorageId = DebitDto.StorageId,
                    CodeReceipt = code_receipt,
                    Note = DebitDto.Note,
                    FormOfPayment = 1,
                    PurchaseDebitId = DebitDto.Id,
                    Object = ReceiptRepositories.DoiTuongNCC,
                    ObjectId = DebitDto.SupplierDetailId,
                    EmployeeId = DebitDto.EmployeeId,
                    TypeReceipt = ReceiptRepositories.ChiMuaHangNCC,
                    IncomeExpenseCategoryId = DebitDto.IncomeExpenseCategoryId,
                    Status = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };
                _context.Receipts.Add(entity_receipt);
                await _context.SaveChangesAsync();
                foreach (var item in DebitDto.MuaHangNCC)
                {
                     var entity_detail = new ReceiptDetail
                    {
                        ReceiptId = entity_receipt.Id,
                        StorageId = DebitDto.StorageId,
                        AccountingDate = DebitDto.AccountingDate,
                        Bill = item.Bill,
                        Allocation = item.Allocation,
                        VehicleId = item.VehicleId,
                        Amount = item.Amount,
                        Vat = item.Vat,
                        Note = item.Note,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.ReceiptDetails.Add(entity_detail);
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/banhang")]
        public async Task<IActionResult> UpdateBanHang([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                entity.Name = DebitDto.Note;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.ServiceDate = DebitDto.AccountingDate;
                entity.Price = DebitDto.Price;
                entity.Vat = DebitDto.Vat;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost("confirmChiPhiHaiQuan")]
        public async Task<IActionResult> ConfirmChiPhiHaiQuan([FromBody] DebitDto DebitDto)
        {
             using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);
                if (debit == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu file giá", null);
                var confirm_file = await _context.ConfirmFiles
                .Where(x =>
                    x.FileInfoId == DebitDto.FileInfoId &&
                    x.PartnerDetailId == DebitDto.CustomerDetailId &&
                    x.DebitId == debit.Id
                ).FirstOrDefaultAsync();
                if (confirm_file == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu xác nhận", null);
                debit.PurchasePrice = DebitDto.productHaiquan.Sum(x => x.PurchasePrice);
                debit.ServiceDetail = JsonSerializer.Serialize(DebitDto.productHaiquan);
                debit.UpdatedBy = userId;
                debit.UpdatedAt = now;
                if (confirm_file.Status == 0 || confirm_file.Status == 1)
                {
                    confirm_file.Status = 1;
                    confirm_file.StatusConfirm = 1;
                    confirm_file.UpdatedBy = userId;
                    confirm_file.UpdatedAt = now;
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
      
        [HttpPost("updateDebitNCC")]
        public async Task<IActionResult> UpdateDebitNCC([FromBody] ConfirmDebitNoFileDto ConfirmDebitNoFileDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ConfirmDebitNoFileDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ConfirmDebitNoFileDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết cong no", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    int debit_id = item.GetProperty("id").GetInt32();
                    int vat = item.GetProperty("purchase_vat").GetInt32();
                    DateTime service_date = Convert.ToDateTime(item.GetProperty("service_date").GetString());
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == debit_id);
                    if (debit == null) continue;
                   
                    debit.PurchaseAccountingDate = service_date;
                    debit.PurchaseVat = vat;
                    debit.PurchaseStatus = 1;
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("confirmDebitNoFileDispatchKH")]
        public async Task<IActionResult> ConfirmDebitNoFileDispatchKH([FromBody] ConfirmDebitNoFileDto ConfirmDebitNoFileDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ConfirmDebitNoFileDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ConfirmDebitNoFileDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết cong no", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    int debit_id = item.GetProperty("id").GetInt32();
                    int vat = item.GetProperty("vat").GetInt32();
                    int purchase_com = item.GetProperty("purchase_com").GetInt32();
                    int price_com = item.GetProperty("price_com").GetInt32();
                    DateTime service_date = Convert.ToDateTime(item.GetProperty("service_date").GetString());
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == debit_id);
                    if (debit == null) continue;
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.DebitId == debit.Id); // duyệt file giá

                    if (confirm_file.Status < 2)
                    {
                        debit.AccountingDate = service_date;
                        debit.Vat = vat;
                        debit.PurchaseCom = purchase_com;
                        debit.PriceCom = price_com;
                        debit.Status = ContractFileRepository.statusDebit;
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        confirm_file.Status = ContractFileRepository.statusDebit;
                        confirm_file.StatusConfirm = 0;
                        confirm_file.UpdatedBy = userId;
                        confirm_file.UpdatedAt = now;
                    }
                    // cập nhat hoa don debit
                    if (ConfirmDebitNoFileDto.Type == 1)
                    {
                        debit.CusBillDate = ConfirmDebitNoFileDto.AccountingDate;
                        debit.CusBill = ConfirmDebitNoFileDto.Bill;
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("confirmFileGiaByIdFileGia")]
        public async Task<IActionResult> ConfirmFileGiaByIdFileGia([FromBody] ConfirmFileInfoDto ConfirmFileInfoDto)
        {
            if (ConfirmFileInfoDto.Ids == null || ConfirmFileInfoDto.Ids.Length == 0)
                return ApiResponseResult<object>(false, "Danh sách Id không hợp lệ", null);

            await using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.Now;

                var debits = await _context.Debits
                                           .Where(x => x.FileInfoId.HasValue && ConfirmFileInfoDto.Ids.Contains(x.FileInfoId.Value))
                                           .ToListAsync();

                if (!debits.Any())
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                // Ví dụ: cập nhật trạng thái xác nhận
                foreach (var item in debits)
                {
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == item.FileInfoId && x.PartnerDetailId == item.CustomerDetailId && x.DebitId == item.Id); // duyệt file giá
                    if (confirm_file == null)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu xác nhận chi phí. Hãy duyệt chi phí hải quan" +item.Id, null);
                    }
                    if (confirm_file.Status == 1 || confirm_file.Status == 2)
                    {
                        item.Status = ContractFileRepository.statusDebit; 
                        item.UpdatedBy = userId;
                        item.UpdatedAt = now;
                        confirm_file.Status = ContractFileRepository.statusDebit;
                        confirm_file.StatusConfirm = ConfirmFileInfoDto.StatusConfirm;
                        confirm_file.UpdatedBy = userId;
                        confirm_file.UpdatedAt = now;
                    }
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.Message, null);
            }
        }
        [HttpPost("confirmFileGia")]
        public async Task<IActionResult> ConfirmFileGia([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    //if (item.Price == 0)
                    //{
                    //    await tran.RollbackAsync();
                    //    return ApiResponseResult<object>(false, "Chưa nhập giá bán. Hãy kiểm tra lại", null);
                    //}
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == item.FileInfoId && x.PartnerDetailId == item.CustomerDetailId && x.DebitId == debit.Id); // duyệt file giá
                    if (confirm_file == null)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu xác nhận chi phí. Hãy duyệt chi phí hải quan" +debit.Id, null);
                    }
                    if (confirm_file.Status == 1 || confirm_file.Status == 2)
                    {
                        debit.Price = item.Price; 
                        debit.Vat = item.Vat; 
                        debit.Status = ContractFileRepository.statusDebit; 
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        confirm_file.Status = ContractFileRepository.statusDebit;
                        confirm_file.StatusConfirm = ConfirmFileDto.StatusConfirm;
                        confirm_file.UpdatedBy = userId;
                        confirm_file.UpdatedAt = now;
                    }
                }
                int CycleName = int.Parse(ConfirmFileDto.AccountingDate.ToString("MMyyyy"));

                foreach (var item in ConfirmFileDto.Chiphikhac)
                {
                    var debit = new Debit
                    {
                        CustomerDetailId = ConfirmFileDto.PartnerDetailId,
                        SupplierDetailId = item.SupplierDetailId,
                        FileInfoId = ConfirmFileDto.FileInfoId,
                        DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ConfirmFileDto.StorageId, "PK" + ConfirmFileDto.AccountingDate.ToString("yyMM"), 4),
                        StorageId = ConfirmFileDto.StorageId,
                        Type = DebitRepositories.PhiKhac,
                        Name = item.Name,
                        AccountingDate = ConfirmFileDto.AccountingDate,
                        ServiceDate = ConfirmFileDto.AccountingDate,
                        PurchasePrice = item.PurchasePrice,
                        Price = item.Price,
                        Vat = item.Vat,
                        Status = ContractFileRepository.statusDebit,
                        Data =  JsonSerializer.Serialize(new{fileNumber=ConfirmFileDto.FileNumber}),
                        Note = item.Note,
                        ServiceDetail = JsonSerializer.Serialize(new []{item}),
                        PurchaseStatus = 0,
                        PurchaseVat = 0,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Debits.Add(debit);
                    await _context.SaveChangesAsync();
                    var entity = new ConfirmFile
                    {
                        FileInfoId = ConfirmFileDto.FileInfoId,
                        StorageId = ConfirmFileDto.StorageId,
                        DebitId = debit.Id,
                        PartnerDetailId = ConfirmFileDto.PartnerDetailId,
                        Status = ContractFileRepository.statusDebit,
                        StatusConfirm = 1,
                        CreatedBy = userId,
                        CreatedAt = now,
                    };
                    _context.ConfirmFiles.Add(entity);
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateVATFileGia")]
        public async Task<IActionResult> UpdateVATFileGia([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    
                    debit.AccountingDate = ConfirmFileDto.AccountingDate;
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;
                    debit.Vat = item.Vat;
                }
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("UpdateBill")]
        public async Task<IActionResult> UpdateBill([FromBody] DebitDto DebitDto)
        {
            if (DebitDto == null || DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var entity = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);

                if (entity == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                var now = DateTime.Now;
                var count_bill = await _context.Debits.Where(x => x.BillId == entity.BillId).CountAsync();
                if (count_bill == 1)
                {
                   var _bill = await _context.Bills.FirstOrDefaultAsync(x=>x.Id == entity.BillId);
                    _bill.DeletedAt = now;
                    _bill.DeletedBy = userId;
                     _context.Bills.Update(_bill);
                }
                entity.BillId = null;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateVATDebitNoFile")]
        public async Task<IActionResult> UpdateVATDebitNoFile([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);
                if (debit == null)  return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu debit", null);
                debit.UpdatedBy = userId;
                debit.UpdatedAt = now;
                debit.Vat = DebitDto.Vat;
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateBillDebitNoFile")]
        public async Task<IActionResult> UpdateBillDebitNoFile([FromBody] BillDebitDto BillDebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debits = await _context.Debits.Where(x => BillDebitDto.Ids.Contains((int)x.Id)).ToListAsync();
                foreach (var debit in debits)
                {
                    if (string.IsNullOrEmpty(BillDebitDto.CusBill))
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.CusBillDate = null;
                        debit.CusBill = null;
                    }
                    else
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.CusBillDate = BillDebitDto.CusBillDate;
                        debit.CusBill = BillDebitDto.CusBill;
                    }
                   
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
      
        [HttpPost("updateBillDebitNCC")]
        public async Task<IActionResult> UpdateBillDebitNCC([FromBody] BillDebitDto BillDebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debits = await _context.Debits.Where(x => BillDebitDto.Ids.Contains((int)x.Id)).ToListAsync();
                foreach (var debit in debits)
                {
                    if (string.IsNullOrEmpty(BillDebitDto.SupBill))
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.SupBill = null;
                        debit.SupBillDate = null;
                    }
                    else
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.SupBillDate = BillDebitDto.SupBillDate;
                        debit.SupBill = BillDebitDto.SupBill;
                    }

                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateBillCustomerFileGia")]
        public async Task<IActionResult> UpdateBillCustomerFileGia([FromBody] BillDebitDto BillDebitDto)
        {
          
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debits = await _context.Debits.Where(x => BillDebitDto.Ids.Contains((int)x.FileInfoId)).ToListAsync();
                foreach (var debit in debits)
                {
                    if (string.IsNullOrEmpty(BillDebitDto.CusBill))
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.CusBillDate = null;
                        debit.CusBill = null;
                    }
                    else
                    {
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        debit.CusBillDate = BillDebitDto.CusBillDate;
                        debit.CusBill = BillDebitDto.CusBill;
                    }
                   
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateFileGia")]
        public async Task<IActionResult> UpdateFileGia([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;

                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    // Chỉ update Price nếu Type == 0
                    if (item.Type == 0 && item.Price == 0)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Chưa nhập giá bán cho phí hải quan.", null);
                    }
                    if (item.Type == 0)
                    {
                        debit.Price = item.Price;
                    }
                    debit.AccountingDate = ConfirmFileDto.AccountingDate;
                    debit.Vat = item.Vat;
                    debit.Status =  ContractFileRepository.statusFileGia; 
                    debit.Bill = item.Bill;
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == ConfirmFileDto.FileInfoId && x.PartnerDetailId == ConfirmFileDto.PartnerDetailId && x.DebitId == debit.Id); // tạo phần duyệt file giá
                    if (confirm_file == null)
                    {
                        var entity = new ConfirmFile
                        {
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            StorageId = ConfirmFileDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = ConfirmFileDto.PartnerDetailId,
                            Status = ContractFileRepository.statusFileGia,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                    }
                    else
                    {
                        if (confirm_file.Status == 0 || confirm_file.Status == 1)
                        {
                            confirm_file.FileInfoId = ConfirmFileDto.FileInfoId;
                            confirm_file.StorageId = ConfirmFileDto.StorageId;
                            confirm_file.PartnerDetailId = ConfirmFileDto.PartnerDetailId;
                            confirm_file.Status = ContractFileRepository.statusFileGia;
                            confirm_file.StatusConfirm = 0;
                            confirm_file.UpdatedAt = null;
                            confirm_file.UpdatedBy = null;
                            _context.ConfirmFiles.Update(confirm_file);
                        }
                      
                    }
                    if (item.Type == 2) // duyệt luôn phần chi hộ
                    {
                         confirm_file.StatusConfirm = 1;
                        _context.ConfirmFiles.Update(confirm_file);
                    }

                }
                int CycleName = int.Parse(ConfirmFileDto.AccountingDate.ToString("MMyyyy"));
                foreach (var item in ConfirmFileDto.Chiphikhac)
                {
                     var debit = new Debit
                        {
                            CustomerDetailId = ConfirmFileDto.PartnerDetailId,
                            SupplierDetailId = item.SupplierDetailId,
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ConfirmFileDto.StorageId, "PK" + ConfirmFileDto.AccountingDate.ToString("yyMM"), 4),
                            StorageId = ConfirmFileDto.StorageId,
                            Type = DebitRepositories.PhiKhac,
                            Name = item.Name,
                            AccountingDate = ConfirmFileDto.AccountingDate,
                            ServiceDate = ConfirmFileDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.Price,
                            Vat = item.Vat,
                            Status = ContractFileRepository.statusFileGia,
                            Data =  JsonSerializer.Serialize(new{fileNumber=ConfirmFileDto.FileNumber}),
                            Note = item.Note,
                            ServiceDetail = JsonSerializer.Serialize(new []{item}),
                            PurchaseStatus = 0,
                            PurchaseVat = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            StorageId = ConfirmFileDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = ConfirmFileDto.PartnerDetailId,
                            Status = ContractFileRepository.statusFileGia,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                }
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateFileGiaNCC")]
        public async Task<IActionResult> updateFileGiaNCC([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;

                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    debit.PurchaseAccountingDate = ConfirmFileDto.AccountingDate;
                    debit.PurchaseVat = item.Vat;
                    debit.PurchaseStatus = 1; 
                    debit.PurchaseNote = ConfirmFileDto.Note; 
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;
                }
                int CycleName = int.Parse(ConfirmFileDto.AccountingDate.ToString("MMyyyy"));
                foreach (var item in ConfirmFileDto.Chiphikhac)
                {
                     var debit = new Debit
                        {
                            SupplierDetailId = ConfirmFileDto.PartnerDetailId,
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ConfirmFileDto.StorageId, "PKNCC" + ConfirmFileDto.AccountingDate.ToString("yyMM"), 4),
                            StorageId = ConfirmFileDto.StorageId,
                            Type = DebitRepositories.PhiKhacNCC,
                            Name = item.Name,
                            PurchaseAccountingDate = ConfirmFileDto.AccountingDate,
                            ServiceDate = ConfirmFileDto.AccountingDate, 
                            PurchasePrice = item.PurchasePrice,
                            PurchaseVat = item.Vat,
                            Status = ContractFileRepository.statusDebit,
                            Data =  JsonSerializer.Serialize(new{fileNumber=ConfirmFileDto.FileNumber}),
                            PurchaseNote = item.Note,
                            ServiceDetail = JsonSerializer.Serialize(new []{item}),
                            PurchaseStatus = 1,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                }
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateDebitToStatusDichVu")]
        public async Task<IActionResult> UpdateDebitToStatusDichVu([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.DebitId == entity.Id);
            if (confirm_file == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu debit confirm", null);
            }
           
            var now = DateTime.Now;
            entity.Vat = 0;
            entity.PurchaseCom = 0;
            entity.PriceCom = 0;
            entity.Status = ContractFileRepository.statusDichVu;
            entity.UpdatedBy = userId;
            entity.UpdatedAt = now;
            confirm_file.Status = ContractFileRepository.statusDichVu;
            confirm_file.StatusConfirm = 0;
            confirm_file.UpdatedBy = userId;
            confirm_file.UpdatedAt = now;
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
       [HttpPost("UpdateDriverStatus")]
        public async Task<IActionResult> UpdateDriverStatus([FromBody] DriverStatusDto dto)
        {
            var now = DateTime.Now;

            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không tồn tại", null);

            var entity = await _context.Debits.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

            // Không update nếu trạng thái không đổi
            if (entity.DriverStatus == dto.DriverStatus)
                return ApiResponseResult<object>(true, "Trạng thái không thay đổi", null);

            // Lấy record confirm
            var confirm = await _context.DriverConfirmStatuses
                .FirstOrDefaultAsync(x => x.DebitId == entity.Id);

            // Nếu đã có confirm => chỉ cho đổi trong ngày
            if (confirm != null)
            {
                // So sánh theo UpdatedAt (hợp lý hơn CreatedAt)
                var lastUpdateDate = (confirm.UpdatedAt ?? confirm.CreatedAt)?.Date;

                if (lastUpdateDate.HasValue && lastUpdateDate.Value != now.Date)
                    return ApiResponseResult<object>(false, "Đã hết hạn thay đổi trạng thái", null);

                confirm.UpdatedAt = now;
                confirm.UpdatedBy = userId; // nếu bạn muốn lưu người update thì nên dùng UpdatedBy
            }
            else
            {
                // Chưa có confirm => tạo mới
                var newConfirm = new DriverConfirmStatus
                {
                    DebitId = entity.Id,
                    StorageId = entity.StorageId,
                    CreatedAt = now,
                    CreatedBy = userId,
                    UpdatedAt = now
                };

                _context.DriverConfirmStatuses.Add(newConfirm);
            }

            // Update status
            entity.DriverStatus = dto.DriverStatus;

            await _context.SaveChangesAsync();
            string DriverStatus = Helper.DriverStatus(dto.DriverStatus);
            var getUserDriver = await _context.Employees.FirstOrDefaultAsync(x => x.UserId == userId);
            await _fcmQueue.EnqueueAsync(new FcmJobDto
            {
                UserIds = new List<int> { entity.CreatedBy.Value },
                Title = "Chuyến xe "+ entity.Name,
                Body = $"{getUserDriver?.FirstName +" "+getUserDriver?.LastName ?? "Tài xế"} đã được cập nhật thành: {DriverStatus}",
                StorageId = entity.StorageId,
                PostId = entity.Id,
                Type = 0,
            });
            return ApiResponseResult<object>(true, "Cập nhật thành công", null);
        }

        [HttpPost("updateServiceStatus")]
        public async Task<IActionResult> UpdateServiceStatus([FromBody] ServiceStatusDto ServiceStatusDto)
        {
            if (ServiceStatusDto.Ids == null || ServiceStatusDto.Ids.Length == 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Where(x => ServiceStatusDto.Ids.Contains(x.Id)).ToList();
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var now = DateTime.Now;
            foreach (var item in entity)
            {
                item.ServiceStatus = ServiceStatusDto.ServiceStatus;
                item.UpdatedBy = userId;
                item.UpdatedAt = now;
            }
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Cập nhật thành công", null);
        }
        [HttpPost("deleteDebitNCC")]
        public async Task<IActionResult> DeleteDebitNCC([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);
                if (debit == null) return ApiResponseResult<object>(false, "Khong tim thay du lieu", null);
                debit.UpdatedBy = userId;
                debit.UpdatedAt = now;
                debit.SupBill = null;
                debit.SupBillDate = null;
                debit.PurchaseStatus = 0;
                debit.PurchaseNote = null;
                debit.PurchaseVat = 0;
                debit.PurchaseAccountingDate = null;
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var now  = DateTime.Now;
             // Kiểm tra receipt liên quan
            var receiptCodes = await _context.ReceiptDetails
                .Where(d => d.DebitId == DebitDto.Id)
                .Join(
                    _context.Receipts,
                    rd => rd.ReceiptId,
                    r => r.Id,
                    (rd, r) => r.CodeReceipt
                )
                .Distinct()
                .ToListAsync();

            if (receiptCodes.Any())
            {
                return ApiResponseResult<object>(false, "Không thể xoá vì chi phí đã được thu tiền: "+ string.Join(", ", receiptCodes), null);
            }
            await deleteReceiptByDebitId(DebitDto.Id);
            entity.DeletedBy = userId;
            entity.DeletedAt = now;
            var getUserDriver = await _context.Employees.FirstOrDefaultAsync(x => x.Id == entity.EmployeeDriverId);
            if (getUserDriver?.UserId != null)
            {
                await _fcmQueue.EnqueueAsync(new FcmJobDto
                {
                    UserIds = new List<int> { getUserDriver.UserId.Value },
                    Title = "Chuyến xe của bạn đã bị hủy",
                    Body = entity.Name,
                    StorageId = entity.StorageId,
                    PostId = entity.Id,
                    Type = 0,
                    Screen = "chuyenxe"
                });
            }
            await _repoDebit.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("delete/multiDebit")]
        public async Task<IActionResult> DeleteMultiDebit([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }

            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();

            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
             // Kiểm tra receipt liên quan
            var receiptCodes = await _context.ReceiptDetails
                .Where(d => d.DebitId.HasValue && DebitDeleteMultiDto.Ids.Contains(d.DebitId.Value))
                .Join(
                    _context.Receipts,
                    rd => rd.ReceiptId,
                    r => r.Id,
                    (rd, r) => r.CodeReceipt
                )
                .Distinct()
                .ToListAsync();

            if (receiptCodes.Any())
            {
                return ApiResponseResult<object>(false, "Không thể xoá vì chi phí đã được thu tiền: "+ string.Join(", ", receiptCodes), null);
            }
            // Cập nhật thông tin xóa mềm
            var now = DateTime.Now;
            foreach (var item in entities)
            {
                item.DeletedBy = userId;
                item.DeletedAt = now;
            }
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("delete/multi")]
        public async Task<IActionResult> DeleteMulti([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }
            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();
            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
            // Kiểm tra receipt liên quan
            var receiptCodes = await _context.ReceiptDetails
                .Where(d => d.DebitId.HasValue && DebitDeleteMultiDto.Ids.Contains(d.DebitId.Value))
                .Join(
                    _context.Receipts,
                    rd => rd.ReceiptId,
                    r => r.Id,
                    (rd, r) => r.CodeReceipt
                )
                .Distinct()
                .ToListAsync();

            if (receiptCodes.Any())
            {
                return ApiResponseResult<object>(false, "Không thể xoá vì chi phí đã được thu tiền: "+ string.Join(", ", receiptCodes), null);
            }
            var entitie_confirms = await _context.ConfirmFiles
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.DebitId))
                .ToListAsync();
            // Cập nhật thông tin xóa mềm
            var now = DateTime.Now;
            foreach (var item in entities)
            {
                // if (item.Type == 0)
                // {
                //     item.Price = 0;
                // }
                if (item.Type == 4)
                {
                    item.DeletedAt = now;
                    item.DeletedBy = userId;
                }
                item.Status = 0;
                // item.Vat = 0;
                item.UpdatedAt = now;
                item.UpdatedBy = userId;
            }
            foreach (var item in entitie_confirms)
            {
                item.Status = 0;
                item.StatusConfirm = 0;
                item.UpdatedAt = now;
                item.UpdatedBy = userId;
            } 
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("showWithIds")]
        public async Task<IActionResult> ShowWithIds([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }

            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();

            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
            return ApiResponseResult<object>(true, "lấy dữ liệu thành công", entities);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("ShowWithPurchaseNCCAsync")]
        public async Task<IActionResult> ShowWithPurchaseNCCAsync([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowWithPurchaseNCCAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("ShowWithFileInfoAsync")]
        public async Task<IActionResult> ShowWithFileInfoAsync([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowWithFileInfoAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("show/byFileId")]
        public async Task<IActionResult> ShowByFileId([FromQuery] int FileId)
        {
            if (FileId <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowByFileIdAsync(FileId);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        public async Task<bool> deleteReceiptByDebitId(int debitId)
        { 
            try
            {
                var now = DateTime.Now;
                var receipt_purchase = await _context.Receipts.FirstOrDefaultAsync(d => d.PurchaseDebitId == debitId);
                if (receipt_purchase != null)
                {
                    var receipt_purchase_detail = await _context.ReceiptDetails.Where(d => d.ReceiptId == receipt_purchase.Id).ToListAsync();
                    foreach (var item in receipt_purchase_detail)
                    {
                    item.DeletedAt = now;
                    item.DeletedBy = userId;
                    _context.ReceiptDetails.Update(item);
                    }
                    receipt_purchase.DeletedAt = now;
                    receipt_purchase.DeletedBy = userId;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
