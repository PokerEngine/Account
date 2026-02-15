using Domain.Exception;
using System.Net.Mail;

namespace Domain.ValueObject;

public readonly struct Email : IEquatable<Email>
{
    private readonly MailAddress _email;

    public Email(string email)
    {
        email = email.Trim();

        if (string.IsNullOrEmpty(email))
        {
            throw new InvalidEmailException("Email cannot be empty");
        }

        try
        {
            _email = new MailAddress(email);
        }
        catch
        {
            throw new InvalidEmailException("Invalid email format");
        }
    }

    public static implicit operator string(Email a)
        => a._email.ToString();

    public static implicit operator Email(string a)
        => new(a);

    public static bool operator ==(Email a, Email b)
        => a.Equals(b);

    public static bool operator !=(Email a, Email b)
        => !a.Equals(b);

    public bool Equals(Email other)
        => _email.Equals(other._email);

    public override bool Equals(object? o)
        => o is not null && o.GetType() == GetType() && _email.Equals(((Email)o)._email);

    public override int GetHashCode()
        => _email.GetHashCode();

    public override string ToString()
        => _email.ToString();
}
