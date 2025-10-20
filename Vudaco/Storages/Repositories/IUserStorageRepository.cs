using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Storages.Dtos;
using Vudaco.Storages.Models;

namespace Vudaco.Storages.Repositories
{
    public interface IUserStorageRepository : IBaseRepository<UserStorage>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(UserStorageDto UserStorageDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<UserStorage> ShowAsync(int id);
        Task<UserStorage> CreateAsync(UserStorage UserStorage);
        Task<UserStorage> UpdateAsync(UserStorage UserStorage);
        Task<UserStorage> DeleteSoftAsync(UserStorage UserStorage);
    }
}
