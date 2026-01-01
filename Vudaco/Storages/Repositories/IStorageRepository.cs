using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Storages.Dtos;
using Vudaco.Storages.Models;

namespace Vudaco.Storages.Repositories
{
    public interface IStorageRepository : IBaseRepository<Storage>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(StorageDto StorageDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetByUserIdAsync(StorageDto StorageDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Storage> ShowAsync(int id);
        Task<Storage> CreateAsync(Storage Storage);
        Task<Storage> UpdateAsync(Storage Storage);
        Task<Storage> DeleteSoftAsync(Storage Storage);
    }
}
