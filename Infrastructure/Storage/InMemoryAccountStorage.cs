using Application.Exception;
using Application.Storage;
using System.Collections.Concurrent;

namespace Infrastructure.Storage;

public class InMemoryAccountStorage : IAccountStorage
{
    private readonly ConcurrentDictionary<Guid, DetailView> _detailMapping = new();

    public Task<bool> NicknameExistsAsync(string nickname)
    {
        return Task.FromResult(_detailMapping.Values.Any(view => view.Nickname == nickname));
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return Task.FromResult(_detailMapping.Values.Any(view => view.Email == email));
    }

    public Task<DetailView> GetDetailViewAsync(Guid uid)
    {
        if (!_detailMapping.TryGetValue(uid, out var view))
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return Task.FromResult(view);
    }

    public Task SaveViewAsync(DetailView view)
    {
        _detailMapping.AddOrUpdate(view.Uid, view, (_, _) => view);
        return Task.CompletedTask;
    }

    public Task MarkEmailVerifiedAsync(Guid uid)
    {
        if (_detailMapping.TryGetValue(uid, out var view))
        {
            var updated = view with { IsEmailVerified = true };
            _detailMapping.AddOrUpdate(uid, updated, (_, _) => updated);
        }

        return Task.CompletedTask;
    }
}
