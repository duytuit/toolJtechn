
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Notifys.Repositories
{
    public interface INotifyRepositories : IBaseRepository<Notify>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(NotifyDto notifyDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Notify> ShowAsync(int id);
        Task<Notify> CreateAsync(Notify notify);
        Task<Notify> UpdateAsync(Notify notify);
        Task<Notify> DeleteSoftAsync(Notify notify);
    }
}
