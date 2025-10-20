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
    public interface IBankRepository : IBaseRepository<Bank>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(BankDto BankDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Bank> ShowAsync(int id);
        Task<Bank> CreateAsync(Bank Bank);
        Task<Bank> UpdateAsync(Bank Bank);
        Task<Bank> DeleteSoftAsync(Bank Bank);
    }
}
