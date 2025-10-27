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
    public interface IIncomeExpenseCategoryRepository : IBaseRepository<IncomeExpenseCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(IncomeExpenseCategoryDto IncomeExpenseCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<IncomeExpenseCategory> ShowAsync(int id);
        Task<IncomeExpenseCategory> CreateAsync(IncomeExpenseCategory IncomeExpenseCategory);
        Task<IncomeExpenseCategory> UpdateAsync(IncomeExpenseCategory IncomeExpenseCategory);
        Task<IncomeExpenseCategory> DeleteSoftAsync(IncomeExpenseCategory IncomeExpenseCategory);
    }
}
