using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    DateTime OccurredAt { init; get; }
}

public sealed record AccountRegisteredEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Email Email { get; init; }
    public required FirstName FirstName { get; init; }
    public required LastName LastName { get; init; }
    public required BirthDate BirthDate { get; init; }
}
