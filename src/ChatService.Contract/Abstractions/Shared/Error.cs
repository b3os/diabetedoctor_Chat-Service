namespace ChatService.Contract.Abstractions.Shared;

public class Error : IEquatable<Error>
{
    public Error(string code, object description, ErrorType errorType)
    {
        Code = code;
        Description = description;
        ErrorType = errorType;
    }

    public string Code { get; }

    public object Description { get; }
    
    public ErrorType ErrorType { get; }
    
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", ErrorType.Failure);
    
    public static Error Failure(string description) =>
        new("System.Failure", description, ErrorType.Failure);
    
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);
    
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
    
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
    
    public static Error Conflict(string code, object description) =>
        new(code, description, ErrorType.Conflict);
    
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
    
    public static Error BadRequest(string code, string description) =>
        new(code, description, ErrorType.BadRequest);
    
    public static implicit operator string(Error error) => error.Code;

    public static bool operator ==(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Error? a, Error? b) => !(a == b);

    public virtual bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Description == other.Description;
    }

    public override bool Equals(object? obj) => obj is Error error && Equals(error);

    public override int GetHashCode() => HashCode.Combine(Code, Description);

    public override string ToString() => Code;
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    BadRequest = 5,
}