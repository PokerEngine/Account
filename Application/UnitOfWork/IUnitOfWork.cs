using Domain.Entity;

namespace Application.UnitOfWork;

public interface IUnitOfWork
{
    void Register(Account account);
    Task CommitAsync(bool updateViews = true);
}
