using Application.Event;
using Application.Repository;
using Application.Storage;
using Domain.Entity;

namespace Application.UnitOfWork;

public class UnitOfWork(
    IRepository repository,
    IStorage storage,
    IEventDispatcher eventDispatcher
) : IUnitOfWork
{
    private readonly HashSet<Account> _accounts = [];

    public void Register(Account account)
    {
        _accounts.Add(account);
    }

    public async Task CommitAsync(bool updateViews = true)
    {
        foreach (var account in _accounts)
        {
            var events = account.PullEvents();

            if (events.Count == 0)
            {
                continue;
            }

            await repository.AddEventsAsync(account.Uid, events);

            if (updateViews)
            {
                await storage.SaveViewAsync(account);
            }

            var context = new EventContext { AccountUid = account.Uid };
            foreach (var @event in events)
            {
                await eventDispatcher.DispatchAsync(@event, context);
            }
        }

        _accounts.Clear();
    }
}
