using Domain.Event;

namespace Application.Repository;

public interface IRepository
{
    Task<Guid> GetNextUidAsync();
    Task<List<IEvent>> GetEventsAsync(Guid uid);
    Task AddEventsAsync(Guid uid, List<IEvent> events);
}
