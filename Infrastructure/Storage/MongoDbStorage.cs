using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Infrastructure.Client.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Storage;

public class MongoDbStorage : IStorage
{
    private const string DetailViewCollectionName = "views_detail";
    private const string UniquenessViewCollectionName = "views_uniqueness";
    private readonly IMongoCollection<DetailViewDocument> _detailViewCollection;
    private readonly IMongoCollection<UniquenessViewDocument> _uniquenessViewCollection;

    public MongoDbStorage(MongoDbClient client, IOptions<MongoDbStorageOptions> options)
    {
        var db = client.Client.GetDatabase(options.Value.Database);

        _detailViewCollection = db.GetCollection<DetailViewDocument>(DetailViewCollectionName);
        _uniquenessViewCollection = db.GetCollection<UniquenessViewDocument>(UniquenessViewCollectionName);
        // TODO: add unique indexes
    }

    public async Task<DetailView> GetDetailViewAsync(Guid uid)
    {
        var document = await _detailViewCollection
            .Find(x => x.Uid == (Guid)uid)
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
            BirthDate = document.BirthDate
        };
    }

    public async Task<bool> NicknameExistsAsync(string nickname)
    {
        var document = await _uniquenessViewCollection
            .Find(x => x.Nickname == nickname)
            .FirstOrDefaultAsync();
        return document is not null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var document = await _uniquenessViewCollection
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
            BirthDate = account.BirthDate
        };

        await _detailViewCollection.FindOneAndReplaceAsync(x => x.Uid == (Guid)account.Uid, document, options);
    }
}

public class MongoDbStorageOptions
{
    public const string SectionName = "MongoDbStorage";

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
    public required DateOnly BirthDate { get; init; }
}

public record UniquenessViewDocument
{
    [BsonId]
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
}
