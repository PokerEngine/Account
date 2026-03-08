using Application.Command;
using Application.Exception;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Storage;
using Application.Test.UnitOfWork;
using Domain.Entity;
using Domain.Event;

namespace Application.Test.Command;

public class VerifyEmailTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldVerifyEmail()
    {
        // Arrange
        var emailVerificationTokenStorage = new StubEmailVerificationTokenStorage(new TimeSpan(0, 5, 0));
        var unitOfWork = CreateUnitOfWork();
        var accountUid = await RegisterAccountAsync(unitOfWork, "Alice", "alice.alright@test.com", "Alice", "Alright", "2000-01-01");
        var token = await emailVerificationTokenStorage.GenerateTokenAsync(accountUid);

        var command = new VerifyEmailCommand
        {
            Token = token
        };
        var handler = new VerifyEmailHandler(unitOfWork.Repository, emailVerificationTokenStorage, unitOfWork);

        // Act
        await handler.HandleAsync(command);

        // Assert
        var account = Account.FromEvents(accountUid, await unitOfWork.Repository.GetEventsAsync(accountUid));
        Assert.True(account.IsEmailVerified);

        var detailView = await unitOfWork.AccountStorage.GetDetailViewAsync(account.Uid);
        Assert.Equal((Guid)account.Uid, detailView.Uid);

        var events = await unitOfWork.EventDispatcher.GetDispatchedEvents(account.Uid);
        Assert.Single(events);
        Assert.IsType<EmailVerifiedEvent>(events[0]);
    }

    [Fact]
    public async Task HandleAsync_TokenNotFound_ShouldThrowException()
    {
        // Arrange
        var emailVerificationTokenStorage = new StubEmailVerificationTokenStorage(new TimeSpan(0, 5, 0));
        var unitOfWork = CreateUnitOfWork();
        await RegisterAccountAsync(unitOfWork, "Alice", "alice.alright@test.com", "Alice", "Alright", "2000-01-01");

        var command = new VerifyEmailCommand
        {
            Token = "unknown-token"
        };
        var handler = new VerifyEmailHandler(unitOfWork.Repository, emailVerificationTokenStorage, unitOfWork);

        // Act
        var exc = await Assert.ThrowsAsync<WrongEmailVerificationTokenException>(async () =>
        {
            await handler.HandleAsync(command);
        });

        // Assert
        Assert.Equal("The token is not found", exc.Message);
    }

    [Fact]
    public async Task HandleAsync_TokenExpired_ShouldThrowException()
    {
        // Arrange
        var emailVerificationTokenStorage = new StubEmailVerificationTokenStorage(new TimeSpan(0, 0, 0));
        var unitOfWork = CreateUnitOfWork();
        var accountUid = await RegisterAccountAsync(unitOfWork, "Alice", "alice.alright@test.com", "Alice", "Alright", "2000-01-01");
        var token = await emailVerificationTokenStorage.GenerateTokenAsync(accountUid);

        var command = new VerifyEmailCommand
        {
            Token = token
        };
        var handler = new VerifyEmailHandler(unitOfWork.Repository, emailVerificationTokenStorage, unitOfWork);

        // Act
        var exc = await Assert.ThrowsAsync<WrongEmailVerificationTokenException>(async () =>
        {
            await handler.HandleAsync(command);
        });

        // Assert
        Assert.Equal("The token is expired", exc.Message);
    }

    private async Task<Guid> RegisterAccountAsync(
        StubUnitOfWork unitOfWork,
        string nickname,
        string email,
        string firstName,
        string lastName,
        string birthDate
    )
    {
        var handler = new RegisterAccountHandler(unitOfWork.Repository, unitOfWork.AccountStorage, unitOfWork);
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
        var storage = new StubAccountStorage();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, storage, eventDispatcher);
    }
}
