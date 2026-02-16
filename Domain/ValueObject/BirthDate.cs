using Domain.Exception;
using System.Globalization;

namespace Domain.ValueObject;

public readonly struct BirthDate : IEquatable<BirthDate>
{
    private readonly DateOnly _date;

    private const string Format = "yyyy-MM-dd";
    private const int MinAge = 18;

    public int Age => GetAge(_date);

    public BirthDate(DateOnly date)
    {
        if (GetAge(date) < MinAge)
        {
            throw new InvalidBirthDateException($"You must be at least {MinAge} year(s) old");
        }

        _date = date;
    }

    public static BirthDate FromString(string value, string format = Format)
    {
        var date = DateOnly.ParseExact(value, format, CultureInfo.InvariantCulture);
        return new BirthDate(date);
    }

    private static int GetAge(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - date.Year;
        if (date > today.AddYears(-age)) age--;
        return age;
    }

    public static implicit operator DateOnly(BirthDate a)
        => a._date;

    public static implicit operator BirthDate(DateOnly a)
        => new(a);

    public static bool operator ==(BirthDate a, BirthDate b)
        => a._date == b._date;

    public static bool operator !=(BirthDate a, BirthDate b)
        => a._date != b._date;

    public bool Equals(BirthDate other)
        => _date.Equals(other._date);

    public override bool Equals(object? o)
        => o is not null && o.GetType() == GetType() && _date.Equals(((BirthDate)o)._date);

    public override int GetHashCode()
        => _date.GetHashCode();

    public override string ToString()
        => _date.ToString(Format);

    public string ToString(string format = Format)
         => _date.ToString(format, CultureInfo.InvariantCulture);
}
