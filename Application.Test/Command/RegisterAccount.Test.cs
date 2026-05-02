using Application.Command;
using Application.Storage;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Storage;
using Application.Test.UnitOfWork;
using Domain.Entity;
using Domain.Event;
using Domain.Exception;
using Domain.ValueObject;

namespace Application.Test.Command;

public class RegisterAccountTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldRegisterAccount()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var accountStorage = new StubAccountStorage();
        var command = new RegisterAccountCommand
        {
            Nickname = "Alice",
            Email = "alice.alright@test.com",
            FirstName = "Alice",
            LastName = "Alright",
            BirthDate = "2000-01-01"
        };
        var handler = new RegisterAccountHandler(unitOfWork.AccountRepository, accountStorage, unitOfWork);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        var account = Account.FromEvents(response.Uid, await unitOfWork.AccountRepository.GetEventsAsync(response.Uid));
        Assert.Equal(new AccountUid(response.Uid), account.Uid);
        Assert.Equal(new Nickname("Alice"), account.Nickname);
        Assert.Equal(new Email("alice.alright@test.com"), account.Email);
        Assert.Equal(new FirstName("Alice"), account.FirstName);
        Assert.Equal(new LastName("Alright"), account.LastName);
        Assert.Equal(BirthDate.FromString("2000-01-01"), account.BirthDate);

        var events = unitOfWork.EventDispatcher.GetDispatchedEvents();
        Assert.Single(events);
        Assert.IsType<AccountRegisteredEvent>(events[0]);
    }

    [Fact]
    public async Task HandleAsync_NotUniqueNickname_ShouldThrowException()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var accountStorage = new StubAccountStorage();
        await RegisterAccountAsync(unitOfWork, accountStorage, "Alice", "alice.adams@test.com", "Alice", "Adams", "2000-01-01");

        var command = new RegisterAccountCommand
        {
            Nickname = "Alice",
            Email = "alice.alright@test.com",
            FirstName = "Alice",
            LastName = "Alright",
            BirthDate = "2000-01-01"
        };
        var handler = new RegisterAccountHandler(unitOfWork.AccountRepository, accountStorage, unitOfWork);

        // Act
        var exc = await Assert.ThrowsAsync<NotUniqueNicknameException>(async () =>
        {
            await handler.HandleAsync(command);
        });

        // Assert
        Assert.Equal("An account with such nickname already exists", exc.Message);
    }

    [Fact]
    public async Task HandleAsync_NotUniqueEmail_ShouldThrowException()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var accountStorage = new StubAccountStorage();
        await RegisterAccountAsync(unitOfWork, accountStorage, "Alicia", "alice.alright@test.com", "Alicia", "Alright", "2000-01-01");

        var command = new RegisterAccountCommand
        {
            Nickname = "Alice",
            Email = "alice.alright@test.com",
            FirstName = "Alice",
            LastName = "Alright",
            BirthDate = "2000-01-01"
        };
        var handler = new RegisterAccountHandler(unitOfWork.AccountRepository, accountStorage, unitOfWork);

        // Act
        var exc = await Assert.ThrowsAsync<NotUniqueEmailException>(async () =>
        {
            await handler.HandleAsync(command);
        });

        // Assert
        Assert.Equal("An account with such email already exists", exc.Message);
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
        var handler = new RegisterAccountHandler(unitOfWork.AccountRepository, accountStorage, unitOfWork);
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
        var repository = new StubAccountRepository();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, eventDispatcher);
    }
}
