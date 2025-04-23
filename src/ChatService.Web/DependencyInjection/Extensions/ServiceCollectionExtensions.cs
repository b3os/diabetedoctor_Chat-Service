using ChatService.Contract.Settings;
using ChatService.Presentation.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatService.Web.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions 
{
    public static IServiceCollection AddConfigurationAppSetting
     (this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<KafkaSetting>(configuration.GetSection(KafkaSetting.SectionName))
            .Configure<MongoDbSetting>(configuration.GetSection(MongoDbSetting.SectionName));

        return services;
    }

    public static void AddWebService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddConfigurationAppSetting(builder.Configuration);

        builder.Services.AddCarter();

        builder.Services.AddScoped<ExceptionHandlingMiddleware>();

        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication();

        builder.Services
            .AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwagger();

        builder.Services
            .AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}