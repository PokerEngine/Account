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
    private readonly List<Func<Task>> _commits = [];

    public void Register(Account account) =>
        _commits.Add(() => CommitAsync(
            account,
            events => repository.AddEventsAsync(account.Uid, events)
        ));

    public async Task CommitAsync()
    {
        foreach (var commit in _commits)
            await commit();
        _commits.Clear();
    }

    private async Task CommitAsync(IAggregateRoot aggregate, Func<List<IEvent>, Task> persist)
    {
        var events = aggregate.PullEvents();
        if (events.Count == 0) return;
        await persist(events);
        foreach (var @event in events)
            await eventDispatcher.DispatchAsync(@event);
    }
}
