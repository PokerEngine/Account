using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class EmailTest
{
    [Theory]
    [InlineData("a@test.com")]
    [InlineData("alice@test.com")]
    [InlineData("alice112@test.com")]
    [InlineData("alice.alright@test.com")]
    [InlineData("alice_aliright@test.com")]
    [InlineData("alice-aliright@test.com")]
    [InlineData("alice+alright@test.com")]
    public void Constructor_WhenValid_ShouldConstruct(string value)
    {
        // Arrange & Act
        var email = new Email(value);

        // Assert
        Assert.Equal(value, (string)email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenEmpty_ShouldThrowException(string value)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidEmailException>(() => new Email(value));
        Assert.StartsWith("Email cannot be empty", exc.Message);
    }

    [Theory]
    [InlineData("@test.com")]
    [InlineData("test.com")]
    [InlineData("alice")]
    [InlineData("alice@")]
    [InlineData("alice@test@test.com")]
    public void Constructor_WhenInvalid_ShouldThrowException(string value)
    {
        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidEmailException>(() => new Email(value));
        Assert.StartsWith("Invalid email format", exc.Message);
    }

    [Fact]
    public void Equality_WhenValid_ShouldCheck()
    {
        // Arrange & Act & Assert
        Assert.True(new Email("alice@test.com") == new Email("alice@test.com"));
        Assert.True(new Email("alice@test.com") == new Email("Alice@Test.Com"));
        Assert.False(new Email("alice@test.com") == new Email("alice@test.net"));
        Assert.False(new Email("alice@test.com") == new Email("bobby@test.com"));

        Assert.False(new Email("alice@test.com") != new Email("alice@test.com"));
        Assert.False(new Email("alice@test.com") != new Email("Alice@Test.Com"));
        Assert.True(new Email("alice@test.com") != new Email("alice@test.net"));
        Assert.True(new Email("alice@test.com") != new Email("bobby@test.com"));
    }

    [Fact]
    public void ToString_WhenValid_ShouldReturnValidString()
    {
        // Arrange
        var email = new Email("alice@test.com");

        // Act & Assert
        Assert.Equal("alice@test.com", $"{email}");
    }
}
