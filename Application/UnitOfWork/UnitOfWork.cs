using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Event;

namespace Application.UnitOfWork;

public class UnitOfWork(
    IRepository repository,
    IEventDispatcher eventDispatcher
) : IUnitOfWork
{
    private readonly HashSet<IAggregateRoot> _aggregates = [];

    public void Register(IAggregateRoot aggregate)
    {
        _aggregates.Add(aggregate);
    }

    public async Task CommitAsync()
    {
        var committed = new List<(IAggregateRoot Aggregate, List<IEvent> Events)>();

        foreach (var aggregate in _aggregates)
        {
            var events = aggregate.PullEvents();
            if (events.Count > 0)
                committed.Add((aggregate, events));
        }

        try
        {
            foreach (var (aggregate, events) in committed)
                await repository.AddEventsAsync(aggregate.Uid, events);

            foreach (var (_, events) in committed)
                foreach (var @event in events)
                    await eventDispatcher.DispatchAsync(@event);
        }
        finally
        {
            _aggregates.Clear();
        }
    }
}
