namespace Domain.Exception;

public abstract class InvariantViolatedException(string message) : System.Exception(message);

public class InvalidAccountStateException(string message) : InvariantViolatedException(message);
