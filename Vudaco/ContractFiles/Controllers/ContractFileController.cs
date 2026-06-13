
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vudaco.ContractFiles.Dtos;
using Vudaco.ContractFiles.Models;
using Vudaco.ContractFiles.Repositories;
using Vudaco.Controllers;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Repositories;
using Vudaco.Partners.Repositories;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.Connects;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.ContractFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractFileController : BaseApiController
    {
        private readonly IContractFileRepository _repoContractFile;
        private readonly IDebitRepositories _repoDebit;
        private readonly IPartnerDetailRepository _repoPartnerDetail;
        private readonly IContractFileDetailRepository _repoContractFileDetail;
        private readonly ILogger<ContractFileController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        public int userId => (int)HttpContext.Items["UserId"];
        public ContractFileController(ILogger<ContractFileController> logger, IPartnerDetailRepository repoPartnerDetail,IDebitRepositories repoDebit, IConfiguration configuration, IContractFileDetailRepository repoContractFileDetail, IContractFileRepository repoContractFile, VudacoDBContext context)
        {
            _logger = logger;
            _repoContractFile = repoContractFile;
            _repoContractFileDetail = repoContractFileDetail;
            _context = context;
            _configuration = configuration;
            _repoDebit = repoDebit;
            _repoPartnerDetail = repoPartnerDetail;
        }
         [HttpGet("noDebitHasFileNCC")]
        public async Task<IActionResult> GetNoDebitHasFileNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectNoDebitHasFileNCCAsync(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("hasDebitHasFileNCC")]
        public async Task<IActionResult> GetHasDebitHasFileNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectHasDebitHasFileNCCAsync(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
         [HttpGet("getFileHasDebitNangHa")]
        public async Task<IActionResult> GetFileHasDebitNangHa(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectHasNangHaAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("getFileNotDebitNangHa")]
        public async Task<IActionResult> GetFileNotDebitNangHa(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectNotNangHaAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("getFileHasDebitService")]
        public async Task<IActionResult> GetFileHasDebitService(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectHasDebitServiceAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("getFileNotService")]
        public async Task<IActionResult> GetFileNotCreateChiPhi(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectNotServiceAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("getFileNotDispatch")]
        public async Task<IActionResult> GetFileNotCreateDispatch(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectFileNotDispatchAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("getFileNotFileGia")]
        public async Task<IActionResult> GetFileNotFileGia(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectNotFileGia(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
         [HttpGet("getFileHasFileGia")]
        public async Task<IActionResult> GetFileHasFileGia(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectHasFileGia(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectTaskAsync(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("excel/xuathoadon")]
        public async Task<IActionResult> ExportXuatHoaDon(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            try
                {
                    var result = await _repoDebit.GetObjectXuatHoaDonKHAsync(
                        DebitDto, page, pageSize, cancellationToken);
                    if (result?.Data == null || !result.Data.Any())
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                    var data = result.Data.Select(x => (dynamic)x).ToList();
                // ================= GROUP DATA =================

                var groupedData = data
                .Where(x => x.file_info_id != null)
                .GroupBy(x => (long?)x.file_info_id)
                .Select(g => new
                {
                    FileId = g.Key,
                    Items = g
                        .OrderByDescending(x => x.type == 1) // ưu tiên type = 1
                        .ToList(),
                    MinServiceDate = g.Min(x => (DateTime)x.service_date)
                })
                .ToList();

                // 🔹 Dữ liệu KHÔNG có file
                var groupedDataNoFile = data
                    .Where(x => x.file_info_id == null)
                    .Select(x => new
                    {
                        FileId = (long?)null,
                        Items = new List<dynamic> { x },
                        MinServiceDate = (DateTime)x.accounting_date
                    })
                    .ToList();
                //return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", groupedDataNoFile);
                // 🔹 Merge (OK vì cùng anonymous shape)
                groupedData.AddRange(groupedDataNoFile);

                // 🔹 Sort cuối
                groupedData = groupedData
                    .OrderBy(g => g.MinServiceDate)
                    .ToList();
                var fileIds = groupedData
                        .Where(g => g.FileId.HasValue)
                        .Select(g => g.FileId.Value)
                        .ToList();
                    var listFile = await _context.FileInfos
                        .Where(x => fileIds.Contains(x.Id))
                        .ToListAsync(cancellationToken);
                    // ================= CREATE EXCEL =================
                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("BẢNG KÊ CHI TIẾT");
                    ws.Cell("A1").Value = "File mẫu danh sách hóa đơn để nhập vào phần mềm";
                    ws.Cell("A2").Value = "Hướng dẫn:";
                    ws.Cell("A3").Value = "- Điền dữ liệu hóa đơn cần lập trên phần mềm vào các cột tương ứng trên file này";
                    ws.Cell("A4").Value = "- Các cột có dấu (*) là những cột bắt buộc";
                    ws.Cell("A5").Value = "- Nếu hóa đơn chiết khấu theo tổng tiền hàng thì điền thông tin về tỷ lệ CK và tiền CK ở cột L và M. Nếu chiết khấu theo từng mặt hàng thì điền thông tin ở cột T và U";
                    ws.Cell("A6").Value = "- Loại tiền tệ lấy theo cột 'Mã tiền tệ' trong chức năng 'Hệ thống => Quản lý tiền tệ'";
                    ws.Cell("A7").Value = "- Mã khách hàng (cột D) chỉ hợp lệ nếu đã tồn tại trong chức năng 'Danh mục => Khách hàng'";
                    ws.Cell("A8").Value = "- Mã hàng (cột Q) chỉ hợp lệ nếu đã tồn tại trong chức năng 'Danh mục => Hàng hóa, dịch vụ'";
                    ws.Cell("A9").Value = "- Các dòng dữ liệu phía dưới chỉ là ví dụ minh họa";
                    // ================= HEADER =================
                    int headerRow = 11;
                    string[] headers =
                    {
                        "Số thứ tự hóa đơn (*)","Ngày hóa đơn","Tên đơn vị mua hàng","Mã khách hàng","Địa chỉ","Mã số thuế",
                        "Người mua hàng","Email","SĐT","CCCD","Ghi chú",
                        "Hình thức thanh toán","Loại tiền","Tỷ giá",
                        "Tỷ lệ CK(%)",
                        "Tiền CK","Thuế suất GTGT (%)","Tiền thuế GTGT",
                        "Tên hàng hóa/dịch vụ (*)","Mã hàng","ĐVT",
                        "Số lượng","Đơn giá","Tỷ lệ CK (%)",
                        "Tiền CK","Thành tiền(*)","Loại",
                        "Loại hàng hoá đặc trưng","Số khung","Số máy",
                        "BKS phương tiện vận chuyển","Người gửi hàng","Địa chỉ người gửi",
                        "MST người gửi","Số định danh người gửi"
                    };

                    for (int i = 0; i < headers.Length; i++)
                        ws.Cell(headerRow, i + 1).Value = headers[i];

                    var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // ================= COLUMN WIDTH =================
                    ws.Columns(1, headers.Length).Width = 15;
                    ws.Column(1).Width = 15;
                    ws.Column(3).Width = 60;
                    ws.Column(5).Width = 30;
                    ws.Column(15).Width = 35;
                    ws.Column(17).Width = 35;
                    ws.Column(18).Width = 30;
                    ws.Column(19).Width = 60;

                    // ================= DATA =================
                    int row = headerRow + 1;
                    int stt = 1;
                foreach (var group in groupedData)
                {
                    var first = group.Items.First();

                    // 👉 Lấy KH 1 lần cho cả group
                    var kh = await _repoPartnerDetail
                        .GetPartnerInfoByIdWithCacheAsync(first.customer_detail_id, cancellationToken);

                    if (group.FileId.HasValue)
                    {
                        var fileInfo = listFile.FirstOrDefault(x => x.Id == group.FileId);

                        // 🔹 tách hải quan & vận chuyển
                        var dvItems = group.Items.Where(x => x.type == 0).ToList(); // hải quan
                        var vcItems = group.Items.Where(x => new [] { 1, 4 }.Contains((int)x.type)).ToList(); // vận chuyển

                        // 👉 nếu không có vận chuyển, nhưng có hải quan → vẫn xuất 1 dòng
                        if (!vcItems.Any() && dvItems.Any())
                        {
                            var dv = dvItems.First();

                            decimal giaDv = (decimal)dv.price + (decimal)dv.price_com;
                            decimal vatDv = giaDv * (decimal)dv.vat / 100m;
                            var taxCode = kh?.Partner?.TaxCode;
                            ws.Cell(row, 1).Value = stt++;
                            ws.Cell(row, 2).Value = dv.service_date.ToString("dd/MM/yyyy");
                            ws.Cell(row, 3).Value = kh?.Partner?.Name ?? "";
                            ws.Cell(row, 4).Value = kh?.Partner?.Abbreviation ?? "";
                            ws.Cell(row, 5).Value = kh?.Partner?.Address ?? "";
                            ws.Cell(row, 6).Value = string.IsNullOrWhiteSpace(taxCode) ? "" : $"MST: {taxCode}";
                            ws.Cell(row, 7).Value = fileInfo?.Sales ?? "";
                            ws.Cell(row, 11).Value = fileInfo?.Bill ?? "";
                            ws.Cell(row, 12).Value = "Chuyển khoản";
                            ws.Cell(row, 13).Value = "VND";
                            ws.Cell(row, 17).Value = 8;
                            ws.Cell(row, 18).Value = vatDv;
                            ws.Cell(row, 19).Value = "Phí dịch vụ giao nhận";
                            ws.Cell(row, 21).Value = "chuyến";
                            ws.Cell(row, 22).Value = 1;
                            ws.Cell(row, 23).Value = giaDv;
                            ws.Cell(row, 26).Value = giaDv;
                            ws.Cell(row, 27).Value = 4;
                            ws.Cell(row, 28).Value = 2;
                            ws.Cell(row, 31).Value = dv.vehicle_number ?? "";

                            row++;
                        }

                        // 👉 có vận chuyển → gộp hải quan vào chuyến đầu
                        for (int i = 0; i < vcItems.Count; i++)
                        {
                            var item = vcItems[i];

                            decimal giaDv;
                            decimal vatDv;

                            if (i == 0)
                            {
                                // 🔹 chuyến đầu: hải quan + vận chuyển 1
                                giaDv = dvItems.Sum(x => (decimal)x.price + (decimal)x.price_com)
                                      + (decimal)item.price + (decimal)item.price_com;

                                vatDv = dvItems.Sum(x =>
                                {
                                    var p = (decimal)x.price + (decimal)x.price_com;
                                    return p * (decimal)x.vat / 100m;
                                })
                                + ((decimal)item.price + (decimal)item.price_com) * (decimal)item.vat / 100m;
                            }
                            else
                            {
                                // 🔹 các chuyến sau: chỉ vận chuyển
                                giaDv = (decimal)item.price + (decimal)item.price_com;
                                vatDv = giaDv * (decimal)item.vat / 100m;
                            }

                            ws.Cell(row, 1).Value = stt++;
                            ws.Cell(row, 2).Value = item.service_date.ToString("dd/MM/yyyy");
                            ws.Cell(row, 3).Value = kh?.Partner?.Name ?? "";
                            ws.Cell(row, 4).Value = kh?.Partner?.Abbreviation ?? "";
                            ws.Cell(row, 5).Value = kh?.Partner?.Address ?? "";
                            ws.Cell(row, 6).Value = kh?.Partner?.TaxCode ?? "";
                            ws.Cell(row, 7).Value = fileInfo?.Sales ?? "";
                            ws.Cell(row, 11).Value = fileInfo?.Bill ?? "";
                            ws.Cell(row, 12).Value = "Chuyển khoản";
                            ws.Cell(row, 13).Value = "VND";
                            ws.Cell(row, 17).Value = 8;
                            ws.Cell(row, 18).Value = vatDv;
                            ws.Cell(row, 19).Value = DebitDto.ExportHasBill == 1? "CƯỚC VẬN CHUYỂN " + (item.name ?? "") +" BILL: "+(fileInfo?.Bill ?? ""): "CƯỚC VẬN CHUYỂN " + (item.name ?? "");
                            ws.Cell(row, 21).Value = "chuyến";
                            ws.Cell(row, 22).Value = 1;
                            ws.Cell(row, 23).Value = giaDv;
                            ws.Cell(row, 26).Value = giaDv;
                            ws.Cell(row, 27).Value = 4;
                            ws.Cell(row, 28).Value = 2;
                            ws.Cell(row, 31).Value = item.vehicle_number ?? "";

                            row++;
                        }
                    }
                    else
                    {
                        // 👉 Không có FileId
                        if (DebitDto.Invoice == 1)
                        {
                            foreach (var item in group.Items)
                            {
                                decimal price = (decimal)item.price + (decimal)item.price_com;
                                decimal vat = price * (decimal)item.vat / 100m;

                                ws.Cell(row, 1).Value = stt++;
                                ws.Cell(row, 2).Value = item.service_date.ToString("dd/MM/yyyy");
                                ws.Cell(row, 3).Value = kh?.Partner?.Name ?? "";
                                ws.Cell(row, 4).Value = kh?.Partner?.Abbreviation ?? "";
                                ws.Cell(row, 5).Value = kh?.Partner?.Address ?? "";
                                ws.Cell(row, 6).Value = kh?.Partner?.TaxCode ?? "";
                                ws.Cell(row, 12).Value = "Chuyển khoản";
                                ws.Cell(row, 13).Value = "VND";
                                ws.Cell(row, 17).Value = 8;
                                ws.Cell(row, 18).Value = vat;
                                ws.Cell(row, 19).Value = item.type == 1 ? "CƯỚC VẬN CHUYỂN " + (item.name ?? ""): "Phí dịch vụ giao nhận";
                                ws.Cell(row, 21).Value = "chuyến";
                                ws.Cell(row, 22).Value = 1;
                                ws.Cell(row, 23).Value = price;
                                ws.Cell(row, 26).Value = price;
                                ws.Cell(row, 27).Value = 4;
                                ws.Cell(row, 28).Value = 2;
                                ws.Cell(row, 31).Value = item.vehicle_number ?? "";

                                row++;
                            }
                        }
                    }
                }


                // ================= BORDER DATA =================
                // var dataRange = ws.Range(headerRow + 1, 1, ws.LastRowUsed().RowNumber(), headers.Length);
                // dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                // dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ================= EXPORT =================
                using var stream = new MemoryStream();
                    wb.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ChiTietHoaDonKH.xlsx"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return ApiResponseResult<object>(false, ex.Message, null);
                }
        }
        [HttpGet("excel/chitietfilegia")]
        public async Task<IActionResult> ExportChiTietFileGia(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            try
                {
                    var result = await _repoDebit.GetObjectXuatHoaDonKHAsync(
                        DebitDto, page, pageSize, cancellationToken);
                    if (result?.Data == null || !result.Data.Any())
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                    var rows = result.Data.Cast<object>().ToList();
                    if (!rows.Any())
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("RawData");

                    // Determine headers from the first row
                    var firstRow = rows[0];
                    var headers = new List<string>();
                    if (firstRow is IDictionary<string, object> dictRow)
                    {
                        headers = dictRow.Keys.ToList();
                    }
                    else
                    {
                        headers = firstRow.GetType()
                            .GetProperties()
                            .Select(p => p.Name)
                            .ToList();
                    }

                    // Write header row
                    for (int col = 0; col < headers.Count; col++)
                    {
                        ws.Cell(1, col + 1).Value = headers[col];
                    }

                    ws.Range(1, 1, 1, headers.Count).Style.Font.Bold = true;
                    ws.SheetView.FreezeRows(1);
                    ws.Range(1, 1, 1, headers.Count).SetAutoFilter();

                    string GetSafeCellText(object obj)
                    {
                        if (obj == null)
                            return string.Empty;

                        var text = obj.ToString() ?? string.Empty;
                        const int maxLength = 32767;
                        return text.Length <= maxLength
                            ? text
                            : text.Substring(0, maxLength);
                    }

                    // Write data rows
                    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                    {
                        var row = rows[rowIndex];
                        for (int col = 0; col < headers.Count; col++)
                        {
                            object value = null;
                            if (row is IDictionary<string, object> dynamicRow)
                            {
                                dynamicRow.TryGetValue(headers[col], out value);
                            }
                            else
                            {
                                var prop = row.GetType().GetProperty(headers[col]);
                                value = prop?.GetValue(row);
                            }

                            ws.Cell(rowIndex + 2, col + 1).SetValue(GetSafeCellText(value));
                        }
                    }

                    ws.Columns().AdjustToContents();

                    using var stream = new MemoryStream();
                    wb.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ChiTietFileGia.xlsx"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return ApiResponseResult<object>(false, ex.Message, null);
                }
        }
        [HttpGet("select")]
        public async Task<IActionResult> GetSelectFileContact(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
             var result = await _context.FileInfos
                 .Where(x=>x.StorageId == FileInfoDto.StorageId)
                 .Select(x => new { x.Id, x.FileNumber })
                 .ToListAsync();

            if (result == null || !result.Any())
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = 0,
                Total = 0,
                Data = result.Cast<object>().ToList()
            };
            return ApiResponseResult(true, "Lấy dữ liệu thành công", _results);
        }
        [HttpGet("GetSelectFileInfoAsync")]
        public async Task<IActionResult> GetSelectFileInfoAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            var result = await _repoContractFile.GetSelectFileInfoAsync(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("codeFile")]
        public IActionResult GetCodeFile(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] GetCodeDto GetCodeDto = null)
        {
            var prefix = GetCodeDto.Type + GetCodeDto.yearMonth.ToString("yyMM");
            var result = SqlServerHelpers.GenerateFileNumber(_configuration.GetConnectionString("DefaultConnection"), "file_infos", "file_number", GetCodeDto.StorageId, prefix, 3);

            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = 0,
                Total = 0
            };
            _results.Extra["code"] = result;
            return ApiResponseResult(true, "Lấy dữ liệu thành công", _results);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FileInfoDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var prefix = dto.Feature == 0 ?  "IS" + dto.AccountingDate.ToString("yyMM") : "ES"+ dto.AccountingDate.ToString("yyMM");
                var FileNumber = await SqlServerHelpers.GenerateFileNumberEfAsync(conn, tran.GetDbTransaction(), "file_infos", "file_number", dto.StorageId, prefix, 3);

                if (dto.EmployeeIds == null || dto.EmployeeIds.Length == 0)
                    return ApiResponseResult<object>(false, "Nhân viên sales bắt buộc", null);

                // Check trùng FileNumber trong cùng storage (bỏ qua soft-deleted)
                var fileInfos = await _context.FileInfos.AnyAsync(f =>
                    f.FileNumber == dto.FileNumber &&
                    f.StorageId == dto.StorageId);
                if (fileInfos)
                    return ApiResponseResult<object>(false, "FileNumber đã tồn tại trong kho này", null);
               if (!string.IsNullOrWhiteSpace(dto.Bill))
                {
                    var fileBill = await _context.FileInfos.AnyAsync(f =>
                        f.Bill == dto.Bill &&
                        f.StorageId == dto.StorageId);

                    if (fileBill)
                        return ApiResponseResult<object>(false, "Hóa đơn đã tồn tại trong kho này", null);
                }
                if (!string.IsNullOrWhiteSpace(dto.Declaration))
                {
                    var fileDeclaration = await _context.FileInfos.AnyAsync(f =>
                        f.Declaration == dto.Declaration &&
                        f.StorageId == dto.StorageId);

                    if (fileDeclaration)
                        return ApiResponseResult<object>(false, "Số tờ khai đã tồn tại trong kho này", null);
                }

                var entity = new Vudaco.ContractFiles.Models.FileInfo
                {
                    CustomerDetailId = dto.CustomerDetailId,
                    AccountingDate = dto.AccountingDate,
                    StorageId = dto.StorageId,
                    FileNumber = FileNumber,
                    Declaration = dto.Declaration,
                    Bill = dto.Bill,
                    Quantity = dto.Quantity,
                    ContainerCode = dto.ContainerCode,
                    Sales = dto.Sales,
                    Type = dto.Type,
                    Feature = dto.Feature,
                    DeclarationQuantity = dto.DeclarationQuantity,
                    DeclarationType = dto.DeclarationType,
                    Business = dto.Business,
                    Occurrence = dto.Occurrence,
                    Note = dto.Note,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId
                };
                _context.FileInfos.Add(entity);
                await _context.SaveChangesAsync();
                foreach (var item in dto.EmployeeIds)
                {
                    var entity_FileInfoDetail = new FileInfoDetail
                    {
                        FileId = entity.Id,
                        EmployeeId = item,
                        StorageId = dto.StorageId,
                        Price = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };
                    _context.FileInfoDetails.Add(entity_FileInfoDetail);
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Thêm file thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost("updateVoLuuCont")]
        public async Task<IActionResult> UpdateVoLuuCont([FromBody] ConfirmFileInfoDto dto)
        {
            if (dto == null || dto.Ids == null || !dto.Ids.Any())
                return ApiResponseResult<object>(false, "Danh sách Id không hợp lệ", null);

            var entities = await _context.FileInfos
                .Where(x => dto.Ids.Contains(x.Id))
                .ToListAsync();

            if (!entities.Any())
                return ApiResponseResult<object>(false, "Không tìm thấy file", null);

            foreach (var item in entities)
            {
                item.NgayHetHan = dto.NgayHetHan;
                item.NgayKeoCont = dto.NgayKeoCont;
            }

            await _context.SaveChangesAsync(); // 🔥 QUAN TRỌNG

            return ApiResponseResult(true, "Cập nhật file thành công", entities);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] FileInfoDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.FileInfos.AsTracking().FirstOrDefaultAsync(f => f.Id == dto.Id);
                if (entity == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy file", null);

                // Check trùng FileNumber trong cùng storage (bỏ qua soft-deleted và chính nó)
                var fileInfos = await _context.FileInfos.AnyAsync(f =>
                    f.Id != dto.Id &&
                    f.FileNumber == dto.FileNumber &&
                    f.StorageId == dto.StorageId);
                if (fileInfos)
                    return ApiResponseResult<object>(false, "FileNumber đã tồn tại trong kho này", null);

                if (!string.IsNullOrWhiteSpace(dto.Bill))   
                {
                    var fileBill = await _context.FileInfos.AnyAsync(f =>
                        f.Id != dto.Id &&
                        f.Bill == dto.Bill &&
                        f.StorageId == dto.StorageId);
                    if (fileBill)
                        return ApiResponseResult<object>(false, "Hóa đơn đã tồn tại trong kho này", null);
                }

                if (!string.IsNullOrWhiteSpace(dto.Declaration))
                {
                    var fileDeclaration = await _context.FileInfos.AnyAsync(f =>
                        f.Id != dto.Id &&
                        f.Declaration == dto.Declaration &&
                        f.StorageId == dto.StorageId);
                    if (fileDeclaration)
                        return ApiResponseResult<object>(false, "Số tờ khai đã tồn tại trong kho này", null);
                }

                var totalPrice = await _context.FileInfoDetails
                .Where(f =>
                    f.FileId == dto.Id &&
                    f.StorageId == dto.StorageId
                )
                .SumAsync(f => f.Price);
                if (totalPrice > 0)
                {
                    return ApiResponseResult<object>(false, "Số file này. đã được duyệt ứng: " + totalPrice, null);
                }
                entity.CustomerDetailId = dto.CustomerDetailId;
                entity.AccountingDate = dto.AccountingDate;
                entity.StorageId = dto.StorageId;
                entity.FileNumber = dto.FileNumber;
                entity.Declaration = dto.Declaration;
                entity.Bill = dto.Bill;
                entity.Quantity = dto.Quantity;
                entity.ContainerCode = dto.ContainerCode;
                entity.Sales = dto.Sales;
                entity.Type = dto.Type;
                entity.Feature = dto.Feature;
                entity.DeclarationQuantity = dto.DeclarationQuantity;
                entity.DeclarationType = dto.DeclarationType;
                entity.Business = dto.Business;
                entity.Occurrence = dto.Occurrence;
                entity.Note = dto.Note;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.Now;

                _context.FileInfos.Update(entity);
                await _context.SaveChangesAsync();
                var FileInfoDetails = await _context.FileInfoDetails.Where(x => x.FileId == dto.Id).ToListAsync();
                foreach (var item in FileInfoDetails)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = userId;
                    await _repoContractFileDetail.DeleteSoftAsync(item);
                }
                foreach (var item in dto.EmployeeIds)
                {
                    var entity_FileInfoDetail = new FileInfoDetail
                    {
                        FileId = entity.Id,
                        EmployeeId = item,
                        StorageId = dto.StorageId,
                        Price = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };
                    _context.FileInfoDetails.Add(entity_FileInfoDetail);
                }
                var debits = await _context.Debits.Where(x => x.FileInfoId == entity.Id).ToListAsync();
                foreach (var item in debits)
                {
                     item.AccountingDate = dto.AccountingDate;
                    _context.Debits.Update(item);
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Cập nhật file thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost("updateNullVoLuuCont")]
        public async Task<IActionResult> updateNullVoLuuCont([FromBody] FileInfoDto FileInfoDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                if (FileInfoDto.Id <= 0)
                {
                    return ApiResponseResult<object>(false, "Id không tồn tại", null);
                }
                var FileInfo = _context.FileInfos.Find(FileInfoDto.Id);
                if (FileInfo == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                FileInfo.NgayKeoCont = null;
                FileInfo.NgayHetHan = null;
                _context.FileInfos.Update(FileInfo);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Xóa thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] FileInfoDto FileInfoDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                if (FileInfoDto.Id <= 0)
                {
                    return ApiResponseResult<object>(false, "Id không tồn tại", null);
                }
                var FileInfo = _context.FileInfos.Find(FileInfoDto.Id);
                if (FileInfo == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                var totalPrice = await _context.FileInfoDetails
                .Where(f =>
                    f.FileId == FileInfo.Id &&
                    f.StorageId == FileInfo.StorageId
                )
                .SumAsync(f => f.Price);
                if (totalPrice > 0)
                {
                    return ApiResponseResult<object>(false, "Số file này. đã được duyệt ứng: " + totalPrice, null);
                }
                var FileInfoDetails = await _context.FileInfoDetails.Where(x => x.FileId == FileInfoDto.Id).ToListAsync();
              
                FileInfo.DeletedBy = userId;
                FileInfo.DeletedAt = DateTime.Now;
                await _repoContractFile.DeleteSoftAsync(FileInfo);
                foreach (var item in FileInfoDetails)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = userId;
                    await _repoContractFileDetail.DeleteSoftAsync(item);
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Xóa thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoContractFile.ShowWithDebitAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("showWithDebit")]
        public async Task<IActionResult> ShowWithDebit([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoContractFile.ShowWithDebitAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("ShowWithDebitConfirmAsync")]
        public async Task<IActionResult> ShowWithDebitConfirmAsync([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoContractFile.ShowWithDebitConfirmAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("ShowWithDebitHasNCC")]
        public async Task<IActionResult> ShowWithDebitHasNCC([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoContractFile.ShowWithDebitHasNCCAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }

    }
}
