using ChatService.Web.DependencyInjection.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ChatService.Web.DependencyInjection.Extensions;


    public static class SwaggerExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        []
                    }
                });
            });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        }
 
        public static void ConfigureSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var version in app.DescribeApiVersions().Select(version => version.GroupName))
                    options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
 
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
                options.DocExpansion(DocExpansion.None);
            });
 
            app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
                .WithTags(string.Empty);
        }
    }
