using ChatService.Domain.Abstractions;
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
            .AddScoped<IGroupRepository, GroupRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IMessageRepository, MessageRepository>()
            .AddScoped<IMessageReadStatusRepository, MessageReadStatusRepository>();
    }

    private static void AddConfigurationService(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services
            .Configure<MongoDbSetting>(configuration.GetSection(MongoDbSetting.SectionName));
    }

    private static void AddDatabaseConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();
    }
}
