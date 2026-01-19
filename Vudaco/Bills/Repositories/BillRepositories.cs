using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Dtos;
using Vudaco.Bills.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Bills.Repositories
{
    public class BillRepositories : BaseRepository<Bill>, IBillRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public BillRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Bill> CreateAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> DeleteSoftAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(BillDto BillDto, int page, int pageSize, CancellationToken cancellationToken)
        {
              var sql = $@"
                    SELECT
                        b.*,
                        ISNULL(db.total_debit, 0) AS total_debit,
                        ISNULL(rb.total_receipt, 0) AS total_receipt
                    FROM bills b
                    -- 🔹 Tổng debit theo bill
                    LEFT JOIN (
                        SELECT
                            d.bill_id,
                            SUM(
                                (d.price + ISNULL(d.price_com, 0))
                                * (d.vat / 100.0)
                                + (d.price + ISNULL(d.price_com, 0))
                            ) AS total_debit
                        FROM debits d
                        LEFT JOIN partner_details p ON p.id = d.customer_detail_id
                        LEFT JOIN file_infos f ON f.id = d.file_info_id
                        WHERE
                            p.status = 1
                            AND d.type IN (0,1,2,3,4,5,6,8)
                            AND (d.status = 2 OR (d.status = 0 AND d.file_info_id IS NULL))
                            AND (
                                d.service_id NOT IN (19,33)
                                OR (d.service_id = 33 AND d.service_status > 2)
                                OR d.service_id IS NULL
                            )
                            AND d.deleted_at IS NULL
                            AND f.deleted_at IS NULL";
                            if (BillDto.StorageId > 0)
                            {
                                sql += $@" AND d.storage_id = {BillDto.StorageId}";
                            }
                        sql += $@" GROUP BY d.bill_id
                    ) db ON db.bill_id = b.id
                    -- 🔹 Tổng receipt theo bill
                    LEFT JOIN (
                        SELECT
                            d.bill_id,
                            SUM(
                                rdt.amount * (rdt.vat / 100.0)
                                + rdt.amount
                            ) AS total_receipt
                        FROM debits d
                        JOIN receipt_details rdt ON rdt.debit_id = d.id
                        JOIN receipts r ON r.id = rdt.receipt_id
                        JOIN income_expense_categorys iecat ON iecat.id = r.income_expense_category_id
                        WHERE
                            iecat.type = 0
                            AND (r.status IS NULL OR r.status = 1)
                            AND r.deleted_at IS NULL
                            AND rdt.deleted_at IS NULL
                            AND d.deleted_at IS NULL";
                            if (BillDto.StorageId > 0)
                            {
                                sql += $@" AND d.storage_id = {BillDto.StorageId}";
                            }
                       sql += $@" GROUP BY d.bill_id
                    ) rb ON rb.bill_id = b.id
                    WHERE b.deleted_at IS NULL";
            if (BillDto.StorageId > 0)
            {
                sql += $@" AND b.storage_id = {BillDto.StorageId}";
            }
            if (BillDto.CustomerDetailId > 0)
            {
                sql += $@" AND b.customer_detail_id = {BillDto.CustomerDetailId}";
            }
            sql += " ORDER BY b.cycle_name";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public Task<Bill> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> UpdateAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }
    }
}
