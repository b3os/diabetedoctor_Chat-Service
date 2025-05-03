using System.Text;
using System.Text.Json;
using ChatService.Contract.Settings;
using ChatService.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ChatService.Web.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions 
{
    private static void AddConfigurationAppSetting(this IHostApplicationBuilder builder)
    {
        builder.Services
            .Configure<KafkaSetting>(builder.Configuration.GetSection(KafkaSetting.SectionName))
            .Configure<MongoDbSetting>(builder.Configuration.GetSection(MongoDbSetting.SectionName))
            .Configure<AuthSetting>(builder.Configuration.GetSection(AuthSetting.SectionName));
    }

    private static void AddAuthenticationAndAuthorization(this IHostApplicationBuilder builder)
    {
        var authSettings = builder.Configuration.GetSection(AuthSetting.SectionName).Get<AuthSetting>() ?? new AuthSetting();


        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authSettings.Issuer,
                    ValidAudience = authSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.AccessSecretToken)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("IS-TOKEN-EXPIRED", "true");
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        builder.Services.AddAuthorizationBuilder();
    }

    public static void AddWebService(this IHostApplicationBuilder builder)
    {
        builder.AddConfigurationAppSetting();
        
        builder.Services.AddCarter();

        builder.Services.AddScoped<ExceptionHandlingMiddleware>();

        builder.Services.AddHttpContextAccessor();
        
        builder.AddAuthenticationAndAuthorization();

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