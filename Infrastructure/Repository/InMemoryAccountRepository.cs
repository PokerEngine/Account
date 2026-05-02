using Application.Exception;
using Application.Repository;
using Domain.Event;
using System.Collections.Concurrent;

namespace Infrastructure.Repository;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> _mapping = new();

    public Task<Guid> GetNextUidAsync()
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<List<IEvent>> GetEventsAsync(Guid uid)
    {
        if (!_mapping.TryGetValue(uid, out var events))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        List<IEvent> snapshot;
        lock (events)
            snapshot = events.ToList();

        return Task.FromResult(snapshot);
    }

    public Task AddEventsAsync(Guid uid, List<IEvent> events)
    {
        var items = _mapping.GetOrAdd(uid, _ => new List<IEvent>());
        lock (items)
            items.AddRange(events);

        return Task.CompletedTask;
    }
}
