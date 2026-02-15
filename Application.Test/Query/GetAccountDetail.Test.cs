using Application.Command;
using Application.Exception;
using Application.Query;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Storage;
using Application.Test.UnitOfWork;

namespace Application.Test.Query;

public class GetAccountDetailTest
{
    [Fact]
    public async Task HandleAsync_Exists_ShouldReturn()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var accountUid = await RegisterAccountAsync(unitOfWork, "Alice", "alice.alright@test.com", "Alice", "Alright", new DateOnly(2000, 1, 1));

        var query = new GetAccountDetailQuery { Uid = accountUid };
        var handler = new GetAccountDetailHandler(unitOfWork.Storage);

        // Act
        var response = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(accountUid, response.Uid);
        Assert.Equal("Alice", response.Nickname);
        Assert.Equal("alice.alright@test.com", response.Email);
        Assert.Equal("Alice", response.FirstName);
        Assert.Equal("Alright", response.LastName);
        Assert.Equal(new DateOnly(2000, 1, 1), response.BirthDate);
    }

    [Fact]
    public async Task HandleAsync_NotExists_ShouldThrowException()
    {
        // Arrange
        var storage = new StubStorage();

        var query = new GetAccountDetailQuery { Uid = Guid.NewGuid() };
        var handler = new GetAccountDetailHandler(storage);

        // Act
        var exc = await Assert.ThrowsAsync<AccountNotFoundException>(async () =>
        {
            await handler.HandleAsync(query);
        });

        // Assert
        Assert.Equal("The account is not found", exc.Message);
    }

    private async Task<Guid> RegisterAccountAsync(
        StubUnitOfWork unitOfWork,
        string nickname,
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate
    )
    {
        var handler = new RegisterAccountHandler(unitOfWork.Repository, unitOfWork.Storage, unitOfWork);
        var command = new RegisterAccountCommand
        {
            Nickname = nickname,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate
        };
        var response = await handler.HandleAsync(command);
        await unitOfWork.EventDispatcher.ClearDispatchedEvents(response.Uid);
        return response.Uid;
    }

    private StubUnitOfWork CreateUnitOfWork()
    {
        var repository = new StubRepository();
        var storage = new StubStorage();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, storage, eventDispatcher);
    }
}
