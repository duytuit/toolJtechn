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
            throw new NotImplementedException();
        }

        public Task<Vehicle> DeleteSoftAsync(Vehicle Vehicle)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(VehicleDto VehicleDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Vehicle> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Vehicle> UpdateAsync(Vehicle Vehicle)
        {
            throw new NotImplementedException();
        }
    }
}
