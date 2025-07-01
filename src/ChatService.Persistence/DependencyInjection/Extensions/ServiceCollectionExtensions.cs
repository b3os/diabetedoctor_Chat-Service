using ChatService.Persistence.Repositories;
using Microsoft.Extensions.Hosting;

namespace ChatService.Persistence.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        
        builder.AddDatabaseConfiguration();
        builder.AddConfigurationService(builder.Configuration);
        
        builder.Services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IConversationRepository, ConversationRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IMessageRepository, MessageRepository>()
            .AddScoped<IParticipantRepository, ParticipantRepository>()
            .AddScoped<IMediaRepository, MediaRepository>()
            .AddScoped<IOutboxEventRepository, OutboxEventRepository>()
            .AddScoped<IOutBoxEventConsumerRepository, OutBoxEventConsumerRepository>();
    }

    private static void AddConfigurationService(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services
            .Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
    }

    private static void AddDatabaseConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();
    }
}
