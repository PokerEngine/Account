using Domain.Event;

namespace Application.Repository;

public interface IAccountRepository
{
    Task<Guid> GetNextUidAsync();
    Task<List<IEvent>> GetEventsAsync(Guid uid);
    Task AddEventsAsync(Guid uid, List<IEvent> events);
}
