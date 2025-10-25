using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares;
using Vudaco.Shares.SqlServerHelper;
using Vudaco.Shares.BaseRepository;
using Vudaco.Vehicles.Dtos;
using Vudaco.Vehicles.Models;
using Microsoft.EntityFrameworkCore;

namespace Vudaco.Vehicles.Repositories
{
    public class VehicleRepositories : BaseRepository<Vehicle>, IVehicleRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public VehicleRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Vehicle> CreateAsync(Vehicle Vehicle)
        {
              _context.Vehicles.Add(Vehicle);
            _context.SaveChanges();
            return Task.FromResult(Vehicle);
        }

        public Task<Vehicle> DeleteSoftAsync(Vehicle Vehicle)
        {
              _context.Vehicles.Update(Vehicle);
            _context.SaveChanges();
            return Task.FromResult(Vehicle);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(VehicleDto VehicleDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (VehicleDto.StorageId > 0)
                whereEquals["storage_id"] = VehicleDto.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "vehicles",
                        new[] { "id", "number_code", "is_external_driver", "storage_id", "note", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at"},
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
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

        public Task<Vehicle> ShowAsync(int id)
        {
           return _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Vehicle> UpdateAsync(Vehicle Vehicle)
        {
            _context.Vehicles.Update(Vehicle);
            _context.SaveChanges();
            return Task.FromResult(Vehicle);
        }
    }
}
