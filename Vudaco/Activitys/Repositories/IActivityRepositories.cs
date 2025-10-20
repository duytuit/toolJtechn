
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Activitys.Dtos;
using Vudaco.Activitys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Activitys.Repositories
{
    public interface IActivityRepositories : IBaseRepository<Activity>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(ActivityDto ActivityDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Activity> ShowAsync(int id);
        Task<Activity> CreateAsync(Activity Activity);
        Task<Activity> UpdateAsync(Activity Activity);
        Task<Activity> DeleteSoftAsync(Activity Activity);
    }
}
