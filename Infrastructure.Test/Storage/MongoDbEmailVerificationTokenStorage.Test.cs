using Application.Exception;
using Application.Storage;
using Infrastructure.Storage;
using Infrastructure.Test.Client.MongoDb;
using Microsoft.Extensions.Options;

namespace Infrastructure.Test.Storage;

[Trait("Category", "Integration")]
public class MongoDbEmailVerificationTokenStorageTest(MongoDbClientFixture fixture) : IClassFixture<MongoDbClientFixture>
{
    [Fact]
    public async Task GenerateTokenAsync_Valid_ShouldReturn()
    {
        // Arrange
        var storage = CreateStorage();
        var accountUid = Guid.NewGuid();

        // Act
        var token = await storage.GenerateTokenAsync(accountUid);

        // Assert
        Assert.Equal(32, token.Length);
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenExists_ShouldReturn()
    {
        // Arrange
        var storage = CreateStorage();
        var accountUid = Guid.NewGuid();
        var token = await storage.GenerateTokenAsync(accountUid);

        // Act
        var uid = await storage.VerifyTokenAsync(token);

        // Assert
        Assert.Equal(accountUid, uid);
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenNotExists_ShouldThrowException()
    {
        // Arrange
        var storage = CreateStorage();
        var accountUid = Guid.NewGuid();
        await storage.GenerateTokenAsync(accountUid); // Another token

        // Act
        var exc = await Assert.ThrowsAsync<WrongEmailVerificationTokenException>(async () =>
            await storage.VerifyTokenAsync("bla-bla-bla"));

        // Assert
        Assert.Equal("The token is not found", exc.Message);
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenExpired_ShouldThrowException()
    {
        // Arrange
        var client = fixture.CreateClient();
        var options = Options.Create(
            new MongoDbEmailVerificationTokenStorageOptions
            {
                Database = $"test_storage_{Guid.NewGuid()}",
                Ttl = new TimeSpan(0, 0, 0)
            }
        );
        var storage = new MongoDbEmailVerificationTokenStorage(client, options);
        var accountUid = Guid.NewGuid();
        var token = await storage.GenerateTokenAsync(accountUid);

        // Act
        var exc = await Assert.ThrowsAsync<WrongEmailVerificationTokenException>(async () =>
            await storage.VerifyTokenAsync(token));

        // Assert
        Assert.Equal("The token is expired", exc.Message);
    }

    [Fact]
    public async Task DeleteTokens_Valid_ShouldDelete()
    {
        // Arrange
        var storage = CreateStorage();
        var accountUid = Guid.NewGuid();
        var token = await storage.GenerateTokenAsync(accountUid);
        var otherToken = await storage.GenerateTokenAsync(Guid.NewGuid()); // Another account

        // Act
        await storage.DeleteTokensAsync(accountUid);

        // Assert
        await Assert.ThrowsAsync<WrongEmailVerificationTokenException>(async () =>
            await storage.VerifyTokenAsync(token));
        await storage.VerifyTokenAsync(otherToken);
    }

    private IEmailVerificationTokenStorage CreateStorage()
    {
        var client = fixture.CreateClient();
        var options = CreateOptions();
        return new MongoDbEmailVerificationTokenStorage(client, options);
    }

    private IOptions<MongoDbEmailVerificationTokenStorageOptions> CreateOptions()
    {
        var options = new MongoDbEmailVerificationTokenStorageOptions
        {
            Database = $"test_storage_{Guid.NewGuid()}"
        };
        return Options.Create(options);
    }
}
