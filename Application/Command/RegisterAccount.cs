using Application.Repository;
using Application.UnitOfWork;
using Domain.Entity;

namespace Application.Command;

public record RegisterAccountCommand : ICommand
{
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly BirthDate { get; init; }
}

public record RegisterAccountResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
}

public class RegisterAccountHandler(
    IRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    public async Task<RegisterAccountResponse> HandleAsync(RegisterAccountCommand command)
    {
        var account = Account.FromScratch(
            uid: await repository.GetNextUidAsync(),
            nickname: command.Nickname,
            email: command.Email,
            firstName: command.FirstName,
            lastName: command.LastName,
            birthDate: command.BirthDate
        );

        unitOfWork.Register(account);
        await unitOfWork.CommitAsync();

        return new RegisterAccountResponse
        {
            Uid = account.Uid
        };
    }
}
