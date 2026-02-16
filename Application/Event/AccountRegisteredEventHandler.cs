using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class AccountRegisteredEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<AccountRegisteredEvent>
{
    public async Task HandleAsync(AccountRegisteredEvent @event, EventContext context)
    {
        var integrationEvent = new AccountRegisteredIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            AccountUid = context.AccountUid,
            Nickname = @event.Nickname,
            Email = @event.Email,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            BirthDate = @event.BirthDate.ToString(),
            OccurredAt = @event.OccurredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "account.account-registered");
    }
}
