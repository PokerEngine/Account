using Domain.Event;
using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Entity;

public class Account : IAggregateRoot
{
    Guid IAggregateRoot.Uid => Uid;
    public AccountUid Uid { get; }
    public Nickname Nickname { get; private set; }
    public Email Email { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public BirthDate BirthDate { get; private set; }
    public bool IsEmailVerified { get; private set; }

    private readonly List<IEvent> _events;

    private Account(
        AccountUid uid,
        Nickname nickname,
        Email email,
        FirstName firstName,
        LastName lastName,
        BirthDate birthDate
    )
    {
        Uid = uid;
        Nickname = nickname;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        IsEmailVerified = false;
        _events = [];
    }

    public static Account FromScratch(
        AccountUid uid,
        Nickname nickname,
        Email email,
        FirstName firstName,
        LastName lastName,
        BirthDate birthDate
    )
    {
        var account = new Account(
            uid: uid,
            nickname: nickname,
            email: email,
            firstName: firstName,
            lastName: lastName,
            birthDate: birthDate
        );

        var @event = new AccountRegisteredEvent
        {
            AccountUid = uid,
            Nickname = nickname,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            OccurredAt = DateTime.UtcNow
        };
        account.AddEvent(@event);

        return account;
    }

    public static Account FromEvents(AccountUid uid, List<IEvent> events)
    {
        if (events.Count == 0 || events[0] is not AccountRegisteredEvent)
        {
            throw new InvalidAccountStateException("The first event must be a AccountRegisteredEvent");
        }

        var registeredEvent = (AccountRegisteredEvent)events[0];
        var account = new Account(
            uid: uid,
            nickname: registeredEvent.Nickname,
            email: registeredEvent.Email,
            firstName: registeredEvent.FirstName,
            lastName: registeredEvent.LastName,
            birthDate: registeredEvent.BirthDate
        );

        foreach (var @event in events[1..])
        {
            switch (@event)
            {
                case EmailVerifiedEvent:
                    account.VerifyEmail();
                    break;
                default:
                    throw new InvalidAccountStateException($"{@event.GetType().Name} is not supported");
            }
        }

        account.PullEvents();

        return account;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
        {
            throw new EmailVerifiedException("The account email is already verified");
        }

        IsEmailVerified = true;

        var @event = new EmailVerifiedEvent
        {
            AccountUid = Uid,
            OccurredAt = DateTime.UtcNow
        };
        AddEvent(@event);
    }

    # region Events

    public List<IEvent> PullEvents()
    {
        var events = _events.ToList();
        _events.Clear();

        return events;
    }

    private void AddEvent(IEvent @event)
    {
        _events.Add(@event);
    }

    # endregion
}
