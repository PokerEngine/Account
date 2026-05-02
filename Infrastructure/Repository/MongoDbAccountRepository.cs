using Application.Exception;
using Application.Repository;
using Domain.Event;
using Infrastructure.Client.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Repository;

public class MongoDbAccountRepository : IAccountRepository
{
    private const string CollectionName = "events";
    private readonly IMongoCollection<EventDocument> _collection;

    public MongoDbAccountRepository(MongoDbClient client, IOptions<MongoDbAccountRepositoryOptions> options)
    {
        var db = client.Client.GetDatabase(options.Value.Database);
        _collection = db.GetCollection<EventDocument>(CollectionName);
    }

    public Task<Guid> GetNextUidAsync()
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public async Task<List<IEvent>> GetEventsAsync(Guid uid)
    {
        var documents = await _collection
            .Find(e => e.AccountUid == uid)
            .SortBy(e => e.Id)
            .ToListAsync();

        var events = new List<IEvent>();

        foreach (var document in documents)
        {
            var type = MongoDbEventTypeResolver.GetType(document.Type);
            var @event = (IEvent)BsonSerializer.Deserialize(document.Data, type);
            events.Add(@event);
        }

        if (events.Count == 0)
        {
            throw new AccountNotFoundException("The account is not found");
        }

        return events;
    }

    public async Task AddEventsAsync(Guid uid, List<IEvent> events)
    {
        var documents = events.Select(e => new EventDocument
        {
            Type = MongoDbEventTypeResolver.GetName(e),
            AccountUid = uid,
            OccurredAt = e.OccurredAt,
            Data = e.ToBsonDocument(e.GetType())
        });

        await _collection.InsertManyAsync(documents);
    }
}

public class MongoDbAccountRepositoryOptions
{
    public const string SectionName = "MongoDbAccountRepository";

    public required string Database { get; init; }
}

internal sealed class EventDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    public required string Type { get; init; }
    public required Guid AccountUid { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required BsonDocument Data { get; init; }
}
