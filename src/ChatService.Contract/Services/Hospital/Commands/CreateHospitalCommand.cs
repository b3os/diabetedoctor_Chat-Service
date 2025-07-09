namespace ChatService.Contract.Services.Hospital.Commands;

public record CreateHospitalCommand() : ICommand
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
};