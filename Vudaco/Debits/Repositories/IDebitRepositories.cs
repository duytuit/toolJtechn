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
        Task<PaginatedResultReact<object>> GetObjectDebitCuocTamThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitTamThuAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitDauKyKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitDauKyNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitMuaBanAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitGiaoNhanAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitLaiXeAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitChiTietKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<List<object>> GetObjectDebitDuNoDKKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectDebitChiTietNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectNoDebitDispatchNoFileKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectHasDebitDispatchNoFileKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectBanHangKHAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectMuaHangNCCAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Debit> ShowAsync(int id);
        Task<Debit> ShowWithFileInfoAsync(int id);
        Task<List<Debit>> ShowByFileIdAsync(int id);
        Task<Debit> CreateAsync(Debit Debit);
        Task<Debit> UpdateAsync(Debit Debit);
        Task<Debit> DeleteSoftAsync(Debit Debit);
    }
}
