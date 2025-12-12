

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.ContractFiles.Dtos;
using Vudaco.ContractFiles.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.MysqlHelper;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.ContractFiles.Repositories
{
    public class ContractFileRepository :BaseRepository<FileInfo>, IContractFileRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public const int statusDichVu = 0;
        public const int statusFileGia = 1;
        public const int statusDebit = 2;
        public ContractFileRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
       public async Task<PaginatedResultReact<object>> GetObjectNoDebitHasFileNCCAsync(FileInfoDto FileInfoDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                        f.*
                    FROM file_infos f 
                    LEFT JOIN partner_details p 
                            ON p.id = f.customer_detail_id
                    WHERE 
                        p.status = 1
                        AND f.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND EXISTS (
                                SELECT 1
                                FROM debits d
                                WHERE 
                                    d.file_info_id = f.id
                                    AND d.supplier_detail_id is not null
                                    AND d.status in (1,2)
                                    AND d.purchase_status = 0
                                    AND d.deleted_at IS NULL
                            )";
            if (FileInfoDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {FileInfoDto.StorageId}";
            }
            if (FileInfoDto.FromDate.HasValue && FileInfoDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfoDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfoDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
         public async Task<PaginatedResultReact<object>> GetObjectHasDebitHasFileNCCAsync(FileInfoDto FileInfoDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        f.*,
                        d_total.bill AS debit_bill,
                        d_total.bill AS debit_supplier_detail_id,
                        d_total.bill AS debit_purchase_accounting_date,
                        d_total.bill AS debit_purchase_status,
                        d_total.bill AS debit_purchase_note,
                        d_total.cus_bill AS debit_cus_bill,
                        d_total.cus_bill_date AS debit_cus_bill_date,
                        d_total.sup_bill AS debit_sup_bill,
                        d_total.sup_bill_date AS debit_sup_bill_date,
                        d_total.vehicle_number AS debit_vehicle_number,
                        d_total.data AS debit_data,
                        d_total.file_info_id AS debit_file_info_id,
                        d_total.customer_detail_id AS debit_customer_detail_id,
                        d_total.employee_staff_id AS debit_employee_staff_id,
                        d_total.service_id AS debit_service_id,
                        d_total.service_detail AS debit_service_detail,
                        d_total.type AS debit_type,
                        d_total.id AS debit_id,
                        d_total.name AS debit_name,
                        d_total.updated_at AS debit_updated_at,
                        d_total.updated_by AS debit_updated_by,
                        d_total.status AS debit_status,
                        d_total.accounting_date AS debit_accounting_date,
                        CAST(d_total.purchase_price AS INT) AS debit_purchase_price,
                        d_total.purchase_vat AS debit_purchase_vat,
                        CAST(d_total.total_purchase_price AS INT) AS debit_total_purchase_price,
                        CAST(d_total.price AS INT) AS debit_price,
                        d_total.vat AS debit_vat,
                        CAST(d_total.total_price AS INT) AS debit_total_price,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM file_infos f
                    LEFT JOIN confirm_file_infos cf 
                        ON cf.file_info_id = f.id
                        AND cf.partner_detail_id = f.customer_detail_id
                    INNER JOIN partner_details p 
                        ON p.id = f.customer_detail_id
                    INNER JOIN (
                        SELECT 
                            d.data,
                            d.file_info_id,
                            d.customer_detail_id,
                            d.employee_staff_id,
                            d.service_id,
                            d.service_detail,
                            d.type,
                            d.id,
                            d.bill,
                            d.cus_bill,
                            d.cus_bill_date,
                            d.sup_bill,
                            d.sup_bill_date,
                            d.vehicle_number,
                            d.name,
                            d.updated_at,
                            d.updated_by,
                            d.vat,
                            d.purchase_vat,
                            d.status,
                            d.accounting_date,
                            d.supplier_detail_id,
                            d.purchase_accounting_date,
                            d.purchase_status,
                            d.purchase_note,
                            SUM(d.purchase_price) AS purchase_price,
                            SUM(d.purchase_price * (d.purchase_vat / 100.0)) + SUM(d.purchase_price) AS total_purchase_price,
                            SUM(d.price) AS price,
                            SUM(d.price * (d.vat / 100.0)) + SUM(d.price) AS total_price
                        FROM debits d
                        WHERE d.status in (1,2) 
                        AND d.supplier_detail_id IS NOT NULL
                        AND d.purchase_status = 1
                        AND d.deleted_at IS NULL
                        GROUP BY 
                            d.file_info_id,
                            d.customer_detail_id,
                            d.data,
                            d.employee_staff_id,
                            d.service_id,
                            d.service_detail,
                            d.type,
                            d.id,
                            d.bill,
                            d.cus_bill,
                            d.cus_bill_date,
                            d.sup_bill,
                            d.sup_bill_date,
                            d.vehicle_number,
                            d.name,
                            d.updated_at,
                            d.updated_by,
                            d.vat,
                            d.purchase_vat,
                            d.accounting_date,
                            d.status,
                            d.supplier_detail_id,
                            d.purchase_accounting_date,
                            d.purchase_status,
                            d.purchase_note
                    ) AS d_total
                        ON d_total.file_info_id = cf.file_info_id
                        AND d_total.customer_detail_id = cf.partner_detail_id
                        AND d_total.id = cf.debit_id
                    WHERE 
                        p.status = 1
                        AND cf.status in (1,2) 
                        AND cf.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND p.deleted_at IS NULL";
            if (FileInfoDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {FileInfoDto.StorageId}";
            }
            if (FileInfoDto.Id > 0)
            {
                sql += $@" AND f.id = {FileInfoDto.Id}";
            }
            if (FileInfoDto.FromDate.HasValue && FileInfoDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfoDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfoDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d_total.type,f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (FileInfo.StorageId > 0)
                whereEquals["storage_id"] = FileInfo.StorageId;
            if (FileInfo.CustomerDetailId > 0)
                whereEquals["customer_detail_id"] = FileInfo.CustomerDetailId;
            if (FileInfo.FromDate.HasValue && FileInfo.ToDate.HasValue)
                whereDateRange.Add(("accounting_date", FileInfo.FromDate.Value, FileInfo.ToDate.Value));
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "file_infos",
                        new[] { "id", "customer_detail_id", "accounting_date", "storage_id", "file_number", "declaration", "bill", "quantity", "container_code", "sales", "type", "feature", "declaration_quantity", "declaration_type", "business", "occurrence", "note", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at", },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "file_info_details",
                                        Table = "file_info_details",
                                        Columns = new[] { "id","file_id","employee_id","price","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "file_id",
                                        KeyName = "file_id",
                                        IsCollection = true
                                    },
                                    new AdoRelation
                                    {
                                        Name = "receipts",
                                        Table = "receipts",
                                        Columns = new[] { "id", "file_info_id","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "file_info_id",
                                        KeyName = "file_info_id",
                                        IsCollection = true,
                                        SubRelations = new List<AdoRelation>
                                        {
                                            new AdoRelation
                                            {
                                                    Name = "receipt_details",
                                                    Table = "receipt_details",
                                                    Columns = new[] { "id", "receipt_id", "amount","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                                    ParentKey = "id",
                                                    ForeignKey = "receipt_id",
                                                    KeyName = "receipt_id",
                                                    IsCollection = false,
                                            }
                                        }
                                    }
                                },
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
        public async Task<FileInfo> ShowAsync(int id)
        {
            var file = await _context.FileInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (file == null) return null;

            file.FileInfoDetails = await _context.FileInfoDetails
                .AsNoTracking()
                .Where(d => d.FileId == id)
                .ToListAsync();

            return file;
        }
        public async Task<FileInfo> ShowWithDebitAsync(int id)
        {
            var file = await _context.FileInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (file == null) return null;

            file.Debits = await _context.Debits
                .AsNoTracking()
                .Where(d => d.FileInfoId == id && d.ServiceId != 19 && d.ServiceId != 33)
                .OrderBy(d=>d.Type)
                .ToListAsync();
            file.FileInfoDetails = await _context.FileInfoDetails
                .AsNoTracking()
                .Where(d => d.FileId == id)
                .ToListAsync();
            return file;
        }
        public async Task<FileInfo> ShowWithDebitHasNCCAsync(int id)
        {
            var file = await _context.FileInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (file == null) return null;

            file.Debits = await _context.Debits
                .AsNoTracking()
                .Where(d => d.FileInfoId == id && d.SupplierDetailId != null && d.ServiceId != 19 && d.ServiceId != 33)
                .OrderBy(d=>d.Type)
                .ToListAsync();
            file.FileInfoDetails = await _context.FileInfoDetails
                .AsNoTracking()
                .Where(d => d.FileId == id)
                .ToListAsync();
            return file;
        }
        public Task<FileInfo> CreateAsync(FileInfo FileInfo)
        {
            _context.FileInfos.Add(FileInfo);
            _context.SaveChanges();
            return Task.FromResult(FileInfo);
        }

        public Task<FileInfo> UpdateAsync(FileInfo FileInfo)
        {
            _context.FileInfos.Update(FileInfo);
            _context.SaveChanges();
            return Task.FromResult(FileInfo);
        }

        public Task<FileInfo> DeleteSoftAsync(FileInfo FileInfo)
        {
            _context.FileInfos.Update(FileInfo);
            _context.SaveChanges();
            return Task.FromResult(FileInfo);
        }
        public async Task<PaginatedResultReact<object>> GetObjectNotFileGia(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                            f.*
                    FROM file_infos f 
                    LEFT JOIN partner_details p 
                            ON p.id = f.customer_detail_id
                    WHERE 
                        p.status = 1
                        AND f.deleted_at IS NULL
                        AND p.deleted_at IS NULL
                        AND EXISTS (
                                SELECT 1
                                FROM debits d
                                WHERE 
                                    d.file_info_id = f.id
                                    AND d.customer_detail_id = f.customer_detail_id
                                    AND d.status = 0
                                    AND d.deleted_at IS NULL
                            )";
            if (FileInfo.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {FileInfo.StorageId}";
            }
            if (FileInfo.FromDate.HasValue && FileInfo.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfo.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfo.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectHasFileGia(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                    SELECT 
                        f.*,
                        d_total.bill AS debit_bill,
                        d_total.cus_bill AS debit_cus_bill,
                        d_total.cus_bill_date AS debit_cus_bill_date,
                        d_total.sup_bill AS debit_sup_bill,
                        d_total.sup_bill_date AS debit_sup_bill_date,
                        d_total.vehicle_number AS debit_vehicle_number,
                        d_total.data AS debit_data,
                        d_total.file_info_id AS debit_file_info_id,
                        d_total.customer_detail_id AS debit_customer_detail_id,
                        d_total.employee_staff_id AS debit_employee_staff_id,
                        d_total.service_id AS debit_service_id,
                        d_total.service_detail AS debit_service_detail,
                        d_total.type AS debit_type,
                        d_total.id AS debit_id,
                        d_total.name AS debit_name,
                        d_total.updated_at AS debit_updated_at,
                        d_total.updated_by AS debit_updated_by,
                        d_total.status AS debit_status,
                        d_total.accounting_date AS debit_accounting_date,
                        CAST(d_total.purchase_price AS INT) AS debit_purchase_price,
                        d_total.purchase_vat AS debit_purchase_vat,
                        CAST(d_total.total_purchase_price AS INT) AS debit_total_purchase_price,
                        CAST(d_total.price AS INT) AS debit_price,
                        d_total.vat AS debit_vat,
                        CAST(d_total.total_price AS INT) AS debit_total_price,
                        cf.note AS cf_note,
                        cf.status AS cf_status,
                        cf.status_confirm AS cf_status_confirm,
                        cf.updated_at AS cf_updated_at,
                        cf.updated_by AS cf_updated_by
                    FROM file_infos f
                    LEFT JOIN confirm_file_infos cf 
                        ON cf.file_info_id = f.id
                        AND cf.partner_detail_id = f.customer_detail_id
                    INNER JOIN partner_details p 
                        ON p.id = f.customer_detail_id
                    INNER JOIN (
                        SELECT 
                            d.data,
                            d.file_info_id,
                            d.customer_detail_id,
                            d.employee_staff_id,
                            d.service_id,
                            d.service_detail,
                            d.type,
                            d.id,
                            d.bill,
                            d.cus_bill,
                            d.cus_bill_date,
                            d.sup_bill,
                            d.sup_bill_date,
                            d.vehicle_number,
                            d.name,
                            d.updated_at,
                            d.updated_by,
                            d.vat,
                            d.purchase_vat,
                            d.status,
                            d.accounting_date,
                            SUM(d.purchase_price) AS purchase_price,
                            SUM(d.purchase_price * (d.purchase_vat / 100.0)) + SUM(d.purchase_price) AS total_purchase_price,
                            SUM(d.price) AS price,
                            SUM(d.price * (d.vat / 100.0)) + SUM(d.price) AS total_price
                        FROM debits d
                        WHERE d.status in (1,2) 
                        AND d.deleted_at IS NULL
                        GROUP BY 
                            d.file_info_id,
                            d.customer_detail_id,
                            d.data,
                            d.employee_staff_id,
                            d.service_id,
                            d.service_detail,
                            d.type,
                            d.id,
                            d.bill,
                            d.cus_bill,
                            d.cus_bill_date,
                            d.sup_bill,
                            d.sup_bill_date,
                            d.vehicle_number,
                            d.name,
                            d.updated_at,
                            d.updated_by,
                            d.vat,
                            d.purchase_vat,
                            d.accounting_date,
                            d.status
                    ) AS d_total
                        ON d_total.file_info_id = cf.file_info_id
                        AND d_total.customer_detail_id = cf.partner_detail_id
                        AND d_total.id = cf.debit_id
                    WHERE 
                        p.status = 1
                        AND cf.status in (1,2) 
                        AND cf.deleted_at IS NULL
                        AND f.deleted_at IS NULL
                        AND p.deleted_at IS NULL";
            if (FileInfo.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {FileInfo.StorageId}";
            }
            if (FileInfo.Id > 0)
            {
                sql += $@" AND f.id = {FileInfo.Id}";
            }
            if (FileInfo.FromDate.HasValue && FileInfo.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfo.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfo.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY d_total.type,f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
    }
}
