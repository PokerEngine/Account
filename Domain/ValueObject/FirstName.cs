using Domain.Exception;

namespace Domain.ValueObject;

public readonly struct FirstName : IEquatable<FirstName>
{
    private readonly string _name;

    private const int MaxLength = 64;

    public FirstName(string name)
    {
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidFirstNameException("First name cannot be empty");
        }
        if (name.Length > MaxLength)
        {
            throw new InvalidFirstNameException($"First name cannot contain more than {MaxLength} symbol(s)");
        }

        _name = name;
    }
    public static implicit operator string(FirstName a)
        => a._name;

    public static implicit operator FirstName(string a)
        => new(a);

    public static bool operator ==(FirstName a, FirstName b)
        => a._name == b._name;

    public static bool operator !=(FirstName a, FirstName b)
        => a._name != b._name;

    public bool Equals(FirstName other)
        => _name.Equals(other._name);

    public override bool Equals(object? o)
        => o is not null && o.GetType() == GetType() && _name.Equals(((FirstName)o)._name);

    public override int GetHashCode()
        => _name.GetHashCode();

    public override string ToString()
        => _name;
}
