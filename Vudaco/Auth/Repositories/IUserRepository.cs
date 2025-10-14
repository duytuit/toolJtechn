
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Auth.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<PaginatedResult<User>> GetPaginatedAsync(UserDto UserDto, int page, int pageSize);
        Task<PaginatedResult<User>> GetTaskAsync(UserDto UserDto, int page, int pageSize);
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(UserDto UserDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<User> CreateRequiredAsync(User user);
        Task<User> UpdateRequiredAsync(User user);
        Task<int> CheckDuplicateTitle(string title, int from_type, DateTime? created_client);
        Task<User> show(int id);
        Task<object> detail(int id);
        Task<bool> DeleteRequiredAsync(int id);
    }
}
