
using System.Threading.Tasks;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.UploadDatas.Models;

namespace JtechnApi.UploadDatas.Repositories
{
    public interface IUploadDataRepository : IBaseRepository<UploadData>
    {
        Task<PaginatedResult<UploadData>> GetPaginatedAsync(int page, int pageSize);
    }
}
