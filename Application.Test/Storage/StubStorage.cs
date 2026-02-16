using Application.Exception;
using Application.Storage;
using Domain.Entity;
using System.Collections.Concurrent;

namespace Application.Test.Storage;

public class StubStorage : IStorage
{
    private readonly ConcurrentDictionary<Guid, DetailView> _detailMapping = new();
    private readonly ConcurrentDictionary<Guid, string> _nicknameUniquenessMapping = new();
    private readonly ConcurrentDictionary<Guid, string> _emailUniquenessMapping = new();

    public Task<DetailView> GetDetailViewAsync(Guid accountUid)
    {
        if (!_detailMapping.TryGetValue(accountUid, out var view))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return Task.FromResult(view);
    }

    public Task<bool> NicknameExistsAsync(string nickname)
    {
        foreach (var kv in _nicknameUniquenessMapping)
        {
            if (kv.Value == nickname)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        foreach (var kv in _emailUniquenessMapping)
        {
            if (kv.Value == email)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task SaveViewAsync(Account account)
    {
        SaveDetailView(account);
        SaveUniquenessView(account);
        return Task.CompletedTask;
    }

    private void SaveDetailView(Account account)
    {
        var view = new DetailView
        {
            Uid = account.Uid,
            Nickname = account.Nickname,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            BirthDate = account.BirthDate.ToString()
        };
        _detailMapping.AddOrUpdate(account.Uid, view, (_, _) => view);
    }

    private void SaveUniquenessView(Account account)
    {
        _nicknameUniquenessMapping.AddOrUpdate(account.Uid, account.Nickname, (_, _) => account.Nickname);
        _emailUniquenessMapping.AddOrUpdate(account.Uid, account.Email, (_, _) => account.Email);
    }
}
