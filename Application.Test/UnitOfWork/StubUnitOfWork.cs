using Application.Test.Event;
using Application.Test.Repository;

namespace Application.Test.UnitOfWork;

public class StubUnitOfWork(
    StubAccountRepository accountRepository,
    StubEventDispatcher eventDispatcher
) : Application.UnitOfWork.UnitOfWork(accountRepository, eventDispatcher)
{
    public readonly StubAccountRepository AccountRepository = accountRepository;
    public readonly StubEventDispatcher EventDispatcher = eventDispatcher;
}
