using ChatService.Application;
using ChatService.Application.Behaviors;
using ChatService.Application.DependencyInjection.Extensions;

namespace ChatService.Application.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddConfigureMediatR(this IServiceCollection services)
    => services.AddMediatR(config => config.RegisterServicesFromAssemblies(
            AssemblyReference.Assembly,
            Assembly.Load("ChatService.Infrastructure")
            ))
          .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
          //.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
          //.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))
          .AddValidatorsFromAssembly(Contract.AssemblyReference.Assembly, includeInternalTypes: true);

    private static IServiceCollection AddMappingConfig(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        return services;
    }

    public static void AddApplicationService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMappingConfig();
        builder.Services.AddConfigureMediatR();
    }


}