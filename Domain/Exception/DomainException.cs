namespace Domain.Exception;

public abstract class DomainException(string message) : System.Exception(message);

public class InvalidNicknameException(string message) : DomainException(message);

public class NotUniqueNicknameException(string message) : DomainException(message);

public class InvalidEmailException(string message) : DomainException(message);

public class NotUniqueEmailException(string message) : DomainException(message);

public class InvalidFirstNameException(string message) : DomainException(message);

public class InvalidLastNameException(string message) : DomainException(message);

public class InvalidBirthDateException(string message) : DomainException(message);
