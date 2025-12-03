using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Repositories
{
    public interface IOffsetRepositories : IBaseRepository<Offset>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(OffsetDto OffsetDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Offset> ShowAsync(int id);
        Task<Offset> CreateAsync(Offset Offset);
        Task<Offset> UpdateAsync(Offset Offset);
        Task<Offset> DeleteSoftAsync(Offset Offset);
    }
}
