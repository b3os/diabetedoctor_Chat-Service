namespace ChatService.Contract.Abstractions.Shared;

public class Response : IEquatable<Response>
{
    public static readonly Response None = new(string.Empty, string.Empty);
    public static readonly Response OperationCompleted = new("Response.OperationCompleted", "The operation was successfully completed.");

    public Response(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    public static implicit operator string(Response response) => response.Code;

    public static bool operator ==(Response? a, Response? b)
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

    public static bool operator !=(Response? a, Response? b) => !(a == b);

    public virtual bool Equals(Response? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object? obj) => obj is Response success && Equals(success);

    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public override string ToString() => Code;
}