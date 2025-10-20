
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Activitys.Dtos;
using Vudaco.Activitys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Activitys.Repositories
{
    public class ActivityRepositories : BaseRepository<Activity>, IActivityRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ActivityRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Activity> CreateAsync(Activity Activity)
        {
            throw new System.NotImplementedException();
        }

        public Task<Activity> DeleteSoftAsync(Activity Activity)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(ActivityDto ActivityDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Activity> ShowAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Activity> UpdateAsync(Activity Activity)
        {
            throw new System.NotImplementedException();
        }
    }
}
