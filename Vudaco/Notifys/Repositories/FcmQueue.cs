using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Vudaco.Notifys.Dtos;

namespace Vudaco.Notifys.Repositories
{
    public class FcmQueue : IFcmQueue
    {
        private readonly Channel<FcmJobDto> _queue;

        public FcmQueue()
        {
            _queue = Channel.CreateUnbounded<FcmJobDto>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(FcmJobDto job)
        {
            await _queue.Writer.WriteAsync(job);
        }

        public async ValueTask<FcmJobDto> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
