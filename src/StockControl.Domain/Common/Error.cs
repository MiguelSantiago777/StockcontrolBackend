namespace StockControl.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure
}

public sealed class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public Error(string code, string message, ErrorType type = ErrorType.Failure)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public static Error Validation(string code, string message)
    {
        return new Error(code, message, ErrorType.Validation);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Unauthorized(string code, string message)
    {
        return new Error(code, message, ErrorType.Unauthorized);
    }

    public static Error Forbidden(string code, string message)
    {
        return new Error(code, message, ErrorType.Forbidden);
    }

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message && Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Error);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message, Type);
    }

    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }
}
