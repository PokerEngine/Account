using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class BirthDateTest
{
    [Theory]
    [InlineData(1900, 1, 1)]
    [InlineData(2006, 12, 31)]
    public void Constructor_WhenValid_ShouldConstruct(int year, int month, int day)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        var birthDate = new BirthDate(date);

        // Assert
        Assert.Equal(date, (DateOnly)birthDate);
    }

    [Fact]
    public void Constructor_WhenLessThan18_ShouldThrowException()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now.AddYears(-17));

        // Arrange & Act & Assert
        var exc = Assert.Throws<InvalidBirthDateException>(() => new BirthDate(date));
        Assert.Equal("You must be at least 18 year(s) old", exc.Message);
    }

    [Fact]
    public void Equality_WhenValid_ShouldCheck()
    {
        // Arrange & Act & Assert
        Assert.True(new BirthDate(new DateOnly(2000, 1, 1)) == new BirthDate(new DateOnly(2000, 1, 1)));
        Assert.False(new BirthDate(new DateOnly(2000, 1, 1)) == new BirthDate(new DateOnly(2000, 1, 2)));

        Assert.False(new BirthDate(new DateOnly(2000, 1, 1)) != new BirthDate(new DateOnly(2000, 1, 1)));
        Assert.True(new BirthDate(new DateOnly(2000, 1, 1)) != new BirthDate(new DateOnly(2000, 1, 2)));
    }

    [Fact]
    public void ToString_WhenValid_ShouldReturnValidString()
    {
        // Arrange
        var birthDate = new BirthDate(new DateOnly(2000, 1, 1));

        // Act & Assert
        Assert.Equal("2000-01-01", $"{birthDate}");
    }
}
