using System.Threading;
using System.Threading.Tasks;
using JustSaying.Models;

namespace SolidCqrsFramework.EventManagement.Publishing
{
    public interface ISnsMessagePublisher
    {
        Task PublishAsync(Message message, CancellationToken cancellationToken);
        Task PublishAsync(Message message, PublishMetadata metadata, CancellationToken cancellationToken);
    }
}

