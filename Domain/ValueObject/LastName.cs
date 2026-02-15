using Domain.Exception;

namespace Domain.ValueObject;

public readonly struct LastName : IEquatable<LastName>
{
    private readonly string _name;

    private const int MaxLength = 64;

    public LastName(string name)
    {
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidLastNameException("Last name cannot be empty");
        }
        if (name.Length > MaxLength)
        {
            throw new InvalidLastNameException($"Last name cannot contain more than {MaxLength} symbol(s)");
        }

        _name = name;
    }
    public static implicit operator string(LastName a)
        => a._name;

    public static implicit operator LastName(string a)
        => new(a);

    public static bool operator ==(LastName a, LastName b)
        => a._name == b._name;

    public static bool operator !=(LastName a, LastName b)
        => a._name != b._name;

    public bool Equals(LastName other)
        => _name.Equals(other._name);

    public override bool Equals(object? o)
        => o is not null && o.GetType() == GetType() && _name.Equals(((LastName)o)._name);

    public override int GetHashCode()
        => _name.GetHashCode();

    public override string ToString()
        => _name;
}
