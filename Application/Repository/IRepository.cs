using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    Task<AccountUid> GetNextUidAsync();
    Task<List<IEvent>> GetEventsAsync(AccountUid uid);
    Task AddEventsAsync(AccountUid uid, List<IEvent> events);
}
