using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Storage;
using Application.Test.UnitOfWork;
using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Test.Command;

public class RegisterAccountTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldRegisterAccount()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var command = new RegisterAccountCommand
        {
            Nickname = "Alice",
            Email = "alice.alright@test.com",
            FirstName = "Alice",
            LastName = "Alright",
            BirthDate = new DateOnly(1990, 1, 1)
        };
        var handler = new RegisterAccountHandler(unitOfWork.Repository, unitOfWork);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        var account = Account.FromEvents(response.Uid, await unitOfWork.Repository.GetEventsAsync(response.Uid));
        Assert.Equal(new AccountUid(response.Uid), account.Uid);
        Assert.Equal(new Nickname("Alice"), account.Nickname);
        Assert.Equal(new Email("alice.alright@test.com"), account.Email);
        Assert.Equal(new FirstName("Alice"), account.FirstName);
        Assert.Equal(new LastName("Alright"), account.LastName);
        Assert.Equal(new BirthDate(new DateOnly(1990, 1, 1)), account.BirthDate);

        var detailView = await unitOfWork.Storage.GetDetailViewAsync(account.Uid);
        Assert.Equal((Guid)account.Uid, detailView.Uid);

        var events = await unitOfWork.EventDispatcher.GetDispatchedEvents(response.Uid);
        Assert.Single(events);
        Assert.IsType<AccountRegisteredEvent>(events[0]);
    }

    private StubUnitOfWork CreateUnitOfWork()
    {
        var repository = new StubRepository();
        var storage = new StubStorage();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, storage, eventDispatcher);
    }
}
