using ChatService.Contract.Services.Hospital.Commands;

namespace ChatService.Application.UseCase.V1.IntegrationCommands.Hospitals;

public sealed class CreateHospitalCommandHandler(IHospitalRepository hospitalRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateHospitalCommand>
{
    public async Task<Result> Handle(CreateHospitalCommand request, CancellationToken cancellationToken)
    {
        var hospital = MapToHospital(request);
        await hospitalRepository.CreateAsync(unitOfWork.ClientSession, hospital, cancellationToken);
        return Result.Success();
    }
    
    private Hospital MapToHospital(CreateHospitalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var id = ObjectId.GenerateNewId();
        var hospitalId = HospitalId.Of(command.HospitalId);
        return Hospital.Create(id, hospitalId, command.Name);
    }
}