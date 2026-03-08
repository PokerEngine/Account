namespace Application.Storage;

public interface IEmailVerificationTokenStorage
{
    Task<string> GenerateTokenAsync(Guid accountUid);
    Task<Guid> VerifyTokenAsync(string token);
    Task DeleteTokensAsync(Guid accountUid);
}
