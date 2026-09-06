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
    public interface ICommentRepositories : IBaseRepository<WorkComment>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkCommentDto WorkCommentDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<WorkComment> ShowAsync(int id);
        Task<WorkComment> CreateAsync(WorkComment WorkComment);
        Task<WorkComment> UpdateAsync(WorkComment WorkComment);
        Task<WorkComment> DeleteSoftAsync(WorkComment WorkComment);
        Task<List<WorkComment>> GetByModelId(int id,string model, CancellationToken cancellationToken);
    }
}
