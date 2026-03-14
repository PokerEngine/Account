using Domain.ValueObject;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Infrastructure.Client.MongoDb;

public class MongoDbClient
{
    public MongoClient Client;
    public MongoDbClient(IOptions<MongoDbClientOptions> options)
    {
        var url = $"mongodb://{options.Value.Username}:{options.Value.Password}@{options.Value.Host}:{options.Value.Port}";
        Client = new MongoClient(url);

        MongoDbSerializerConfig.Register();
    }
}

public class MongoDbClientOptions
{
    public const string SectionName = "MongoDb";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

internal static class MongoDbSerializerConfig
{
    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.TryRegisterSerializer(new AccountUidSerializer());
        BsonSerializer.TryRegisterSerializer(new NicknameSerializer());
        BsonSerializer.TryRegisterSerializer(new EmailSerializer());
        BsonSerializer.TryRegisterSerializer(new FirstNameSerializer());
        BsonSerializer.TryRegisterSerializer(new LastNameSerializer());
        BsonSerializer.TryRegisterSerializer(new BirthDateSerializer());
    }
}

internal sealed class AccountUidSerializer : SerializerBase<AccountUid>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, AccountUid value)
        => context.Writer.WriteGuid(value);

    public override AccountUid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadGuid();
}

internal sealed class NicknameSerializer : SerializerBase<Nickname>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Nickname value)
        => context.Writer.WriteString(value);

    public override Nickname Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class EmailSerializer : SerializerBase<Email>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Email value)
        => context.Writer.WriteString(value);

    public override Email Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class FirstNameSerializer : SerializerBase<FirstName>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, FirstName value)
        => context.Writer.WriteString(value);

    public override FirstName Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class LastNameSerializer : SerializerBase<LastName>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LastName value)
        => context.Writer.WriteString(value);

    public override LastName Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class BirthDateSerializer : SerializerBase<BirthDate>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BirthDate value)
        => context.Writer.WriteString(value.ToString());

    public override BirthDate Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => BirthDate.FromString(context.Reader.ReadString());
}
