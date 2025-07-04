
using System;
using System.Threading.Tasks;
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Requireds.Repositories
{
    public interface IRequiredRepository : IBaseRepository<Required>
    {
        Task<PaginatedResult<Required>> GetPaginatedAsync(RequestRequiredDto RequestRequiredDto, int page, int pageSize);
        Task<PaginatedResult<Required>> GetTaskAsync(RequestRequiredDto RequestRequiredDto, int page, int pageSize);
        Task<PaginatedResult<object>> GetObjectTaskAsync(RequestRequiredDto RequestRequiredDto, int page, int pageSize);
        Task<Required> CreateRequiredAsync(Required required);
        Task<int> CheckDuplicateTitle(string title, int from_type, DateTime? created_client);
        Task<Required> show(int id);
    }
}
