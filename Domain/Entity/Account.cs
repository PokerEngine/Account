using Domain.Event;
using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Entity;

public class Account
{
    public AccountUid Uid { get; }
    public Nickname Nickname { get; }
    public Email Email { get; }
    public FirstName FirstName { get; }
    public LastName LastName { get; }
    public BirthDate BirthDate { get; }

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
            Nickname = nickname,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            OccurredAt = DateTime.Now
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
                default:
                    throw new InvalidAccountStateException($"{@event.GetType().Name} is not supported");
            }
        }

        account.PullEvents();

        return account;
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
