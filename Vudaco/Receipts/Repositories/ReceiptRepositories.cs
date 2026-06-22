using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.MysqlHelper;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Receipts.Repositories
{
    public class ReceiptRepositories : BaseRepository<Receipt>, IReceiptRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public const int ThuKH = 0;
        public const int ChiGiaoNhan = 1;
        public const int ChiHoanUngGiaoNhan = 2;
        public const int ThuHoanUngGiaoNhan = 3;
        public const int ChuyenTienNoiBo = 10;
        public const int DoiTruCongNo = 11;
        public const int HoanTraTamThuGiaoNhan = 12;
        public const int ChiKhac = 8; // chi khác
        public const int ChiNCC = 7;
        public const int ThuKhac = 9; // thu khác
        public const int ChiMuaHangNCC = 4; // đã dùng
        public const int PhiDiDuongCuaLaiXe = 5; // đã dùng
        public const int ThuBanHangNV = 6; // chưa dùng
        //=============================================
        public const int DoiTuongKH = 0;
        public const int DoiTuongNCC = 1;
        public const int DoiTuongNV = 2;

        public ReceiptRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Receipt> CreateAsync(Receipt Receipt)
        {
            _context.Receipts.Add(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public Task<Receipt> DeleteSoftAsync(Receipt Receipt)
        {
                _context.Receipts.Update(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ReceiptDto.StorageId > 0)
                whereEquals["storage_id"] = ReceiptDto.StorageId;
            if (ReceiptDto.EmployeeId > 0)
                whereEquals["employee_id"] = ReceiptDto.EmployeeId;
                whereEquals["type_receipt"] = ReceiptDto.TypeReceipt;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","fund_id","income_expense_category_id","bill","note","description","form_of_payment","type_receipt","type","bank_id","status","data","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
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
                                        Name = "receipt_details",
                                        Table = "receipt_details",
                                        Columns = new[] { "id","receipt_id","storage_id","debit_id","accounting_date","amount","vat","data","note","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "receipt_id",
                                        KeyName = "receipt_id",
                                        IsCollection = true,
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
        public async Task<PaginatedResultReact<object>> GetPhiDiDuongCuaLaiXeAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ReceiptDto.StorageId > 0)
                whereEquals["storage_id"] = ReceiptDto.StorageId;
            if (ReceiptDto.EmployeeId > 0)
                whereEquals["employee_id"] = ReceiptDto.EmployeeId;

            whereCustoms.Add(("type_receipt IN (5)", Array.Empty<object>()));

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","debit_driver_id","fund_id","income_expense_category_id","bill","note","description","form_of_payment","type_receipt","type","bank_id","status","data","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        whereCustom:whereCustoms,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "receipt_details",
                                        Table = "receipt_details",
                                        Columns = new[] { "id","receipt_id","storage_id","debit_id","accounting_date","amount","vat","data","note","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "receipt_id",
                                        KeyName = "receipt_id",
                                        IsCollection = true,
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
        public async Task<PaginatedResultReact<object>> GetXacNhanChiPhiGiaoNhanAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ReceiptDto.StorageId > 0)
                whereEquals["storage_id"] = ReceiptDto.StorageId;
            if (ReceiptDto.EmployeeId > 0)
                whereEquals["employee_id"] = ReceiptDto.EmployeeId;

            whereCustoms.Add(("type_receipt IN (2,3)", Array.Empty<object>()));

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","fund_id","income_expense_category_id","bill","note","description","form_of_payment","type_receipt","type","bank_id","status","data","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        whereCustom:whereCustoms,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "receipt_details",
                                        Table = "receipt_details",
                                        Columns = new[] { "id","receipt_id","storage_id","debit_id","accounting_date","amount","vat","data","note","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "receipt_id",
                                        KeyName = "receipt_id",
                                        IsCollection = true,
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

        public async Task<Receipt> ShowAsync(int id)
        {
            var entity = await _context.Receipts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return null;

            entity.ReceiptDetails = await _context.ReceiptDetails
                .AsNoTracking()
                .Where(d => d.ReceiptId == id)
                .ToListAsync();

            return entity;
        }

        public Task<Receipt> UpdateAsync(Receipt Receipt)
        {
             _context.Receipts.Update(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public async Task<ReceiptDetail> ShowWithDebitAsync(int id)
        {
            var entity = await _context.ReceiptDetails
                .FirstOrDefaultAsync(x => x.ReceiptId == id);

            if (entity == null) return null;

            entity.Debit = await _context.Debits
                .Where(d => d.Id == entity.DebitId)
                .FirstOrDefaultAsync();

            return entity;
        }
        public async Task<PaginatedResultReact<object>> GetSoQuyAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.purchase_debit_id,r.debit_receivable_id,r.debit_driver_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.type,r.bank_id,r.status,r.created_by,r.updated_by,r.deleted_by,r.deleted_at,r.created_at,r.updated_at,
                        iecat.type AS iecat_type,
                        iecat.name AS iecat_name,
                        d.amount,
                        d.total
                    FROM receipts r
                     LEFT JOIN (
                        SELECT 
                            receipt_id,
                            SUM(amount) AS amount,
                            SUM(amount * (1 + vat / 100.0)) AS total
                        FROM receipt_details
                        WHERE deleted_at IS NULL
                        GROUP BY receipt_id
                    ) d ON d.receipt_id = r.id
                    LEFT JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    WHERE r.type_receipt NOT IN (12) AND r.deleted_at IS NULL AND iecat.deleted_at IS NULL AND (r.status IS NULL OR r.status = 1)";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
            if (ReceiptDto.FormOfPayment > 0)
            {
                sql += $@" AND r.form_of_payment = {ReceiptDto.FormOfPayment}";
            }
            if (ReceiptDto.BankId > 0)
            {
                sql += $@" AND r.bank_id = {ReceiptDto.BankId}";
            }
            if (ReceiptDto.FundId > 0)
            {
                sql += $@" AND r.fund_id = {ReceiptDto.FundId}";
            }
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY r.accounting_date, r.id";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<object> GetSoQuyDKAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                   SELECT
                        SUM(CASE WHEN iecat.type = 0 THEN COALESCE(d.total,0) ELSE 0 END) AS tong_thu,
                        SUM(CASE WHEN iecat.type = 1 THEN COALESCE(d.total,0) ELSE 0 END) AS tong_chi,
                        SUM(
                            CASE
                                WHEN iecat.type = 0 THEN COALESCE(d.total,0)
                                WHEN iecat.type = 1 THEN -COALESCE(d.total,0)
                                ELSE 0
                            END
                        ) AS so_du_dau_ky
                    FROM receipts r
                    LEFT JOIN (
                        SELECT 
                            receipt_id,
                            SUM(amount * (1 + vat / 100.0)) AS total
                        FROM receipt_details
                        WHERE deleted_at IS NULL
                        GROUP BY receipt_id
                    ) d ON d.receipt_id = r.id
                    LEFT JOIN income_expense_categorys iecat
                        ON iecat.id = r.income_expense_category_id
                    WHERE r.type_receipt NOT IN (12)
                      AND r.deleted_at IS NULL
                      AND iecat.deleted_at IS NULL AND (r.status IS NULL OR r.status = 1)";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
            if (ReceiptDto.FormOfPayment > 0)
            {
                sql += $@" AND r.form_of_payment = {ReceiptDto.FormOfPayment}";
            }
            if (ReceiptDto.BankId > 0)
            {
                sql += $@" AND r.bank_id = {ReceiptDto.BankId}";
            }
            if (ReceiptDto.FundId > 0)
            {
                sql += $@" AND r.fund_id = {ReceiptDto.FundId}";
            }
            sql += $@" AND r.accounting_date < '{ReceiptDto.FromDate.Value:yyyy-MM-dd}'";
            return await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
        }
        public async Task<PaginatedResultReact<object>> GetPhieuThuAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.purchase_debit_id,r.debit_receivable_id,r.debit_driver_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.type,r.bank_id,r.status,r.created_by,r.updated_by,r.deleted_by,r.deleted_at,r.created_at,r.updated_at,
                        d.amount,
                        d.total
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
                        AND r.deleted_at IS NULL";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
           
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY r.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetPhieuChiAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT 
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.purchase_debit_id,r.debit_receivable_id,r.debit_driver_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.type,r.bank_id,r.status,r.created_by,r.updated_by,r.deleted_by,r.deleted_at,r.created_at,r.updated_at,
                        d.amount,
                        d.total,
                        db.min_status
                    FROM receipts r
                    LEFT JOIN file_infos f
                        ON f.id = r.file_info_id
                    -- ✅ Lấy MIN(status) từ debits
                    LEFT JOIN (
                        SELECT
                            file_info_id,
                            MIN(status) AS min_status
                        FROM debits
                        WHERE deleted_at IS NULL
                        AND type in (0,2) 
                        GROUP BY file_info_id
                    ) db
                        ON db.file_info_id = r.file_info_id
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
                    ) d 
                        ON d.receipt_id = r.id
                    WHERE 
                        (r.status IS NULL OR r.status = 1)
                        AND iecat.type = 1
                        AND r.deleted_at IS NULL
                        AND f.deleted_at IS NULL";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
           
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY r.updated_at DESC";
            //_ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetChiTietThuChiAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                    SELECT
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.bank_id,r.created_by,r.updated_by,r.created_at,r.updated_at,
                        rd.amount,
                        rd.vat,
                        COALESCE(CAST(rd.amount * (1 + rd.vat / 100.0) AS INT), 0) AS total_amount,
                        iecat.name AS iecat_name,
                        rd.vehicle_id,
                        v.number_code,
                        b.account_number AS bank_account_number
                    FROM receipt_details rd
                    LEFT JOIN receipts r
                        ON rd.receipt_id = r.id
                    LEFT JOIN income_expense_categorys iecat
                        ON r.income_expense_category_id = iecat.id
                    LEFT JOIN banks b
                        ON r.bank_id = b.id
                    LEFT JOIN vehicles v
                        ON rd.vehicle_id = v.id
                    WHERE
                        rd.deleted_at IS NULL
                        AND r.deleted_at IS NULL";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
           
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetSoDuDauKyAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.purchase_debit_id,r.debit_receivable_id,r.debit_driver_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.type,r.bank_id,r.status,r.created_by,r.updated_by,r.deleted_by,r.deleted_at,r.created_at,r.updated_at,
                        d.amount,
                        d.total
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
                        r.income_expense_category_id = 33
                        AND (r.status IS NULL OR r.status = 1)
                        AND iecat.deleted_at IS NULL
                        AND r.deleted_at IS NULL";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
           
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            sql += " ORDER BY r.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

       public async Task<PaginatedResultReact<object>> GetBaoCaoLuuChuyenTienTeAsync(
            ReceiptDto receiptDto,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var result = new PaginatedResultReact<object>();

            string baseSql = @"
                SELECT
                    iecat.id,
                    iecat.name,
                    COALESCE(SUM(d.amount), 0) AS total_amount,
                    COALESCE(SUM(d.total), 0) AS total_with_vat
                FROM receipts r
                LEFT JOIN income_expense_categorys iecat
                    ON iecat.id = r.income_expense_category_id
                LEFT JOIN
                (
                    SELECT
                        receipt_id,
                        SUM(CAST(amount AS DECIMAL(18,2))) AS amount,
                        SUM(
                            CAST(amount AS DECIMAL(18,2))
                            * (1 + CAST(ISNULL(vat,0) AS DECIMAL(18,2)) / 100)
                        ) AS total
                    FROM receipt_details
                    WHERE deleted_at IS NULL
                    GROUP BY receipt_id
                ) d ON d.receipt_id = r.id
                WHERE
                    (r.status IS NULL OR r.status = 1)
                    AND r.deleted_at IS NULL
                    AND iecat.deleted_at IS NULL
                    AND iecat.id IN (
                        1,3,24,25,14,15,10,11,13,
                        21,37,16,17,18,20,19,35,36
                    )";

            if (receiptDto.StorageId > 0)
            {
                baseSql += $" AND r.storage_id = {receiptDto.StorageId}";
            }

            // ==========================
            // Báo cáo trong kỳ
            // ==========================
            var sqlTrongKy = baseSql;

            if (receiptDto.FromDate.HasValue && receiptDto.ToDate.HasValue)
            {
                var toDateNext = receiptDto.ToDate.Value.Date.AddDays(1);

                sqlTrongKy += $@"
                    AND r.accounting_date >= '{receiptDto.FromDate.Value:yyyy-MM-dd}'
                    AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sqlTrongKy += @"
                GROUP BY
                    iecat.id,
                    iecat.name";

            var baoCaoTrongKy = await SqlServerHelpers.ExecuteQuerySqlAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                sqlTrongKy,
                cancellationToken);

            result.Extra["bao_cao_tk_results"] = baoCaoTrongKy;

            // ==========================
            // Báo cáo đầu kỳ
            // ==========================
            var sqlDauKy = baseSql;

            if (receiptDto.FromDate.HasValue)
            {
                sqlDauKy += $@"
                    AND r.accounting_date < '{receiptDto.FromDate.Value:yyyy-MM-dd}'";
            }

            sqlDauKy += @"
                GROUP BY
                    iecat.id,
                    iecat.name";

            var baoCaoDauKy = await SqlServerHelpers.ExecuteQuerySqlAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                sqlDauKy,
                cancellationToken);

            result.Extra["bao_cao_dk_results"] = baoCaoDauKy;

            return result;
        }

        public async Task<PaginatedResultReact<object>> GetUngTienCuaLaiXeAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        r.id,r.code_receipt,r.storage_id,r.partner_detail_id,r.accounting_date,r.object,r.object_id,r.offset_id,r.employee_id,r.file_info_id,r.fund_id,r.purchase_debit_id,r.debit_receivable_id,r.debit_driver_id,r.income_expense_category_id,r.bill,r.note,r.description,r.form_of_payment,r.type_receipt,r.type,r.bank_id,r.status,r.created_by,r.updated_by,r.deleted_by,r.deleted_at,r.created_at,r.updated_at,
                        d.amount,
                        d.total
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
                        r.income_expense_category_id = 10
                        AND (r.status IS NULL OR r.status = 1)
                        AND iecat.deleted_at IS NULL
                        AND r.deleted_at IS NULL";
            if (ReceiptDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {ReceiptDto.StorageId}";
            }
            if (ReceiptDto.EmployeeId > 0)
            {
                sql += $@" AND r.object = 2 AND r.object_id = {ReceiptDto.EmployeeId}";
            }
            if (ReceiptDto.FromDate.HasValue && ReceiptDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = ReceiptDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND r.accounting_date >= '{ReceiptDto.FromDate.Value:yyyy-MM-dd}' 
                AND r.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
    }
}
