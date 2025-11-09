using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Debits.Repositories
{
    public interface IDebitRepositories : IBaseRepository<Debit>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitDispatchAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitServiceAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Debit> ShowAsync(int id);
        Task<Debit> CreateAsync(Debit Debit);
        Task<Debit> UpdateAsync(Debit Debit);
        Task<Debit> DeleteSoftAsync(Debit Debit);
    }
}
