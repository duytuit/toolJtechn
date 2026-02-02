using System.Threading;
using System.Threading.Tasks;
using Vudaco.PayrollPeriods.Dtos;
using Vudaco.PayrollPeriods.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.PayrollPeriods.Repositories
{
    public interface IPayrollPeriodRepositories : IBaseRepository<PayrollPeriod>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(PayrollPeriodDto payrollPeriodDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PayrollPeriod> ShowAsync(int id);
        Task<PayrollPeriod> CreateAsync(PayrollPeriod payrollPeriod);
        Task<PayrollPeriod> UpdateAsync(PayrollPeriod payrollPeriod);
        Task<PayrollPeriod> DeleteSoftAsync(PayrollPeriod payrollPeriod);
    }
}
