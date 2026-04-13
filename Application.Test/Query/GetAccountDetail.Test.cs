using Application.Command;
using Application.Exception;
using Application.Query;
using Application.Storage;
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
        var accountStorage = new StubAccountStorage();
        var accountUid = await RegisterAccountAsync(unitOfWork, accountStorage, "Alice", "alice.alright@test.com", "Alice", "Alright", "2000-01-01");

        var query = new GetAccountDetailQuery { Uid = accountUid };
        var handler = new GetAccountDetailHandler(accountStorage);

        // Act
        var response = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(accountUid, response.Uid);
        Assert.Equal("Alice", response.Nickname);
        Assert.Equal("alice.alright@test.com", response.Email);
        Assert.Equal("Alice", response.FirstName);
        Assert.Equal("Alright", response.LastName);
        Assert.Equal("2000-01-01", response.BirthDate);
    }

    [Fact]
    public async Task HandleAsync_NotExists_ShouldThrowException()
    {
        // Arrange
        var accountStorage = new StubAccountStorage();

        var query = new GetAccountDetailQuery { Uid = Guid.NewGuid() };
        var handler = new GetAccountDetailHandler(accountStorage);

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
        StubAccountStorage accountStorage,
        string nickname,
        string email,
        string firstName,
        string lastName,
        string birthDate
    )
    {
        var handler = new RegisterAccountHandler(unitOfWork.Repository, accountStorage, unitOfWork);
        var command = new RegisterAccountCommand
        {
            Nickname = nickname,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate
        };
        var response = await handler.HandleAsync(command);

        await accountStorage.SaveViewAsync(new DetailView
        {
            Uid = response.Uid,
            Nickname = nickname,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            IsEmailVerified = false
        });

        unitOfWork.EventDispatcher.ClearDispatchedEvents();
        return response.Uid;
    }

    private StubUnitOfWork CreateUnitOfWork()
    {
        var repository = new StubRepository();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, eventDispatcher);
    }
}
