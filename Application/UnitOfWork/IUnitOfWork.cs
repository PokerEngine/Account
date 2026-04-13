using Domain.Entity;

namespace Application.UnitOfWork;

public interface IUnitOfWork
{
    void Register(IAggregateRoot aggregate);
    Task CommitAsync();
}
