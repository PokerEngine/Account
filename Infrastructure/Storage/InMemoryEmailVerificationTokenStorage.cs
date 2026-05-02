using Application.Exception;
using Application.Storage;
using Domain.ValueObject;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Infrastructure.Storage;

public class InMemoryEmailVerificationTokenStorage : IEmailVerificationTokenStorage
{
    private const int Length = 32;
    private static TimeSpan Ttl = new(1, 0, 0);

    private readonly ConcurrentDictionary<AccountUid, List<EmailVerificationTokenEntry>> _mapping = new();

    public Task<string> GenerateTokenAsync(Guid accountUid)
    {
        var token = GenerateRandomString();
        var now = DateTime.UtcNow;

        var view = new EmailVerificationTokenEntry
        {
            AccountUid = accountUid,
            Token = token,
            GeneratedAt = now,
            ExpiresAt = now + Ttl
        };

        var items = _mapping.GetOrAdd(accountUid, _ => new List<EmailVerificationTokenEntry>());
        lock (items)
            items.Add(view);

        return Task.FromResult(token);
    }

    public Task<Guid> VerifyTokenAsync(string token)
    {
        foreach (var views in _mapping.Values)
        {
            foreach (var view in views)
            {
                if (view.Token == token)
                {
                    if (view.ExpiresAt < DateTime.UtcNow)
                    {
                        throw new WrongEmailVerificationTokenException("The token is expired");
                    }

                    return Task.FromResult(view.AccountUid);
                }
            }
        }

        throw new WrongEmailVerificationTokenException("The token is not found");
    }

    public Task DeleteTokensAsync(Guid accountUid)
    {
        _mapping.TryRemove(accountUid, out _);
        return Task.CompletedTask;
    }

    private string GenerateRandomString()
    {
        var bytes = RandomNumberGenerator.GetBytes(Length / 2);
        return Convert.ToHexStringLower(bytes);
    }
}

internal class EmailVerificationTokenEntry
{
    public required Guid AccountUid { get; init; }
    public required string Token { get; init; }
    public required DateTime GeneratedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
