namespace ChatService.Persistence.Repositories;

public class MediaRepository(IMongoDbContext context) : RepositoryBase<Media>(context), IMediaRepository
{
};