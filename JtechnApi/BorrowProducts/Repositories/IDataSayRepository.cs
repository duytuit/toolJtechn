
using System.Threading;
using System.Threading.Tasks;
using JtechnApi.BorrowProducts.Dtos;
using JtechnApi.BorrowProducts.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.BorrowProducts.Repositories
{
    public interface IDataSayRepository : IBaseRepository<DataSay>
    {
        Task<PaginatedResultVue<object>> GetPaginatedAsync(DataSayDto dto, int page, int pageSize, CancellationToken cancellationToken);
        Task<DataSay> ShowAsync(int id);
        Task<DataSay> CreateAsync(DataSay DataSay);
        Task<DataSay> UpdateAsync(DataSay DataSay);
        Task<DataSay> ChangeStatusAsync(DataSay DataSay);
        Task<DataSay> DeleteSoftAsync(DataSay DataSay);
    }
}
