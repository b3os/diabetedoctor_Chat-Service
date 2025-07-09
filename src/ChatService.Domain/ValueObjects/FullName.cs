namespace ChatService.Domain.ValueObjects;

public sealed class FullName : ValueObject
{
    [BsonElement("last_name")]
    public string LastName { get; private set; } = null!;
    
    [BsonElement("middle_name")]
    public string? MiddleName { get; private set; }
    
    [BsonElement("first_name")]
    public string FirstName { get; private set; } = null!;
    
    private FullName() { }

    private FullName(string lastName, string? middleName, string firstName)
    {
        LastName = lastName;
        MiddleName = middleName;
        FirstName = firstName;
    }

    public static FullName Create(string lastName, string? middleName, string firstName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("LastName phải bắt buộc");
        }
        
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("FirstName phải bắt buộc");
        }

        return new FullName(lastName.Trim(), middleName?.Trim(), firstName.Trim());
    }
        
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return MiddleName ?? string.Empty;
        yield return LastName;
    }

    public override string ToString()
    {
        return string.Join(" ",
            new[] { FirstName, MiddleName, LastName }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}