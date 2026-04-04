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
    public interface IReceiptRepositories : IBaseRepository<Receipt>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetXacNhanChiPhiGiaoNhanAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetPhiDiDuongCuaLaiXeAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetPhieuThuAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetBaoCaoLuuChuyenTienTeAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetPhieuChiAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetSoQuyAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetSoDuDauKyAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<object> GetSoQuyDKAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Receipt> ShowAsync(int id);
        Task<ReceiptDetail> ShowWithDebitAsync(int id);
        Task<Receipt> CreateAsync(Receipt Receipt);
        Task<Receipt> UpdateAsync(Receipt Receipt);
        Task<Receipt> DeleteSoftAsync(Receipt Receipt);
    }
}
