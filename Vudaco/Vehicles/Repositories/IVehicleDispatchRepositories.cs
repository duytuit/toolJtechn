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
    public interface IVehicleDispatchRepositories : IBaseRepository<VehicleDispatch>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(VehicleDispatchDto VehicleDispatchDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<VehicleDispatch> ShowAsync(int id);
        Task<VehicleDispatch> CreateAsync(VehicleDispatch VehicleDispatch);
        Task<VehicleDispatch> UpdateAsync(VehicleDispatch VehicleDispatch);
        Task<VehicleDispatch> DeleteSoftAsync(VehicleDispatch VehicleDispatch);
    }
}
