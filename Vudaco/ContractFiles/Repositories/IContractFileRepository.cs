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
    public interface IContractFileRepository : IBaseRepository<FileInfo>
    {
        Task<PaginatedResultReact<object>> GetObjectNoDebitHasFileNCCAsync(FileInfoDto FileInfoDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectHasDebitHasFileNCCAsync(FileInfoDto FileInfoDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectNotFileGia(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectHasFileGia(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(FileInfoDto FileInfo, int page, int pageSize, CancellationToken cancellationToken);
        Task<FileInfo> ShowAsync(int id);
        Task<FileInfo> ShowWithDebitAsync(int id);
        Task<FileInfo> ShowWithDebitHasNCCAsync(int id);
        Task<FileInfo> CreateAsync(FileInfo FileInfo);
        Task<FileInfo> UpdateAsync(FileInfo FileInfo);
        Task<FileInfo> DeleteSoftAsync(FileInfo FileInfo);
    }
}
