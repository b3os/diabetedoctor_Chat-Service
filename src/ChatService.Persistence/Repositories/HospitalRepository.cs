namespace ChatService.Persistence.Repositories;

public class HospitalRepository(IMongoDbContext context) : RepositoryBase<Hospital>(context), IHospitalRepository
{
    
};