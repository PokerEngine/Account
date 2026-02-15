using Domain.Entity;
using Domain.Event;
using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class AccountTest
{
    [Fact]
    public void FromScratch_WhenValid_ShouldConstruct()
    {
        // Arrange
        var uid = new AccountUid(Guid.NewGuid());
        var nickname = new Nickname("Alice");
        var email = new Email("alice.alright@test.com");
        var firstName = new FirstName("Alice");
        var lastName = new LastName("Alright");
        var birthDate = new BirthDate(new DateOnly(2000, 1, 1));

        // Act
        var account = Account.FromScratch(
            uid: uid,
            nickname: nickname,
            email: email,
            firstName: firstName,
            lastName: lastName,
            birthDate: birthDate
        );

        // Assert
        Assert.Equal(uid, account.Uid);
        Assert.Equal(nickname, account.Nickname);
        Assert.Equal(email, account.Email);
        Assert.Equal(firstName, account.FirstName);
        Assert.Equal(lastName, account.LastName);
        Assert.Equal(birthDate, account.BirthDate);

        var pulledEvents = account.PullEvents();
        Assert.Single(pulledEvents);
        var @event = Assert.IsType<AccountRegisteredEvent>(pulledEvents[0]);
        Assert.Equal(new Nickname("Alice"), @event.Nickname);
        Assert.Equal(new Email("alice.alright@test.com"), @event.Email);
        Assert.Equal(new FirstName("Alice"), @event.FirstName);
        Assert.Equal(new LastName("Alright"), @event.LastName);
        Assert.Equal(new BirthDate(new DateOnly(2000, 1, 1)), @event.BirthDate);
    }

    [Fact]
    public void FromEvents_WhenValid_ShouldConstruct()
    {
        // Arrange
        var uid = new AccountUid(Guid.NewGuid());
        var nickname = new Nickname("Alice");
        var email = new Email("alice.alright@test.com");
        var firstName = new FirstName("Alice");
        var lastName = new LastName("Alright");
        var birthDate = new BirthDate(new DateOnly(2000, 1, 1));
        var events = new List<IEvent>
        {
            new AccountRegisteredEvent
            {
                Nickname = nickname,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                OccurredAt = new DateTime(2025, 1, 1)
            }
        };

        // Act
        var account = Account.FromEvents(uid, events);

        // Assert
        Assert.Equal(uid, account.Uid);
        Assert.Equal(nickname, account.Nickname);
        Assert.Equal(email, account.Email);
        Assert.Equal(firstName, account.FirstName);
        Assert.Equal(lastName, account.LastName);
        Assert.Equal(birthDate, account.BirthDate);

        var pulledEvents = account.PullEvents();
        Assert.Empty(pulledEvents);
    }

    [Fact]
    public void FromEvents_WhenRegisterDuplicated_ShouldThrowException()
    {
        // Arrange
        var uid = new AccountUid(Guid.NewGuid());
        var events = new List<IEvent>
        {
            new AccountRegisteredEvent
            {
                Nickname = new Nickname("Alice"),
                Email =  new Email("alice.alright@test.com"),
                FirstName = new FirstName("Alice"),
                LastName = new LastName("Alright"),
                BirthDate = new BirthDate(new DateOnly(2000, 1, 1)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new AccountRegisteredEvent
            {
                Nickname = new Nickname("Alice"),
                Email =  new Email("alice.alright@test.com"),
                FirstName = new FirstName("Alice"),
                LastName = new LastName("Alright"),
                BirthDate = new BirthDate(new DateOnly(2000, 1, 1)),
                OccurredAt = new DateTime(2025, 1, 1)
            }
        };

        // Act
        var exc = Assert.Throws<InvalidAccountStateException>(() => Account.FromEvents(uid, events));

        // Assert
        Assert.Equal("AccountRegisteredEvent is not supported", exc.Message);
    }
}
