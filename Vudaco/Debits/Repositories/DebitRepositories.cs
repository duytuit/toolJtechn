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
        public async Task<Debit> ShowWithFileInfoAsync(int id)
        {
            var debit = await _context.Debits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (debit == null) return null;

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

        public async Task<PaginatedResultReact<object>> GetObjectDebitCuocTamThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                        d.*,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM debits d
                    LEFT JOIN file_infos f 
                        ON d.file_info_id = f.id
                        AND d.customer_detail_id = f.customer_detail_id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN confirm_file_infos cf 
                         ON d.id = cf.debit_id
                    WHERE 
                        p.status = 1
                        AND d.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND f.deleted_at IS NULL
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

        public Task<PaginatedResultReact<object>> GetObjectDebitTamThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultReact<object>> GetObjectDebitDauKyKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        d.*,
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
                        d.*,
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
                        d.*,
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

        public async Task<PaginatedResultReact<object>> GetObjectDebitChiTietKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                    d.*,
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
                                    AND r.type_receipt = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (d.service_id NOT IN (19) OR d.service_id IS NULL)
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
            sql += " ORDER BY d.file_info_id,d.customer_detail_id,d.type,d.accounting_date";
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
                            SUM(rdt.amount * (rdt.vat / 100.0)) 
                            + SUM(rdt.amount) AS total_receipt
                        FROM receipts r
                        INNER JOIN receipt_details rdt 
                            ON rdt.receipt_id = r.id
                        WHERE
                            r.type_receipt = 0
                            AND r.deleted_at IS NULL
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
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
                    LEFT JOIN ReceiptTotal rt 
                        ON rt.debit_id = d.id
                    WHERE
                        p.status = 1
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (d.service_id NOT IN (19) OR d.service_id IS NULL)
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
            sql += $@" AND d.accounting_date < '{DebitDto.FromDate.Value:yyyy-MM-dd}'";
            return await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
        }
          public async Task<List<object>> GetObjectDebitDuNoDKNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    WITH ReceiptTotal AS (
                        SELECT
                            rdt.debit_id,
                            SUM(rdt.amount * (rdt.vat / 100.0)) 
                            + SUM(rdt.amount) AS total_receipt
                        FROM receipts r
                        INNER JOIN receipt_details rdt 
                            ON rdt.receipt_id = r.id
                        WHERE
                            r.type_receipt = 7
                            AND r.deleted_at IS NULL
                            AND rdt.deleted_at IS NULL
                        GROUP BY rdt.debit_id
                    )

                    SELECT
                        CAST(SUM(
                            (d.purchase_price + COALESCE(d.purchase_com, 0)) 
                            * (1 + d.purchase_vat / 100.0)
                        ) AS INT) AS total_debit,

                        CAST(SUM(ISNULL(rt.total_receipt, 0)) AS INT) AS total_receipt
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
                        d.*,
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
                        d.*,
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
                        d.*,
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
                        d.*,
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
                        d.*,
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
                        d.*,
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
                    SELECT 
                    d.*,
                    CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                    CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                    CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total
                    FROM debits d
                    LEFT JOIN file_infos f
                    ON f.id = d.file_info_id
                    LEFT JOIN partner_details p 
                    ON p.id = d.supplier_detail_id
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
                                    AND r.type_receipt = 7
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 2
                    AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                    AND (d.service_id NOT IN (19 ,33 ) OR d.service_id IS NULL)
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
            sql += " ORDER BY d.file_info_id,d.supplier_detail_id,d.type,d.accounting_date";
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
                    d.*,
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
                                    AND r.type_receipt = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.status = 2
                    AND d.service_id IN (19,33)
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
                    d.*,
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
                                    AND r.type_receipt = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                    p.status = 1
                    AND d.status = 2
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

        public async Task<PaginatedResultReact<object>> GetObjectDebitBuTruKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        d.*,
                        CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                        CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                        CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total,
                        -- Công nợ còn lại
                        (d.price + (d.price * d.vat) / 100.0 - ISNULL(rdt_total.total, 0)) AS remain_debit
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.customer_detail_id
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
                                    AND r.type_receipt = 0
                                    AND r.deleted_at IS NULL
                                    AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                        p.status = 1
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (d.service_id NOT IN (19) OR d.service_id IS NULL)
                        AND p.deleted_at IS NULL
                        AND d.deleted_at IS NULL
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
                        d.*,
                        CAST(ISNULL(rdt_total.amount, 0) AS INT) AS receipt_amount,
                        CAST(ISNULL(rdt_total.vat, 0) AS INT) AS receipt_vat,
                        CAST(ISNULL(rdt_total.total, 0) AS INT) AS receipt_total,
                        (d.purchase_price + (d.purchase_price * d.purchase_vat) / 100.0 - ISNULL(rdt_total.total, 0)) AS remain_debit
                    FROM debits d
                    LEFT JOIN partner_details p 
                        ON p.id = d.supplier_detail_id
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
                                AND r.type_receipt = 7
                                AND r.deleted_at IS NULL
                                AND rdt.deleted_at IS NULL
                    ) AS rdt_total
                    WHERE
                        p.status = 2
                        AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                        AND (d.service_id NOT IN (19) OR d.service_id IS NULL)
                        AND p.deleted_at IS NULL
                        AND d.deleted_at IS NULL
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
                    d.*
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
                    d.*
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
    }
}
