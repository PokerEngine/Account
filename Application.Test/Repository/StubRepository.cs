using Application.Exception;
using Application.Repository;
using Domain.Event;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Application.Test.Repository;

public class StubRepository : IRepository
{
    private readonly ConcurrentDictionary<AccountUid, List<IEvent>> _mapping = new();

    public Task<AccountUid> GetNextUidAsync()
    {
        return Task.FromResult(new AccountUid(Guid.NewGuid()));
    }

    public Task<List<IEvent>> GetEventsAsync(AccountUid accountUid)
    {
        if (!_mapping.TryGetValue(accountUid, out var events))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        List<IEvent> snapshot;
        lock (events)
            snapshot = events.ToList();

        return Task.FromResult(snapshot);
    }

    public Task AddEventsAsync(AccountUid accountUid, List<IEvent> events)
    {
        var items = _mapping.GetOrAdd(accountUid, _ => new List<IEvent>());
        lock (items)
            items.AddRange(events);

        return Task.CompletedTask;
    }
}
