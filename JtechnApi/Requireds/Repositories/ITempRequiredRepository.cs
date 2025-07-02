
using System.Threading.Tasks;
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Requireds.Repositories
{
    public interface ITempRequiredRepository : IBaseRepository<TempRequired>
    {
        Task<PaginatedResult<TempRequired>> GetPaginatedAsync(RequestRequiredDto RequestRequiredDto,int page, int pageSize);
        // get temp required by id
        Task<TempRequired> GetTempRequiredByIdAsync(int id);     
        // create temp required
        Task<TempRequired> CreateTempRequiredAsync(TempRequired tempRequired);
        // update temp required
        Task<TempRequired> UpdateTempRequiredAsync(TempRequired tempRequired);
        // delete temp required 
        Task<bool> DeleteTempRequiredAsync(int id);

    }
}
