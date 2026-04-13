using Application.IntegrationEvent;
using Application.Storage;
using Domain.Event;

namespace Application.Event;

public class EmailVerifiedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    IAccountStorage accountStorage
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

        await accountStorage.MarkEmailVerifiedAsync(@event.AccountUid);
    }
}
