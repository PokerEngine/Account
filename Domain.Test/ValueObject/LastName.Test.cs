using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class LastNameTest
{
    [Theory]
    [InlineData("alice")]
    [InlineData("Alice")]
    [InlineData("ALICE")]
    public void Constructor_WhenValid_ShouldConstruct(string name)
    {
        // Arrange & Act
        var lastName = new LastName(name);

        // Assert
        Assert.Equal(name, (string)lastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenEmpty_ShouldThrowException(string name)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidLastNameException>(() => new LastName(name));
        Assert.StartsWith("Last name cannot be empty", exc.Message);
    }

    [Fact]
    public void Constructor_WhenTooLong_ShouldThrowException()
    {
        // Arrange
        var name = "AliceBobbyCharlieDianaEmilyFrankGeorgeHannahIsabellaJackKevinLiamMiaNoahOliviaPaulQuinnRachelSophiaThomasUrsulaVictoriaWilliamXanderYaraZoe";

        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidLastNameException>(() => new LastName(name));
        Assert.StartsWith("Last name cannot contain more than 64 symbol(s)", exc.Message);
    }

    [Fact]
    public void Equality_WhenValid_ShouldCheck()
    {
        // Arrange & Act & Assert
        Assert.True(new LastName("alice") == new LastName("alice"));
        Assert.False(new LastName("alice") == new LastName("Alice"));
        Assert.False(new LastName("alice") == new LastName("bobby"));

        Assert.False(new LastName("alice") != new LastName("alice"));
        Assert.True(new LastName("alice") != new LastName("Alice"));
        Assert.True(new LastName("alice") != new LastName("bobby"));
    }

    [Fact]
    public void ToString_WhenValid_ShouldReturnValidString()
    {
        // Arrange
        var lastName = new LastName("alice");

        // Act & Assert
        Assert.Equal("alice", $"{lastName}");
    }
}
