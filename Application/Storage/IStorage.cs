using Domain.Entity;

namespace Application.Storage;

public interface IStorage
{
    Task<DetailView> GetDetailViewAsync(Guid accountUid);
    Task<bool> NicknameExistsAsync(string nickname);
    Task<bool> EmailExistsAsync(string email);
    Task SaveViewAsync(Account account);
}

public record DetailView
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly BirthDate { get; init; }
}
