using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Vehicles.Dtos;
using Vudaco.Vehicles.Models;

namespace Vudaco.Vehicles.Repositories
{
    public class VehicleDispatchRepositories : BaseRepository<VehicleDispatch>, IVehicleDispatchRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public VehicleDispatchRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<VehicleDispatch> CreateAsync(VehicleDispatch VehicleDispatch)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleDispatch> DeleteSoftAsync(VehicleDispatch VehicleDispatch)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(VehicleDispatchDto VehicleDispatchDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleDispatch> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleDispatch> UpdateAsync(VehicleDispatch VehicleDispatch)
        {
            throw new NotImplementedException();
        }
    }
}
