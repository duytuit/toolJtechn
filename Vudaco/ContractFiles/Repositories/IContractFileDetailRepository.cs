using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.ContractFiles.Dtos;
using Vudaco.ContractFiles.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.ContractFiles.Repositories
{
    public interface IContractFileDetailRepository : IBaseRepository<FileInfoDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectNotNangHaAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectHasNangHaAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectHasDebitServiceAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectNotServiceAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectFileNotDispatchAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectFileHasDispatchAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<FileInfoDetail> ShowAsync(int id);
        Task<FileInfoDetail> CreateAsync(FileInfoDetail FileInfoDetail);
        Task<FileInfoDetail> UpdateAsync(FileInfoDetail FileInfoDetail);
        Task<FileInfoDetail> DeleteSoftAsync(FileInfoDetail FileInfoDetail);
    }
}
