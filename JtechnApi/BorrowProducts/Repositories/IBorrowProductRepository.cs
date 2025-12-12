
using System.Threading;
using System.Threading.Tasks;
using JtechnApi.BorrowProducts.Dtos;
using JtechnApi.BorrowProducts.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.BorrowProducts.Repositories
{
    public interface IBorrowProductRepository : IBaseRepository<BorrowProduct>
    {
        Task<PaginatedResultVue<object>> GetPaginatedAsync(BorrowProductDto dto, int page, int pageSize, CancellationToken cancellationToken);
        Task<BorrowProduct> ShowAsync(int id);
        Task<BorrowProduct> CreateAsync(BorrowProduct BorrowProduct);
        Task<BorrowProduct> UpdateAsync(BorrowProduct BorrowProduct);
        Task<BorrowProduct> ChangeStatusAsync(BorrowProduct BorrowProduct);
        Task<BorrowProduct> DeleteSoftAsync(BorrowProduct BorrowProduct);
    }
}
