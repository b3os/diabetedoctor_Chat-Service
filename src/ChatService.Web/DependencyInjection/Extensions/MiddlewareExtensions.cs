using ChatService.Presentation.Middlewares;
using ChatService.Presentation.V1;

namespace ChatService.Web.DependencyInjection.Extensions;

public static class MiddlewareExtensions
{
    public static void ConfigureMiddleware(this WebApplication app)
    {
        
        //if (app.Environment.IsDevelopment())
        //{
        //    app.ConfigureSwagger();
        //}

        app.ConfigureSwagger();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        
        app.UseAuthorization();
        
        app.UseCors();
        
        // app.MapCarter();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // app.NewVersionedApi(GroupEndpoints.ApiName)
        //     .MapGroupApiV1();
        
        app.NewVersionedApi(UserEndpoints.ApiName)
            .MapUserApiV1();
        
        app.NewVersionedApi(ChatEndpoints.ApiName)
            .MapChatApiV1();
        
        app.NewVersionedApi(ConversationEndpoints.ApiName)
            .MapConversationApiV1();
        
        app.NewVersionedApi(MediaEndpoints.ApiName)
            .MapMediaApiV1();
        
    }
}