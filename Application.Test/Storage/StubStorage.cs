using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Application.Test.Storage;

public class StubStorage : IStorage
{
    private readonly ConcurrentDictionary<AccountUid, DetailView> _detailMapping = new();

    public Task<DetailView> GetDetailViewAsync(Guid accountUid)
    {
        if (!_detailMapping.TryGetValue(accountUid, out var view))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return Task.FromResult(view);
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
            BirthDate = account.BirthDate
        };
        _detailMapping.AddOrUpdate(account.Uid, view, (_, _) => view);
        return Task.CompletedTask;
    }
}
