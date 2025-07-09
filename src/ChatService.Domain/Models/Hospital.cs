namespace ChatService.Domain.Models;

public class Hospital : DomainEntity<ObjectId>
{
    [BsonElement("hospital_id")] 
    public HospitalId HospitalId { get; private set; } = null!;
    
    [BsonElement("name")]
    public string Name { get; private set; } = null!;

    public static Hospital Create(ObjectId id, HospitalId hospitalId, string name)
    {
        return new Hospital()
        {
            Id = id,
            HospitalId = hospitalId,
            Name = name,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public void Modify(string? name)
    {
        Name = name ?? Name;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}