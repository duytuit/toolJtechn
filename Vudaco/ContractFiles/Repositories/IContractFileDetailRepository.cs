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
        Task<FileInfoDetail> ShowAsync(int id);
        Task<FileInfoDetail> CreateAsync(FileInfoDetail FileInfo);
        Task<FileInfoDetail> UpdateAsync(FileInfoDetail FileInfo);
        Task<FileInfoDetail> DeleteSoftAsync(FileInfoDetail FileInfo);
    }
}
