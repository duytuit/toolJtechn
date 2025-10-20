using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Dtos;
using Vudaco.Bills.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Bills.Repositories
{
    public interface IBillRepositories : IBaseRepository<Bill>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(BillDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Bill> ShowAsync(int id);
        Task<Bill> CreateAsync(Bill Bill);
        Task<Bill> UpdateAsync(Bill Bill);
        Task<Bill> DeleteSoftAsync(Bill Bill);
    }
}
