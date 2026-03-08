using Application.Repository;
using Application.Storage;
using Application.UnitOfWork;
using Domain.Entity;

namespace Application.Command;

public record VerifyEmailCommand : ICommand
{
    public required string Token { get; init; }
}

public record VerifyEmailResponse : ICommandResponse;

public class VerifyEmailHandler(
    IRepository repository,
    IEmailVerificationTokenStorage emailVerificationTokenStorage,
    IUnitOfWork unitOfWork
) : ICommandHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    public async Task<VerifyEmailResponse> HandleAsync(VerifyEmailCommand command)
    {
        var accountUid = await emailVerificationTokenStorage.VerifyTokenAsync(command.Token);

        var account = Account.FromEvents(accountUid, await repository.GetEventsAsync(accountUid));

        account.VerifyEmail();

        unitOfWork.Register(account);
        await unitOfWork.CommitAsync();

        await emailVerificationTokenStorage.DeleteTokensAsync(accountUid);

        return new VerifyEmailResponse();
    }
}
