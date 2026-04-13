using Domain.Event;

namespace Domain.Entity;

public interface IAggregateRoot
{
    Guid Uid { get; }
    List<IEvent> PullEvents();
}
