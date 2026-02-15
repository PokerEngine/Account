using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class NicknameTest
{
    [Theory]
    [InlineData("alice")]
    [InlineData("Alice123")]
    [InlineData("ALICE_123")]
    public void Constructor_WhenValid_ShouldConstruct(string name)
    {
        // Arrange & Act
        var nickname = new Nickname(name);

        // Assert
        Assert.Equal(name, (string)nickname);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ali")]
    [InlineData("ali ")]
    public void Constructor_WhenTooShort_ShouldThrowException(string name)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidNicknameException>(() => new Nickname(name));
        Assert.StartsWith("Nickname must contain at least 4 symbol(s)", exc.Message);
    }

    [Fact]
    public void Constructor_WhenTooLong_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidNicknameException>(() => new Nickname("alice_bobby_charlie_diana_emily_frank"));
        Assert.StartsWith("Nickname must not contain more than 32 symbol(s)", exc.Message);
    }

    [Theory]
    [InlineData("1alice")]
    [InlineData("@lpha")]
    [InlineData("alph@")]
    [InlineData("alice bobby")]
    [InlineData("alice!")]
    public void Constructor_WhenWrongSymbols_ShouldThrowException(string name)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidNicknameException>(() => new Nickname(name));
        Assert.StartsWith(
            "Nickname must start with a latin letter and contain only latin letters, digits and underscore symbols",
            exc.Message
        );
    }

    [Fact]
    public void Equality_WhenValid_ShouldCheck()
    {
        // Arrange & Act & Assert
        Assert.True(new Nickname("alice") == new Nickname("alice"));
        Assert.False(new Nickname("alice") == new Nickname("Alice"));
        Assert.False(new Nickname("alice") == new Nickname("bobby"));

        Assert.False(new Nickname("alice") != new Nickname("alice"));
        Assert.True(new Nickname("alice") != new Nickname("Alice"));
        Assert.True(new Nickname("alice") != new Nickname("bobby"));
    }

    [Fact]
    public void ToString_WhenValid_ShouldReturnValidString()
    {
        // Arrange
        var nickname = new Nickname("alice");

        // Act & Assert
        Assert.Equal("alice", $"{nickname}");
    }
}
