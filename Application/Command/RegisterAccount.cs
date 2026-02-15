using Application.Repository;
using Application.Storage;
using Application.UnitOfWork;
using Domain.Entity;
using Domain.Exception;

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
    IStorage storage,
    IUnitOfWork unitOfWork
) : ICommandHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    public async Task<RegisterAccountResponse> HandleAsync(RegisterAccountCommand command)
    {
        // We throw domain exceptions at the application layer to improve performance
        // because passing a full list of accounts to the domain layer is an overkill

        if (await storage.NicknameExistsAsync(command.Nickname))
        {
            throw new NotUniqueNicknameException("An account with such nickname already exists");
        }

        if (await storage.EmailExistsAsync(command.Email))
        {
            throw new NotUniqueEmailException("An account with such email already exists");
        }

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
