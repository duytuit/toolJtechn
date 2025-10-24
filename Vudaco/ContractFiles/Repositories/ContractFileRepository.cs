

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
        public ContractFileRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (FileInfo.StorageId > 0)
                whereEquals["storage_id"] = FileInfo.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "file_infos",
                        new[] { "id", "partner_detail_id", "accounting_date", "storage_id", "file_number", "declaration", "bill", "quantity", "container_code", "sales", "type", "feature", "declaration_quantity", "declaration_type", "business", "occurrence", "note", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at", },
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
      
    }
}
