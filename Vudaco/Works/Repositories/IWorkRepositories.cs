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
    public interface IWorkRepositories : IBaseRepository<Work>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkListDto WorkListDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Work> ShowAsync(int id);
        Task<Work> CreateAsync(Work Work);
        Task<Work> UpdateAsync(Work Work);
        Task<Work> DeleteSoftAsync(Work Work);
    }
}
