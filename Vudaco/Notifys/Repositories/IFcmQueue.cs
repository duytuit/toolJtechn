
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Notifys.Dtos;

namespace Vudaco.Notifys.Repositories
{
    public interface IFcmQueue
    {
        ValueTask EnqueueAsync(FcmJobDto job);
        ValueTask<FcmJobDto> DequeueAsync(CancellationToken cancellationToken);
    }
}
