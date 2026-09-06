using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Works.Dtos;
using Vudaco.Works.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Works.Repositories
{
    public interface IHistoryRepositories : IBaseRepository<WorkHistory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkHistoryDto WorkHistoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<WorkHistory> ShowAsync(int id);
        Task<WorkHistory> CreateAsync(WorkHistory WorkHistory);
        Task<WorkHistory> UpdateAsync(WorkHistory WorkHistory);
        Task<WorkHistory> DeleteSoftAsync(WorkHistory WorkHistory);
        Task<List<WorkHistory>> GetByModelId(int id, string model, CancellationToken cancellationToken);
    }
}
