using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class EmailVerifiedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<EmailVerifiedEvent>
{
    public async Task HandleAsync(EmailVerifiedEvent @event)
    {
        var integrationEvent = new EmailVerifiedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            AccountUid = @event.AccountUid,
            OccurredAt = @event.OccurredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "account.email-verified");
    }
}
