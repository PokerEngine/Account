using Application.Exception;
using Application.Storage;
using Domain.Entity;
using System.Collections.Concurrent;

namespace Application.Test.Storage;

public class StubAccountStorage : IAccountStorage
{
    private readonly ConcurrentDictionary<Guid, DetailView> _detailMapping = new();

    public Task<DetailView> GetDetailViewAsync(Guid uid)
    {
        if (!_detailMapping.TryGetValue(uid, out var view))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return Task.FromResult(view);
    }

    public Task<bool> NicknameExistsAsync(string nickname)
    {
        foreach (var kv in _detailMapping)
        {
            if (kv.Value.Nickname == nickname)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        foreach (var kv in _detailMapping)
        {
            if (kv.Value.Email == email)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task SaveViewAsync(Account account)
    {
        var view = new DetailView
        {
            Uid = account.Uid,
            Nickname = account.Nickname,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            BirthDate = account.BirthDate.ToString(),
            IsEmailVerified = account.IsEmailVerified
        };
        _detailMapping.AddOrUpdate(account.Uid, view, (_, _) => view);
        return Task.CompletedTask;
    }
}
