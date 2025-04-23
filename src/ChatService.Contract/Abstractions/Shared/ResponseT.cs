namespace ChatService.Contract.Abstractions.Shared;

public class Response<T> : IEquatable<Response<T>>
{
    public static readonly Response<T> None = new(string.Empty, string.Empty, default);
    public static readonly Response<T> OperationCompleted = new("Response.OperationCompleted", "The operation was successfully completed.", default);

    public Response(string code, string message, T? data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public string Code { get; }
    public string Message { get; }
    public T? Data { get; } // The data can be null

    public static implicit operator string(Response<T> response) => response.Code;

    public static bool operator ==(Response<T>? a, Response<T>? b)
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

    public static bool operator !=(Response<T>? a, Response<T>? b) => !(a == b);

    public virtual bool Equals(Response<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message && EqualityComparer<T?>.Default.Equals(Data, other.Data);
    }

    public override bool Equals(object? obj) => obj is Response<T> success && Equals(success);

    public override int GetHashCode() => HashCode.Combine(Code, Message, Data);

    public override string ToString() => Code;
}
