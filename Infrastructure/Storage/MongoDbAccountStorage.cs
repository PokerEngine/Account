using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Infrastructure.Client.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Storage;

public class MongoDbAccountStorage : IAccountStorage
{
    private const string DetailViewCollectionName = "views_detail";
    private readonly IMongoCollection<DetailViewDocument> _detailViewCollection;

    public MongoDbAccountStorage(MongoDbClient client, IOptions<MongoDbStorageOptions> options)
    {
        var db = client.Client.GetDatabase(options.Value.Database);

        _detailViewCollection = db.GetCollection<DetailViewDocument>(DetailViewCollectionName);
        // TODO: add unique indexes
    }

    public async Task<DetailView> GetDetailViewAsync(Guid uid)
    {
        var document = await _detailViewCollection
            .Find(x => x.Uid == uid)
            .FirstOrDefaultAsync();

        if (document is null)
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return new DetailView
        {
            Uid = document.Uid,
            Nickname = document.Nickname,
            Email = document.Email,
            FirstName = document.FirstName,
            LastName = document.LastName,
            BirthDate = document.BirthDate,
            IsEmailVerified = document.IsEmailVerified
        };
    }

    public async Task<bool> NicknameExistsAsync(string nickname)
    {
        var document = await _detailViewCollection
            .Find(x => x.Nickname == nickname)
            .FirstOrDefaultAsync();
        return document is not null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var document = await _detailViewCollection
            .Find(x => x.Email == email)
            .FirstOrDefaultAsync();
        return document is not null;
    }

    public async Task SaveViewAsync(Account account)
    {
        var options = new FindOneAndReplaceOptions<DetailViewDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var document = new DetailViewDocument
        {
            Uid = account.Uid,
            Nickname = account.Nickname,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            BirthDate = account.BirthDate.ToString(),
            IsEmailVerified = account.IsEmailVerified
        };

        await _detailViewCollection.FindOneAndReplaceAsync(x => x.Uid == (Guid)account.Uid, document, options);
    }
}

public class MongoDbStorageOptions
{
    public const string SectionName = "MongoDbAccountStorage";

    public required string Database { get; init; }
}

public record DetailViewDocument
{
    [BsonId]
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string BirthDate { get; init; }
    public required bool IsEmailVerified { get; init; }
}
