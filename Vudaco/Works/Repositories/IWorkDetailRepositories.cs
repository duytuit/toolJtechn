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
    public interface IWorkDetailRepositories : IBaseRepository<WorkDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(CheckListDto CheckListDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<WorkDetail> ShowAsync(int id);
        Task<WorkDetail> CreateAsync(WorkDetail WorkDetail);
        Task<WorkDetail> UpdateAsync(WorkDetail WorkDetail);
        Task<WorkDetail> DeleteSoftAsync(WorkDetail WorkDetail);
        Task<List<WorkDetail>> GetByModelId(int id,string model, CancellationToken cancellationToken);
    }
}
