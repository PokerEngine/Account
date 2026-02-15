using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class FirstNameTest
{
    [Theory]
    [InlineData("alice")]
    [InlineData("Alice")]
    [InlineData("ALICE")]
    public void Constructor_WhenValid_ShouldConstruct(string name)
    {
        // Arrange & Act
        var firstName = new FirstName(name);

        // Assert
        Assert.Equal(name, (string)firstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenEmpty_ShouldThrowException(string name)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidFirstNameException>(() => new FirstName(name));
        Assert.StartsWith("First name cannot be empty", exc.Message);
    }

    [Fact]
    public void Constructor_WhenTooLong_ShouldThrowException()
    {
        // Arrange
        var name = "AliceBobbyCharlieDianaEmilyFrankGeorgeHannahIsabellaJackKevinLiamMiaNoahOliviaPaulQuinnRachelSophiaThomasUrsulaVictoriaWilliamXanderYaraZoe";

        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidFirstNameException>(() => new FirstName(name));
        Assert.StartsWith("First name cannot contain more than 64 symbol(s)", exc.Message);
    }

    [Fact]
    public void Equality_WhenValid_ShouldCheck()
    {
        // Arrange & Act & Assert
        Assert.True(new FirstName("alice") == new FirstName("alice"));
        Assert.False(new FirstName("alice") == new FirstName("Alice"));
        Assert.False(new FirstName("alice") == new FirstName("bobby"));

        Assert.False(new FirstName("alice") != new FirstName("alice"));
        Assert.True(new FirstName("alice") != new FirstName("Alice"));
        Assert.True(new FirstName("alice") != new FirstName("bobby"));
    }

    [Fact]
    public void ToString_WhenValid_ShouldReturnValidString()
    {
        // Arrange
        var firstName = new FirstName("alice");

        // Act & Assert
        Assert.Equal("alice", $"{firstName}");
    }
}
