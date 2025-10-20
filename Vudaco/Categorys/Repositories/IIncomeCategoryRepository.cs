using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Repositories
{
    public interface IIncomeCategoryRepository : IBaseRepository<IncomeCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(IncomeCategoryDto IncomeCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<IncomeCategory> ShowAsync(int id);
        Task<IncomeCategory> CreateAsync(IncomeCategory IncomeCategory);
        Task<IncomeCategory> UpdateAsync(IncomeCategory IncomeCategory);
        Task<IncomeCategory> DeleteSoftAsync(IncomeCategory IncomeCategory);
    }
}
