using Application.IntegrationEvent;
using Application.Service.MessageSender;
using Application.Storage;
using Domain.Event;

namespace Application.Event;

public class AccountRegisteredEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    IEmailVerificationTokenStorage emailVerificationTokenStorage,
    IMessageSender messageSender
) : IEventHandler<AccountRegisteredEvent>
{
    public async Task HandleAsync(AccountRegisteredEvent @event)
    {
        var integrationEvent = new AccountRegisteredIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            AccountUid = @event.AccountUid,
            Nickname = @event.Nickname,
            Email = @event.Email,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            BirthDate = @event.BirthDate.ToString(),
            OccurredAt = @event.OccurredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "account.account-registered");

        var token = await emailVerificationTokenStorage.GenerateTokenAsync(@event.AccountUid);
        var message = new Message
        {
            Header = "Email verification",
            Content = $"[Verify email](/account/verify-email?token={token})" // TODO: implement URL template
        };
        var recipient = new Recipient
        {
            Email = @event.Email
        };
        await messageSender.SendAsync(message, recipient);
    }
}
