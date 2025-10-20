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
    public interface IVehicleRepositories : IBaseRepository<Vehicle>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(VehicleDto VehicleDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Vehicle> ShowAsync(int id);
        Task<Vehicle> CreateAsync(Vehicle Vehicle);
        Task<Vehicle> UpdateAsync(Vehicle Vehicle);
        Task<Vehicle> DeleteSoftAsync(Vehicle Vehicle);
    }
}
