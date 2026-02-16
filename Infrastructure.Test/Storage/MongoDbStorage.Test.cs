using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Domain.ValueObject;
using Infrastructure.Storage;
using Infrastructure.Test.Client.MongoDb;
using Microsoft.Extensions.Options;

namespace Infrastructure.Test.Storage;

[Trait("Category", "Integration")]
public class MongoDbStorageTest(MongoDbClientFixture fixture) : IClassFixture<MongoDbClientFixture>
{
    [Fact]
    public async Task GetDetailViewAsync_WhenExists_ShouldReturn()
    {
        // Arrange
        var storage = CreateStorage();
        var account = RegisterAccount("Alice", "alice.alright@test.com", "Alice", "Alright", "2000-01-01");
        await storage.SaveViewAsync(account);

        // Act
        var view = await storage.GetDetailViewAsync(account.Uid);

        // Assert
        Assert.Equal(account.Uid, (AccountUid)view.Uid);
        Assert.Equal("Alice", view.Nickname);
        Assert.Equal("alice.alright@test.com", view.Email);
        Assert.Equal("Alice", view.FirstName);
        Assert.Equal("Alright", view.LastName);
        Assert.Equal("2000-01-01", view.BirthDate);
    }

    [Fact]
    public async Task GetDetailViewAsync_WhenNotExists_ShouldThrowException()
    {
        // Arrange
        var storage = CreateStorage();

        // Act
        var exc = await Assert.ThrowsAsync<AccountNotFoundException>(async () =>
            await storage.GetDetailViewAsync(new AccountUid(Guid.NewGuid())));

        // Assert
        Assert.Equal("The account is not found", exc.Message);
    }

    private IStorage CreateStorage()
    {
        var client = fixture.CreateClient();
        var options = CreateOptions();
        return new MongoDbStorage(client, options);
    }

    private IOptions<MongoDbStorageOptions> CreateOptions()
    {
        var options = new MongoDbStorageOptions
        {
            Database = $"test_storage_{Guid.NewGuid()}"
        };
        return Options.Create(options);
    }

    private Account RegisterAccount(
        string nickname,
        string email,
        string firstName,
        string lastName,
        string birthDate
    )
    {
        return Account.FromScratch(
            uid: new AccountUid(Guid.NewGuid()),
            nickname: nickname,
            email: email,
            firstName: firstName,
            lastName: lastName,
            birthDate: BirthDate.FromString(birthDate)
        );
    }
}
