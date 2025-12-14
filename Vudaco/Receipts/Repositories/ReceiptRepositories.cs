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
        public const int ChiNoiBo = 8; // chi khách hàng
        public const int ChiNCC = 7;
        public const int ChiNV = 9;
        public const int ChiMuaHangNCC = 4; // chưa dùng
        public const int ThuBanHangKH = 5; // chưa dùng
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
                        r.*,
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
                        AND iecat.deleted_at IS NULL
                    WHERE r.type_receipt NOT IN (12) AND r.deleted_at IS NULL";
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
                        AND iecat.deleted_at IS NULL
                    WHERE r.type_receipt NOT IN (12)
                      AND r.deleted_at IS NULL";
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
                        r.*,
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
                    WHERE 
                        r.type_receipt IN (0, 3)
                        AND r.status IS NULL
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
                        r.*,
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
                    WHERE 
                        r.type_receipt IN (1,2,7,8)
                        AND r.status IS NULL
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
    }
}
