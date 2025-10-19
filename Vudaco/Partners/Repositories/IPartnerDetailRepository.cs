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
    public interface IPartnerDetailRepository : IBaseRepository<PartnerDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PartnerDetail> ShowAsync(int id);
        Task<PartnerDetail> CreateAsync(PartnerDetail PartnerDetail);
        Task<PartnerDetail> UpdateAsync(PartnerDetail PartnerDetail);
        Task<PartnerDetail> DeleteSoftAsync(PartnerDetail PartnerDetail);
    }
}
