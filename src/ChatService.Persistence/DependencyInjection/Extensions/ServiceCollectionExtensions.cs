using Microsoft.Extensions.Hosting;

namespace ChatService.Persistence.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        
        builder.AddDatabaseConfiguration(builder.Configuration);
        builder.AddConfigurationService(builder.Configuration);
    }

    private static void AddConfigurationService(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services
            .Configure<MongoDbSetting>(configuration.GetSection(MongoDbSetting.SectionName));
    }

    private static void AddDatabaseConfiguration(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddSingleton<MongoDbContext>();
    }
}
