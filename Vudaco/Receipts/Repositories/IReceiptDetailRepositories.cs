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
    public interface IReceiptDetailRepositories : IBaseRepository<ReceiptDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDetailDto ReceiptDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<ReceiptDetail> ShowAsync(int id);
        Task<ReceiptDetail> CreateAsync(ReceiptDetail ReceiptDetail);
        Task<ReceiptDetail> UpdateAsync(ReceiptDetail ReceiptDetail);
        Task<ReceiptDetail> DeleteSoftAsync(ReceiptDetail ReceiptDetail);
    }
}
