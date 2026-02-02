using System.Threading;
using System.Threading.Tasks;
using Vudaco.PayrollPeriods.Dtos;
using Vudaco.PayrollPeriods.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.PayrollPeriods.Repositories
{
    public interface IPayrollPeriodDetailRepositories : IBaseRepository<PayrollPeriodDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(PayrollPeriodDetailDto payrollPeriodDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PayrollPeriodDetail> ShowAsync(int id);
        Task<PayrollPeriodDetail> CreateAsync(PayrollPeriodDetail payrollPeriodDetail);
        Task<PayrollPeriodDetail> UpdateAsync(PayrollPeriodDetail payrollPeriodDetail);
        Task<PayrollPeriodDetail> DeleteSoftAsync(PayrollPeriodDetail payrollPeriodDetail);
    }
}
