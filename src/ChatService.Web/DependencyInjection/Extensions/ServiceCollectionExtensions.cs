using System.Text;
using System.Text.Json.Serialization;
using ChatService.Contract.Settings;
using ChatService.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;

namespace ChatService.Web.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions 
{
    private static void AddConfigurationAppSetting(this IHostApplicationBuilder builder)
    {
        builder.Services
            .Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName))
            .Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName))
            .Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName))
            .Configure<AblySetting>(builder.Configuration.GetSection(AblySetting.SectionName))
            .Configure<CloudinarySettings>(builder.Configuration.GetSection(CloudinarySettings.SectionName))
            .Configure<AppDefaultSettings>(builder.Configuration.GetSection(AppDefaultSettings.SectionName));
    }

    private static void AddAuthenticationAndAuthorization(this IHostApplicationBuilder builder)
    {
        var authSettings = builder.Configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>() ?? new AuthSettings();


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
        
        // builder.Services.Configure<JsonOptions>(options =>
        // {
        //     options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // });
        
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
        
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        
        builder.Services.AddCors(options => options.AddDefaultPolicy(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
    }
}