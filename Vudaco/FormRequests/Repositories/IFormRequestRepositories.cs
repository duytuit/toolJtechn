
using System.Threading;
using System.Threading.Tasks;
using Vudaco.FormRequests.Dtos;
using Vudaco.FormRequests.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.FormRequests.Repositories
{
    public interface IFormRequestRepositories : IBaseRepository<FormRequest>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(FormRequestDto formRequestDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<FormRequest> ShowAsync(int id);
        Task<FormRequest> CreateAsync(FormRequest formRequest);
        Task<FormRequest> UpdateAsync(FormRequest formRequest);
        Task<FormRequest> DeleteSoftAsync(FormRequest formRequest);
    }
}
