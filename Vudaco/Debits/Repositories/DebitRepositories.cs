using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Debits.Repositories
{
    public class DebitRepositories : BaseRepository<Debit>, IDebitRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;

        public const int PhiHaiQuan = 0;
        public const int PhiVanChuyen = 1;
        public const int PhiChiHo = 2;
        public const int PhiNangHa = 3;
        public const int PhiKhac = 4;
        public const int PhiDichVuDK_KH = 5;
        public const int PhiChihoDK_KH = 6;
        public const int MuaHangNCC = 7;
        public const int BanHangKH = 8;
        public const int BanHangNV = 9;
        public const int PhiDichVuDK_NCC = 10;
        public const int PhiChihoDK_NCC = 11;
        public const int PhiKhacNCC = 12;
        //=============================================
        public const int ServiceStatusDaHoanCuoc = 5;
        public const int ServiceStatusDaHoanTien = 4;
        public const int ServiceStatusDaHoanTra = 3;
        public DebitRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Debit> CreateAsync(Debit Debit)
        {
              _context.Debits.Add(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }

        public Task<Debit> DeleteSoftAsync(Debit Debit)
        {
            _context.Debits.Update(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }
        public async Task<PaginatedResultReact<object>> GetObjectDebitDispatchAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };
            if (DebitDto.StorageId > 0)
                whereEquals["storage_id"] = DebitDto.StorageId;
            if (DebitDto.CustomerDetailId > 0)
                whereEquals["customer_detail_id"] = DebitDto.CustomerDetailId;
            if (DebitDto.FileInfoId > 0)
                whereEquals["file_info_id"] = DebitDto.FileInfoId;

            whereEquals["type"] = PhiVanChuyen;
            whereCustoms.Add(("employee_staff_id IS NULL", Array.Empty<object>()));

            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
                whereDateRange.Add(("accounting_date", DebitDto.FromDate.Value, DebitDto.ToDate.Value));
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "debits",
                        new[] { "id","bill_id","vehicle_id","customer_detail_id","supplier_detail_id","file_info_id","employee_staff_id","employee_driver_id","storage_id","type","dispatch_code","name","accounting_date","purchase_price","price","vat","driver_fee","meal_fee","ticket_fee","overnight_fee","penalty_fee","goods_fee","status","data","note","customer_vehicle_type","supplier_vehicle_type","approved_by_user","approval_time","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        whereCustom: whereCustoms,
                        redisCache: _redis,
                        includeCount: false,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (DebitDto.StorageId > 0)
                whereEquals["storage_id"] = DebitDto.StorageId;
            if (DebitDto.CustomerDetailId > 0)
                whereEquals["customer_detail_id"] = DebitDto.CustomerDetailId;
            if (DebitDto.CustomerDetailId > 0)
                whereEquals["supplier_detail_id"] = DebitDto.SupplierDetailId;
            if (DebitDto.Status > 0)
                whereEquals["status"] = DebitDto.Status;
            if (DebitDto.FileInfoId > 0)
                whereEquals["file_info_id"] = DebitDto.FileInfoId;
            if (DebitDto.Type > 0)
                whereEquals["type"] = DebitDto.Type;
            if (DebitDto.EmployeeStaffId > 0)
            {
                whereEquals["employee_staff_id"] = DebitDto.EmployeeStaffId;
            }
            else
            {
                whereCustoms.Add(("employee_staff_id IS NULL", Array.Empty<object>()));
            }
              
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
                whereDateRange.Add(("accounting_date", DebitDto.FromDate.Value, DebitDto.ToDate.Value));
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "debits",
                        new[] { "id","bill_id","vehicle_id","customer_detail_id","supplier_detail_id","file_info_id","employee_staff_id","employee_driver_id","storage_id","type","dispatch_code","name","accounting_date","purchase_price","price","vat","driver_fee","meal_fee","ticket_fee","overnight_fee","penalty_fee","goods_fee","status","data","note","customer_vehicle_type","supplier_vehicle_type","approved_by_user","approval_time","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        whereCustom: whereCustoms,
                        redisCache: _redis,
                        includeCount: false,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }
        public Task<Debit> ShowAsync(int id)
        {
             return _context.Debits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Debit> ShowWithPurchaseNCCAsync(int id)
        {
            var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == id);

            if (debit == null) return null;
            var receipt = await _context.Receipts.FirstOrDefaultAsync(x => x.PurchaseDebitId == debit.Id);
            if (receipt != null)
            {
                var receiptDetails = await _context.ReceiptDetails.Where(x => x.ReceiptId == receipt.Id).ToListAsync();
                receipt.ReceiptDetails = receiptDetails;
                debit.Receipt = receipt;
            }
            return debit;
        }
        public async Task<Debit> ShowWithFileInfoAsync(int id)
        {
            var debit = await _context.Debits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (debit == null) return null;
            if (debit.CustomerDetailId.HasValue && debit.CustomerDetailId.Value > 0)
            {
                debit.CustomerDetail = await _context.Partners
                    .Join(
                        _context.PartnerDetails.Where(pd => pd.Id == debit.CustomerDetailId.Value),
                        p => p.Id,
                        pd => pd.PartnerId,
                        (p, pd) => p
                    )
                    .FirstOrDefaultAsync();
            }
            if (debit.SupplierDetailId.HasValue && debit.SupplierDetailId.Value > 0)
            {
                debit.SupplierDetail = await _context.Partners
                    .Join(
                        _context.PartnerDetails.Where(pd => pd.Id == debit.SupplierDetailId.Value),
                        p => p.Id,
                        pd => pd.PartnerId,
                        (p, pd) => p
                    )
                    .FirstOrDefaultAsync();
            }
            debit.FileInfo = await _context.FileInfos
                .Where(d => d.Id == debit.FileInfoId)
                .FirstOrDefaultAsync();
            return debit;
        }
        public async Task<List<Debit>> ShowByFileIdAsync(int FileId)
        {
            return await _context.Debits.Where(x => x.FileInfoId == FileId && x.Status == 0).ToListAsync();
        }
        public Task<Debit> UpdateAsync(Debit Debit)
        {
            _context.Debits.Update(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitPhiTamThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at
                    FROM debits d
                    LEFT JOIN file_infos f 
                        ON d.file_info_id = f.id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    WHERE 
                        p.status = 1
                        AND d.service_id = 33
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND (f.deleted_at IS NULL OR d.file_info_id IS NULL)";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }      
        public async Task<PaginatedResultReact<object>> GetObjectDebitDauKyKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type in (5,6)
                        AND p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitDauKyNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type in (10,11)
                        AND p.status = 2
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitMuaBanAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type = 6
                        AND p.status = 2
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectXuatHoaDonKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                    CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                    CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total,
                    p.customer_credit_limit
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    -- ✅ Tổng receipts
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN income_expense_categorys iecat
                            ON iecat.id = r.income_expense_category_id
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = rdt.debit_id 
                                    AND iecat.type = 0
                                    AND (r.status IS NULL OR r.status = 1)
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.type in (0,1,2,3,4,5,6,8)
                    AND (d.status > 0 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND ( d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";

            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FileInfoIds != null)
            {
                sql += $" AND d.file_info_id IN ({DebitDto.FileInfoIds})";
            }
            else
            {
                if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
                {
                    // Cộng thêm 1 ngày cho ToDate
                    var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                    // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                    sql += $@" AND f.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                            AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
                }
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,f.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectDebitChiTietKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                   WITH receipt_agg AS (
                    SELECT 
                        rdt.debit_id,
                        SUM(CAST(rdt.amount AS DECIMAL(18,2))) AS amount,
                        MAX(rdt.vat) AS vat,
                        SUM(CAST(rdt.amount AS DECIMAL(18,2)) * (rdt.vat / 100.0)) 
                         + SUM(CAST(rdt.amount AS DECIMAL(18,2))) AS total
                    FROM receipts r
                    LEFT JOIN receipt_details rdt 
                        ON rdt.receipt_id = r.id
                    LEFT JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    WHERE 
                        (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 0   -- ✅ đổi theo yêu cầu
                        AND r.deleted_at IS NULL
                        AND rdt.deleted_at IS NULL
                    GROUP BY rdt.debit_id
                ),

                receipt_code_agg AS (
                    SELECT 
                        d.id AS debit_id,
                        STUFF((
                            SELECT ', ' + r2.code_receipt
                            FROM receipts r2
                            LEFT JOIN receipt_details rdt2 
                                ON rdt2.receipt_id = r2.id
                            LEFT JOIN income_expense_categorys iecat2
                                ON iecat2.id = r2.income_expense_category_id
                            WHERE 
                                rdt2.debit_id = d.id
                                AND (r2.status IS NULL OR r2.status = 1)
                                AND iecat2.type = 0   -- ✅ đồng bộ điều kiện
                                AND r2.deleted_at IS NULL
                                AND rdt2.deleted_at IS NULL
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS receipt_codes
                    FROM debits d
                )

                SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    f.file_number,
                    f.declaration,
                    f.bill,
                    f.container_code,
                    CAST(ISNULL(ra.amount, 0) AS BIGINT) AS receipt_amount,
                    CAST(ISNULL(ra.vat, 0) AS BIGINT) AS receipt_vat,
                    CAST(ISNULL(ra.total, 0) AS BIGINT) AS receipt_total,
                    rca.receipt_codes,
                    p.customer_credit_limit   -- ✅ thêm field
                FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                LEFT JOIN partner_details p ON p.id = d.customer_detail_id  -- ✅ đổi đúng

                -- ✅ join aggregate
                LEFT JOIN receipt_agg ra ON ra.debit_id = d.id
                LEFT JOIN receipt_code_agg rca ON rca.debit_id = d.id

                WHERE
                    p.status = 1
                    AND d.type IN (0,1,2,3,4,5,6,8)
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (
                        d.service_id NOT IN (19,33) 
                        OR (d.service_id = 33 AND d.service_status > 2) 
                        OR d.service_id IS NULL
                    )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
         public async Task<List<object>> GetObjectDebitDuNoDKKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    WITH ReceiptTotal AS (
                    SELECT
                        rdt.debit_id,
                        SUM(
                            rdt.amount
                            + rdt.amount * (rdt.vat / 100.0)
                        ) AS total_receipt
                    FROM receipts r
                    INNER JOIN receipt_details rdt 
                        ON rdt.receipt_id = r.id
                    INNER JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    WHERE
                        r.deleted_at IS NULL
                        AND (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 0
                        AND iecat.deleted_at IS NULL
                        AND rdt.deleted_at IS NULL
                    GROUP BY rdt.debit_id
                )

                SELECT
                    CAST(SUM(
                    (d.price + ISNULL(d.price_com, 0)) * (d.vat / 100.0)
                    + (d.price + ISNULL(d.price_com, 0))
                ) AS INT) AS total_debit,

                    CAST(SUM(ISNULL(rt.total_receipt, 0)) AS INT) AS total_receipt
                FROM debits d
                LEFT JOIN ReceiptTotal rt 
                    ON rt.debit_id = d.id
                LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                INNER JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                WHERE
                    d.deleted_at IS NULL
                    AND p.status = 1
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.type in (0,1,2,3,4,5,6,8)
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (d.service_id NOT IN (19, 33) OR d.service_id IS NULL)";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            sql += $@" AND d.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
           // _ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            return await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
        }
          public async Task<List<object>> GetObjectDebitDuNoDKNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    WITH ReceiptTotal AS (
                        SELECT
                            rdt.debit_id,
                            SUM(
                                CAST(rdt.amount AS DECIMAL(18,2)) 
                                * (1 + rdt.vat / 100.0)
                            ) AS total_receipt
                        FROM receipts r
                        INNER JOIN receipt_details rdt 
                            ON rdt.receipt_id = r.id
                        INNER JOIN income_expense_categorys iecat
                            ON iecat.id = r.income_expense_category_id
                        WHERE
                            iecat.type = 0
                            AND (r.status IS NULL OR r.status = 1)
                            AND iecat.deleted_at IS NULL
                            AND r.deleted_at IS NULL
                            AND rdt.deleted_at IS NULL
                        GROUP BY rdt.debit_id
                    )

                    SELECT
                        CAST(SUM(
                            (d.purchase_price + COALESCE(d.purchase_com, 0)) 
                            * (1 + d.purchase_vat / 100.0)
                        ) AS DECIMAL(18,2)) AS total_debit,

                        CAST(SUM(ISNULL(rt.total_receipt, 0)) AS DECIMAL(18,2)) AS total_receipt
                    FROM debits d
                    INNER JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN file_infos f
                        ON f.id = d.file_info_id
                    LEFT JOIN ReceiptTotal rt 
                        ON rt.debit_id = d.id
                    WHERE
                        p.status = 2
                        AND d.supplier_detail_id IS NOT NULL
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (d.service_id NOT IN (19,33) OR d.service_id IS NULL)
                        AND p.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            sql += $@" AND d.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
            return await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
        }
        public async Task<PaginatedResultReact<object>> GetObjectNoDebitNoFileNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.purchase_status = 0
                        AND d.file_info_id IS NULL
                        AND p.status = 2
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            _ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectHasDebitNoFileNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.purchase_status = 1
                        AND d.file_info_id IS NULL
                        AND p.status = 2
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
       
        public async Task<PaginatedResultReact<object>> GetObjectNoDebitDispatchNoFileKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type = 1
                        AND d.file_info_id IS NULL
                        AND d.status = 0
                        AND p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectHasDebitDispatchNoFileKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type = 1
                        AND d.file_info_id IS NULL
                        AND d.status = 2
                        AND p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectBanHangKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        d.type = 8
                        AND p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectMuaHangNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by,
                        CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                        CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                        CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    -- ✅ Tổng receipts
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = r.purchase_debit_id 
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE 
                        d.type = 7
                        AND p.status = 2
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitChiTietNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    WITH receipt_agg AS (
                    SELECT 
                        rdt.debit_id,
                       SUM(CAST(rdt.amount AS DECIMAL(18,2))) AS amount,
                        MAX(rdt.vat) AS vat,
                       SUM(CAST(rdt.amount AS DECIMAL(18,2)) * (rdt.vat / 100.0)) 
                       + SUM(CAST(rdt.amount AS DECIMAL(18,2))) AS total
                    FROM receipts r
                    LEFT JOIN receipt_details rdt 
                        ON rdt.receipt_id = r.id
                    LEFT JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    WHERE 
                        (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 1
                        AND r.deleted_at IS NULL
                        AND rdt.deleted_at IS NULL
                    GROUP BY rdt.debit_id
                ),

                receipt_code_agg AS (
                    SELECT 
                        d.id AS debit_id,
                        STUFF((
                            SELECT ', ' + r2.code_receipt
                            FROM receipts r2
                            LEFT JOIN receipt_details rdt2 
                                ON rdt2.receipt_id = r2.id
                            LEFT JOIN income_expense_categorys iecat2
                                ON iecat2.id = r2.income_expense_category_id
                            WHERE 
                                rdt2.debit_id = d.id
                                AND (r2.status IS NULL OR r2.status = 1)
                                AND iecat2.type = 1
                                AND r2.deleted_at IS NULL
                                AND rdt2.deleted_at IS NULL
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS receipt_codes
                    FROM debits d
                )

                SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    f.file_number,
                    f.declaration,
                    f.bill,
                    f.container_code,
                    CAST(ISNULL(ra.amount, 0) AS BIGINT) AS receipt_amount,
                    CAST(ISNULL(ra.vat, 0) AS BIGINT) AS receipt_vat,
                    CAST(ISNULL(ra.total, 0) AS BIGINT) AS receipt_total,
                    rca.receipt_codes
                FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                LEFT JOIN partner_details p ON p.id = d.supplier_detail_id

                -- ✅ join dữ liệu đã gom
                LEFT JOIN receipt_agg ra ON ra.debit_id = d.id
                LEFT JOIN receipt_code_agg rca ON rca.debit_id = d.id

                WHERE
                    p.status = 2
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (
                        d.service_id NOT IN (19,33) 
                        OR (d.service_id IN (33) AND d.service_status > 2) 
                        OR d.service_id IS NULL
                    )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.accounting_date";
            // _ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitGiaoNhanAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    f.file_number,
                    f.declaration,
                    f.bill,
                    f.container_code,
                    CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                    CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                    CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    -- ✅ Tổng receipts
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = rdt.debit_id 
                                    AND (r.status IS NULL OR r.status = 1)
                                    AND r.type_receipt = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.service_id IN (19,33)
                    AND d.service_status >=2
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.EmployeeStaffId > 0)
            {
                sql += $@" AND d.employee_staff_id = {DebitDto.EmployeeStaffId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitLaiXeAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
               var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                    CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                    CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    -- ✅ Tổng receipts
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = rdt.debit_driver_id 
                                    AND (r.status IS NULL OR r.status = 1)
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.type = 1 AND driver_fee > 0
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.EmployeeStaffId > 0)
            {
                sql += $@" AND d.employee_staff_id = {DebitDto.EmployeeStaffId}";
            }
            if (DebitDto.DriverStatus > 0)
            {
                sql += $@" AND d.driver_status = {DebitDto.DriverStatus}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectLoiNhuanXeTrongAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
         {
            var _results = new PaginatedResultReact<object>();

            var sql = @"
            SELECT
                v.id,
                v.number_code,
                v.note,
                COALESCE(SUM(d.price), 0) AS total_price,
                COALESCE(SUM(d.driver_fee), 0) AS total_driver_fee
            FROM vehicles v
            LEFT JOIN debits d
                ON d.vehicle_id = v.id
                AND d.type = 1
                AND d.deleted_at IS NULL";

            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);

                sql += $@"
                AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}'
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += @"
            WHERE 1 = 1";

            if (DebitDto.StorageId > 0)
            {
                sql += $@"
                AND v.storage_id = {DebitDto.StorageId}";
            }

            sql += @"
            GROUP BY
                v.id,
                v.number_code,
                v.note
            ORDER BY
                v.number_code";

            var debit_theoxe = await SqlServerHelpers.ExecuteQuerySqlAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                sql,
                cancellationToken);

            _results.Extra["debit_theoxe"] = debit_theoxe;
            // các loại phí
            // 16: phí sửa chữa
            // 17: phí dầu DO
            // 18: Cước đường bộ
            // 19: phí khác
            // 20: phí lãi vạy
            // 22 : trích lương nhân viên
            // 34: trích BHXH
            // 35: phí gửi xe
            // 38: phí đi đường lái xe
            sql = $@"SELECT v.number_code,v.note v_note,r.income_expense_category_id,rd.* FROM vehicles v LEFT JOIN receipt_details rd on rd.vehicle_id = v.id LEFT JOIN receipts r on r.id = rd.receipt_id WHERE r.deleted_at IS NULL AND rd.deleted_at IS NULL AND r.income_expense_category_id IN (38,34,35,22,19,20,18,17,16)";     
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }   
            var chi_theoxe = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["chi_theoxe"] = chi_theoxe;
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectLoiNhuanXeNgoaiAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
         {
            var sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    LEFT JOIN file_infos f 
                        ON d.file_info_id = f.id
                        AND d.customer_detail_id = f.customer_detail_id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE d.type = 1
                        AND p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND cf.deleted_at IS NULL
                        AND d.vehicle_id IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
           // _ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectLoiNhuanDoanhThuKhacAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
         {
            var _results = new PaginatedResultReact<object>();
            var sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    WHERE d.type = 8 AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var loinhuan_banhang = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["loinhuan_banhang"] = loinhuan_banhang;

            sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    WHERE d.type = 1 AND d.deleted_at IS NULL AND d.file_info_id IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var loinhuan_com = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["loinhuan_com"] = loinhuan_com;
             sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    WHERE d.type = 4 AND d.deleted_at IS NULL AND d.file_info_id IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var loinhuan_phikhac = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["loinhuan_phikhac"] = loinhuan_phikhac;
              sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    WHERE d.type = 7 AND d.deleted_at IS NULL AND d.file_info_id IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var loinhuan_muahang = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["loinhuan_muahang"] = loinhuan_muahang;
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectLoiNhuanHaiQuanAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
              var sql = $@"
                    SELECT
                        COALESCE(SUM(d.purchase_com), 0) AS total_purchase_com,
                        COALESCE(SUM(d.price_com), 0) AS total_price_com,
                        COALESCE(SUM(d.price), 0) AS total_price,
                        COALESCE(SUM(d.purchase_price), 0) AS total_purchase_price,
                        COALESCE(SUM(d.price), 0) - COALESCE(SUM(d.purchase_price), 0) AS profit
                    FROM debits d
                    WHERE d.type = 0 AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitBuTruKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    f.file_number,
                    f.declaration,
                    f.bill,
                    f.container_code,
                        p.customer_credit_limit,
                        CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                        CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                        CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total,
                        -- Công nợ còn lại
                        (d.price + (d.price * d.vat) / 100.0 - ISNULL(rdt_total.total, 0)) AS remain_debit
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN income_expense_categorys iecat
                            ON iecat.id = r.income_expense_category_id
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = rdt.debit_id 
                                    AND (r.status IS NULL OR r.status = 1)
                                    AND iecat.type = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                        p.status = 1
                        AND d.type in (0,1,2,3,4,5,6,8)
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND ( d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                        AND p.deleted_at IS NULL
                        AND d.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND (d.price + (d.price * d.vat) / 100.0 - ISNULL(rdt_total.total, 0)) > 0";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitBuTruNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    f.file_number,
                    f.declaration,
                    f.bill,
                    f.container_code,
                        p.supplier_credit_limit,
                        CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                        CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                        CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total,
                        (d.purchase_price + (d.purchase_price * d.purchase_vat) / 100.0 - ISNULL(rdt_total.total, 0)) AS remain_debit
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
                    OUTER APPLY (
                            SELECT 
                                SUM(rdt.amount) AS amount,
                                MAX(rdt.vat) AS vat,
                                SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN income_expense_categorys iecat
                            ON iecat.id = r.income_expense_category_id
                            LEFT JOIN receipt_details rdt 
                                ON rdt.receipt_id = r.id
                            WHERE 
                                d.id = rdt.debit_id 
                                AND (r.status IS NULL OR r.status = 1)
                                AND iecat.type = 1
                                AND r.deleted_at IS NULL
                                AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                        p.status = 2
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND ( d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                        AND p.deleted_at IS NULL
                        AND d.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND (d.purchase_price + (d.purchase_price * d.purchase_vat) / 100.0 - ISNULL(rdt_total.total, 0)) > 0";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectNoDebitNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.supplier_detail_id
                    WHERE
                    p.status = 2
                    AND d.purchase_status = 0
                    AND d.type NOT IN (10,11)
                    AND d.supplier_detail_id IS NOT NULL
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND ( d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectHasDebitNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.supplier_detail_id
                    WHERE
                    p.status = 2
                    AND d.purchase_status = 1
                    AND d.type NOT IN (10,11)
                    AND d.supplier_detail_id IS NOT NULL
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND ( d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectBaoCaoDoanhThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var _results = new PaginatedResultReact<object>();
            var sql = $@"
                -- doanh thu chi hộ
                SELECT COALESCE(SUM(price), 0) AS total_price FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                WHERE d.type IN (2,3) AND d.file_info_id>0 AND d.deleted_at IS NULL AND f.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var dt_ch_hasfile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["dt_ch_hasfile_results"] = dt_ch_hasfile_results;

            sql = $@"
               -- chi phí chi ho
                SELECT COALESCE(SUM(purchase_price), 0) AS total_purchase_price FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                WHERE d.type IN (2,3) AND d.file_info_id>0 AND d.deleted_at IS NULL AND f.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var cp_ch_hasfile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["cp_ch_hasfile_results"] = cp_ch_hasfile_results;

            sql = $@"
                -- doanh thu các lô hàng có lập file ,trừ mua hàng từ nhà cung cấp,trừ bán hàng cho khách hàng
                SELECT COALESCE(SUM(price), 0) AS total_price FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                WHERE d.type IN (0,1,4) AND d.file_info_id>0 AND d.deleted_at IS NULL AND f.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var dt_hasfile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["dt_hasfile_results"] = dt_hasfile_results;

            sql = $@"
               -- doanh thu các lô hàng không lập file, trừ mua hàng từ nhà cung cấp
              SELECT COALESCE(SUM(d.price), 0)+COALESCE(SUM(d.price_com), 0)+COALESCE(SUM(d.driver_fee),0) AS total_price FROM debits d WHERE d.type IN (1,8) AND (d.file_info_id IS NULL OR d.file_info_id = 0) AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var dt_nofile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["dt_nofile_results"] = dt_nofile_results;

            sql = $@"
               -- chi phí các lô hàng có lặp file
                SELECT COALESCE(SUM(purchase_price), 0) AS total_purchase_price FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                WHERE d.type IN (0,1,4) AND d.file_info_id>0 AND d.deleted_at IS NULL AND f.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var cp_hasfile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["cp_hasfile_results"] = cp_hasfile_results;

             sql = $@"
              -- chi phí các lô hàng không lặp file, trừ mua hàng từ nhà cung cấp
              SELECT  COALESCE(SUM(d.purchase_price), 0)+ COALESCE(SUM(d.purchase_com), 0) AS total_purchase_price FROM debits d WHERE d.type IN (1) AND (d.file_info_id IS NULL OR d.file_info_id = 0) AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var cp_nofile_results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["cp_nofile_results"] = cp_nofile_results;

             sql = $@"
             -- lấy phiếu chi có income_expense_categorys kiểu là chi phí kinh doanh
            SELECT 
                    sum(d.amount) AS amount
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                ON iecat.id = r.income_expense_category_id
                LEFT JOIN (
                        SELECT 
                                receipt_id,
                                SUM(amount) AS amount,
                                SUM(amount * (1 + vat / 100.0)) AS total
                        FROM receipt_details
                        WHERE deleted_at IS NULL
                        GROUP BY receipt_id
                ) d ON d.receipt_id = r.id
                WHERE 
                        (r.status IS NULL OR r.status = 0)
                        AND iecat.type = 1
                        AND (iecat.parent_id in (12) OR iecat.id = 12)
                        AND r.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var cp_kinhdoanh = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["cp_kinhdoanh"] = cp_kinhdoanh;
             sql = $@"
                SELECT 
                        sum(d.amount) AS amount
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                ON iecat.id = r.income_expense_category_id
                LEFT JOIN (
                        SELECT 
                                receipt_id,
                                SUM(amount) AS amount,
                                SUM(amount * (1 + vat / 100.0)) AS total
                        FROM receipt_details
                        WHERE deleted_at IS NULL
                        GROUP BY receipt_id
                ) d ON d.receipt_id = r.id
                WHERE 
                        (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 0
                        AND r.income_expense_category_id = 36
                        AND r.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var doanhthu_khac = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["doanhthu_khac"] = doanhthu_khac;

            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitTongHopKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var _results = new PaginatedResultReact<object>();
            var sql = $@"
            WITH DebitTotal AS (
                SELECT
                    d.customer_detail_id,
                    d.type,
                    CAST(SUM(
                        (d.price + ISNULL(d.price_com, 0))
                        * (1 + d.vat / 100.0)
                    ) AS INT) AS debit_total
                FROM debits d
                LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                INNER JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                WHERE
                    p.status = 1
                    AND f.deleted_at IS NULL
                    AND p.deleted_at IS NULL
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND ( d.service_id NOT IN (19,33) 
                        OR (d.service_id IN (33) AND d.service_status > 2) 
                        OR d.service_id IS NULL )
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            sql+= $@" GROUP BY d.customer_detail_id, d.type
            ),
            ReceiptTotal AS (
                SELECT 
                    db.customer_detail_id,
                    db.type,
                    CAST(SUM(rdt.total) AS INT) AS receipt_total
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                    ON iecat.id = r.income_expense_category_id
                LEFT JOIN (
                    SELECT 
                        receipt_id,
                        debit_id,
                        SUM(amount * (1 + vat / 100.0)) AS total
                    FROM receipt_details
                    WHERE deleted_at IS NULL
                    GROUP BY receipt_id, debit_id
                ) rdt
                    ON rdt.receipt_id = r.id
                INNER JOIN debits db
                    ON db.id = rdt.debit_id
                INNER JOIN partner_details p
                    ON p.id = db.customer_detail_id
                WHERE 
                    (r.status IS NULL OR r.status = 1)
                    AND iecat.type = 0
                    AND r.deleted_at IS NULL
                    AND p.deleted_at IS NULL
                    AND db.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND db.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            sql += $@" GROUP BY 
                    db.customer_detail_id,
                    db.type
            )
            SELECT
                COALESCE(dt.customer_detail_id, rt.customer_detail_id) AS customer_detail_id,
                COALESCE(dt.type, rt.type) AS type,
                ISNULL(dt.debit_total, 0) AS debit_total,
                ISNULL(rt.receipt_total, 0) AS receipt_total,
                ISNULL(dt.debit_total, 0) - ISNULL(rt.receipt_total, 0) AS remain_total
            FROM DebitTotal dt
            FULL OUTER JOIN ReceiptTotal rt
                ON rt.customer_detail_id = dt.customer_detail_id
                AND rt.type = dt.type
            ORDER BY
                customer_detail_id,
                type";
            var congnotonghop_kh = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            _results.Extra["congnotonghop_kh"] = congnotonghop_kh;
            sql = $@"
                WITH DebitOpening AS (
                    SELECT
                        d.customer_detail_id,
                        d.type,
                        CAST(SUM(
                            (d.price + ISNULL(d.price_com, 0))
                            * (1 + d.vat / 100.0)
                        ) AS INT) AS debit_opening
                    FROM debits d
                    LEFT JOIN file_infos f
                        ON f.id = d.file_info_id
                    INNER JOIN partner_details p
                        ON p.id = d.customer_detail_id
                    WHERE
                        p.status = 1
                        AND f.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (
                            d.service_id NOT IN (19,33)
                            OR (d.service_id IN (33) AND d.service_status > 2)
                            OR d.service_id IS NULL
                        )
                        AND d.deleted_at IS NULL";

                if (DebitDto.StorageId > 0)
                {
                    sql += $@" AND d.storage_id = {DebitDto.StorageId}";
                }

                if (DebitDto.FromDate.HasValue)
                {
                    // Đầu kỳ = tất cả trước FromDate
                    sql += $@" AND d.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
                }

                if (DebitDto.CustomerDetailId > 0)
                {
                    sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
                }

                sql += $@"
                    GROUP BY d.customer_detail_id, d.type
                ),
                ReceiptOpening AS (
                    SELECT
                        db.customer_detail_id,
                        db.type,
                        CAST(SUM(rdt.total) AS INT) AS receipt_opening
                    FROM receipts r
                    LEFT JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    LEFT JOIN (
                        SELECT
                            receipt_id,
                            debit_id,
                            SUM(amount * (1 + vat / 100.0)) AS total
                        FROM receipt_details
                        WHERE deleted_at IS NULL
                        GROUP BY receipt_id, debit_id
                    ) rdt
                        ON rdt.receipt_id = r.id
                    INNER JOIN debits db
                        ON db.id = rdt.debit_id
                    INNER JOIN partner_details p
                        ON p.id = db.customer_detail_id
                    WHERE
                        (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 0
                        AND r.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND db.deleted_at IS NULL";

                if (DebitDto.StorageId > 0)
                {
                    sql += $@" AND r.storage_id = {DebitDto.StorageId}";
                }

                if (DebitDto.FromDate.HasValue)
                {
                    // Đầu kỳ = tất cả trước FromDate
                    sql += $@" AND r.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
                }

                if (DebitDto.CustomerDetailId > 0)
                {
                    sql += $@" AND db.customer_detail_id = {DebitDto.CustomerDetailId}";
                }

                sql += $@"
                    GROUP BY db.customer_detail_id, db.type
                )
                SELECT
                    COALESCE(do.customer_detail_id, ro.customer_detail_id) AS customer_detail_id,
                    COALESCE(do.type, ro.type) AS type,
                    ISNULL(do.debit_opening, 0) AS debit_total,
                    ISNULL(ro.receipt_opening, 0) AS receipt_total,
                    ISNULL(do.debit_opening, 0) - ISNULL(ro.receipt_opening, 0) AS remain_total
                FROM DebitOpening do
                FULL OUTER JOIN ReceiptOpening ro
                    ON ro.customer_detail_id = do.customer_detail_id
                    AND ro.type = do.type
                ORDER BY
                    customer_detail_id,
                    type";
                var congnotonghop_dk_kh = await SqlServerHelpers.ExecuteQuerySqlAsync(
                    _configuration.GetConnectionString("DefaultConnection"),
                    sql,
                    cancellationToken
                );

            _results.Extra["congnotonghop_dk_kh"] = congnotonghop_dk_kh;
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitTongHopNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var _results = new PaginatedResultReact<object>();
            var sql = $@"
            WITH DebitTotal AS (
                SELECT
                    d.supplier_detail_id,
                    d.type,
                    CAST(SUM(
                        (d.purchase_price + ISNULL(d.purchase_com, 0))
                        * (1 + d.purchase_vat / 100.0)
                    ) AS INT) AS debit_total
                FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                INNER JOIN partner_details p ON p.id = d.supplier_detail_id
                WHERE
                    p.status = 2
                    AND f.deleted_at IS NULL
                    AND p.deleted_at IS NULL
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (
                        d.service_id NOT IN (19,33)
                        OR (d.service_id IN (33) AND d.service_status > 2)
                        OR d.service_id IS NULL
                    )
                    AND d.deleted_at IS NULL";

            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}'
                        AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }

            sql += $@"
                GROUP BY d.supplier_detail_id, d.type
            ),
            ReceiptTotal AS (
                SELECT
                    db.supplier_detail_id,
                    db.type,
                    CAST(SUM(
                        ISNULL(rdt.amount, 0) + ISNULL(rdt.amount * (rdt.vat / 100.0), 0)
                    ) AS INT) AS receipt_total
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                    ON iecat.id = r.income_expense_category_id
                INNER JOIN receipt_details rdt
                    ON rdt.receipt_id = r.id
                INNER JOIN debits db
                    ON db.id = rdt.debit_id
                INNER JOIN partner_details p
                    ON p.id = db.supplier_detail_id
                WHERE
                    iecat.deleted_at IS NULL
                    AND (r.status IS NULL OR r.status = 1)
                    AND rdt.deleted_at IS NULL
                    AND iecat.type = 1
                    AND r.deleted_at IS NULL
                    AND p.status = 2
                    AND p.deleted_at IS NULL
                    AND db.deleted_at IS NULL";

            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                sql += $@" AND r.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}'
                        AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND db.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }

            sql += $@"
                GROUP BY db.supplier_detail_id, db.type
            )
            SELECT
                COALESCE(dt.supplier_detail_id, rt.supplier_detail_id) AS supplier_detail_id,
                COALESCE(dt.type, rt.type) AS type,
                ISNULL(dt.debit_total, 0) AS debit_total,
                ISNULL(rt.receipt_total, 0) AS receipt_total,
                ISNULL(dt.debit_total, 0) - ISNULL(rt.receipt_total, 0) AS remain_total
            FROM DebitTotal dt
            FULL OUTER JOIN ReceiptTotal rt
                ON rt.supplier_detail_id = dt.supplier_detail_id
                AND rt.type = dt.type
            ORDER BY supplier_detail_id, type";

            var congnotonghop_ncc = await SqlServerHelpers.ExecuteQuerySqlAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                sql,
                cancellationToken
            );
            _results.Extra["congnotonghop_ncc"] = congnotonghop_ncc;
            sql = $@"
            WITH DebitOpening AS (
                SELECT
                    d.supplier_detail_id,
                    d.type,
                    CAST(SUM(
                        (d.purchase_price + ISNULL(d.purchase_com, 0))
                        * (1 + d.purchase_vat / 100.0)
                    ) AS INT) AS debit_total
                FROM debits d
                LEFT JOIN file_infos f ON f.id = d.file_info_id
                INNER JOIN partner_details p ON p.id = d.supplier_detail_id
                WHERE
                    p.status = 2
                    AND f.deleted_at IS NULL
                    AND p.deleted_at IS NULL
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (
                        d.service_id NOT IN (19,33)
                        OR (d.service_id IN (33) AND d.service_status > 2)
                        OR d.service_id IS NULL
                    )
                    AND d.deleted_at IS NULL";

            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue)
            {
                sql += $@" AND d.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND d.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }

            sql += $@"
                GROUP BY d.supplier_detail_id, d.type
            ),
            ReceiptOpening AS (
                SELECT
                    db.supplier_detail_id,
                    db.type,
                    CAST(SUM(
                        ISNULL(rdt.amount, 0) + ISNULL(rdt.amount * (rdt.vat / 100.0), 0)
                    ) AS INT) AS receipt_total
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                    ON iecat.id = r.income_expense_category_id
                INNER JOIN receipt_details rdt
                    ON rdt.receipt_id = r.id
                INNER JOIN debits db
                    ON db.id = rdt.debit_id
                INNER JOIN partner_details p
                    ON p.id = db.supplier_detail_id
                WHERE
                    iecat.deleted_at IS NULL
                    AND (r.status IS NULL OR r.status = 1)
                    AND rdt.deleted_at IS NULL
                    AND iecat.type = 1
                    AND r.deleted_at IS NULL
                    AND p.status = 2
                    AND p.deleted_at IS NULL
                    AND db.deleted_at IS NULL";

            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.FromDate.HasValue)
            {
                sql += $@" AND r.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
            }
            if (DebitDto.SupplierDetailId > 0)
            {
                sql += $@" AND db.supplier_detail_id = {DebitDto.SupplierDetailId}";
            }

            sql += $@"
                GROUP BY db.supplier_detail_id, db.type
            )
            SELECT
                COALESCE(dt.supplier_detail_id, rt.supplier_detail_id) AS supplier_detail_id,
                COALESCE(dt.type, rt.type) AS type,
                ISNULL(dt.debit_total, 0) AS debit_total,
                ISNULL(rt.receipt_total, 0) AS receipt_total,
                ISNULL(dt.debit_total, 0) - ISNULL(rt.receipt_total, 0) AS remain_total
            FROM DebitOpening dt
            FULL OUTER JOIN ReceiptOpening rt
                ON rt.supplier_detail_id = dt.supplier_detail_id
                AND rt.type = dt.type
            ORDER BY supplier_detail_id, type";

            var congnotonghop_dk_ncc = await SqlServerHelpers.ExecuteQuerySqlAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                sql,
                cancellationToken
            );

            _results.Extra["congnotonghop_dk_ncc"] = congnotonghop_dk_ncc;
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitPhiCuocAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN file_infos f 
                        ON d.file_info_id = f.id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        p.status = 1
                        AND d.service_id = 19
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND (f.deleted_at IS NULL OR d.file_info_id IS NULL)
                        AND cf.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.ServiceId > 0)
            {
                sql += $@" AND d.service_id = {DebitDto.ServiceId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY d.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitChiTietNoBillKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    WHERE
                    p.status = 1
                    AND NOT EXISTS (
                            SELECT 1
                            FROM bills b
                            WHERE d.bill_id = b.id AND b.deleted_at IS NULL
                    )
                    AND d.type in (0,1,2,3,4,5,6,8)
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                sql += $@" AND d.accounting_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectDebitChiTietHasBillKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    WHERE
                    p.status = 1
                    AND EXISTS (
                            SELECT 1
                            FROM bills b
                            WHERE d.bill_id = b.id AND b.deleted_at IS NULL
                    )
                    AND d.type in (0,1,2,3,4,5,6,8)
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (d.service_id NOT IN (19,33) OR (d.service_id IN (33) AND d.service_status > 2) OR d.service_id IS NULL )
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.CustomerDetailId > 0)
            {
                sql += $@" AND d.customer_detail_id = {DebitDto.CustomerDetailId}";
            }
            if (DebitDto.BillId > 0)
            {
                sql += $@" AND d.bill_id = {DebitDto.BillId}";
            }
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,d.accounting_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectTheoDoiNhacNoKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                   WITH debit_overdue AS (
                    SELECT
                        d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                        p.customer_credit_limit_month,
                        p.customer_credit_limit,
                        b.accounting_date AS b_accounting_date,
                        d.accounting_date AS d_accounting_date,

                        ISNULL(rdt_total.amount, 0) AS receipt_amount,
                        ISNULL(rdt_total.vat, 0) AS receipt_vat,
                        ISNULL(rdt_total.total, 0) AS receipt_total,

                        (
                                d.price_com + 
                                d.price 
                            + (d.price * d.vat) / 100.0 
                            - ISNULL(rdt_total.total, 0)
                        ) AS remain_debit,

                        DATEDIFF(DAY, b.accounting_date, GETDATE()) 
                            - p.customer_credit_limit_month AS overdue_days_of_month,

                        DATEDIFF(DAY, d.accounting_date, GETDATE()) 
                            - p.customer_credit_limit AS overdue_days_of_day

                    FROM debits d
                    LEFT JOIN file_infos f ON f.id = d.file_info_id
                    LEFT JOIN bills b ON b.id = d.bill_id
                    LEFT JOIN partner_details p ON p.id = d.customer_detail_id

                    OUTER APPLY (
                        SELECT 
                            SUM(rdt.amount) AS amount,
                            MAX(rdt.vat) AS vat,
                            SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                        FROM receipts r
                        JOIN receipt_details rdt ON rdt.receipt_id = r.id
                        JOIN income_expense_categorys iecat ON iecat.id = r.income_expense_category_id
                        WHERE 
                            d.id = rdt.debit_id
                            AND (r.status IS NULL OR r.status = 1)
                            AND iecat.type = 0
                            AND r.deleted_at IS NULL
                            AND rdt.deleted_at IS NULL
                    ) rdt_total

                    WHERE
                        p.status = 1";
                        if (DebitDto.StorageId > 0)
                        {
                            sql += $@" AND d.storage_id = {DebitDto.StorageId}";
                        }
                        sql += $@" AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (
                            d.service_id NOT IN (19,33)
                            OR (d.service_id = 33 AND d.service_status > 2)
                            OR d.service_id IS NULL
                        )
                        AND p.deleted_at IS NULL
                        AND d.deleted_at IS NULL
                        AND (f.deleted_at IS NULL OR d.file_info_id IS NULL)
                        AND (b.deleted_at IS NULL OR d.bill_id IS NULL)

                        AND (
                                    d.price_com +
                            d.price 
                            + (d.price * d.vat) / 100.0 
                            - ISNULL(rdt_total.total, 0)
                        ) > 0
                ),

                ranked AS (
                    SELECT *,
                        ROW_NUMBER() OVER (
                            PARTITION BY customer_detail_id
                            ORDER BY 
                                overdue_days_of_month DESC,
                                overdue_days_of_day DESC
                        ) AS rn
                    FROM debit_overdue
                )

                SELECT *
                FROM ranked
                WHERE rn = 1
                ORDER BY overdue_days_of_month DESC, overdue_days_of_day DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitLaiXeTinhLuongAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                    d.id,
                    d.bill_id,
                    d.supplier_bill_id,
                    d.vehicle_id,
                    d.customer_detail_id,
                    d.supplier_detail_id,
                    d.file_info_id,
                    d.employee_staff_id,
                    d.employee_driver_id,
                    d.storage_id,
                    d.service_id,
                    d.service_detail,
                    d.type,
                    d.dispatch_code,
                    d.name,
                    d.accounting_date,
                    d.service_date,
                    d.service_status,
                    d.purchase_accounting_date,
                    d.purchase_price,
                    d.purchase_vat,
                    d.price,
                    d.vat,
                    d.purchase_com,
                    d.price_com,
                    d.driver_fee,
                    d.meal_fee,
                    d.ticket_fee,
                    d.overnight_fee,
                    d.penalty_fee,
                    d.goods_fee,
                    d.delivery_point,
                    d.purchase_status,
                    d.status,
                    d.transportation_cost,
                    d.purchase_bill,
                    d.bill,
                    d.link_bill,
                    d.code_bill,
                    d.note,
                    d.purchase_note,
                    d.customer_vehicle_type,
                    d.supplier_vehicle_type,
                    d.vehicle_number,
                    d.approved_by_user,
                    d.approval_time,
                    d.cus_bill,
                    d.cus_bill_date,
                    d.sup_bill,
                    d.sup_bill_date,
                    d.driver_status,
                    d.created_by,
                    d.updated_by,
                    d.deleted_by,
                    d.deleted_at,
                    d.created_at,
                    d.updated_at,
                    CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                    CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                    CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.customer_detail_id
                    -- ✅ Tổng receipts
                    OUTER APPLY (
                            SELECT 
                                    SUM(rdt.amount) AS amount,
                                    MAX(rdt.vat) AS vat,
                                    SUM(rdt.amount * (rdt.vat / 100.0)) + SUM(rdt.amount) AS total
                            FROM receipts r
                            LEFT JOIN receipt_details rdt 
                                    ON rdt.receipt_id = r.id
                            WHERE 
                                    d.id = rdt.debit_driver_id 
                                    AND (r.status IS NULL OR r.status = 1)
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.type = 1
                    AND p.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND d.deleted_at IS NULL";
            if (DebitDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DebitDto.StorageId}";
            }
            if (DebitDto.EmployeeDriverId > 0)
            {
                sql += $@" AND d.employee_driver_id = {DebitDto.EmployeeDriverId}";
            }
            if (DebitDto.DriverStatus > 0)
            {
                sql += $@" AND d.driver_status = {DebitDto.DriverStatus}";
            }
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = DebitDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND d.service_date >= '{DebitDto.FromDate.Value:yyyy-MM-dd}' 
                AND d.service_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d.service_date";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
    }
}
