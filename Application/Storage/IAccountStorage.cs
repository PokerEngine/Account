namespace Application.Storage;

public interface IAccountStorage
{
    Task<bool> NicknameExistsAsync(string nickname);
    Task<bool> EmailExistsAsync(string email);
    Task<DetailView> GetDetailViewAsync(Guid uid);
    Task SaveViewAsync(DetailView view);
    Task MarkEmailVerifiedAsync(Guid uid);
}

public record DetailView
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string BirthDate { get; init; }
    public required bool IsEmailVerified { get; init; }
}
