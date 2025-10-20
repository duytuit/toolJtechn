using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Models;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Partners.Repositories
{
    public interface IPartnerRepository : IBaseRepository<Partner>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(PartnerDto PartnerDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Partner> ShowAsync(int id);
        Task<Partner> CreateAsync(Partner Partner);
        Task<Partner> UpdateAsync(Partner Partner);
        Task<Partner> DeleteSoftAsync(Partner Partner);
    }
}
