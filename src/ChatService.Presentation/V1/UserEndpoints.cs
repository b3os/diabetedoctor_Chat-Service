using ChatService.Contract.Abstractions.Message;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.UserIntegrationEvents;
using ChatService.Contract.Services.User.Commands;
using FluentValidation;

namespace ChatService.Presentation.V1;

public static class UserEndpoints
{
    public const string ApiName = "users";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapUserApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

        // group.MapPost("", CreateUser);
        // group.MapPatch("", UpdateUser);
        group.MapPost("/user/{objectId}", Test);

        return builder;
    }

    // private static async Task UpdateUser(ISender sender)
    // {
    //     var userid = "b93d6316-be4c-4885-a5e0-eae1ea3d1379";
    //     var result = await sender.Send(new UpdateUserCommand {Id = userid, FullName = "Nguyễn Đỗ Chung Quý"});
    // }

    private static IResult Test(ISender sender, [FromBody]Test test, string testId)
    {
        IValidator<Test> validator = new TestValidation();
        if (!ObjectId.TryParse(testId, out var objectId))
        {
            throw new FormatException("Invalid ObjectId");
        }
        test.Id = objectId;
        
        var result = validator.Validate(test);
        return result.IsValid ? Results.Ok(test) : Results.Ok(result);
    } 
    
    private static async Task<IResult> CreateUser(ISender sender, IEventPublisher eventPublisher)
    {
        var @event = new UserCreatedIntegrationEvent()
        {
            UserId = "4da28295-2ec3-4b48-9a7f-7f74d0152138",
            Avatar = "",
            FullName = "string"
        };

        await eventPublisher.PublishAsync("user_topic", @event, 0);
        // var result = await sender.Send(new CreateUserCommand
        // {
        //     Id = "ab88428d-2c9b-439f-b497-6c39cb77f80f",
        //     FullName = "Nguyễn Đỗ Chung Quý",
        //     Avatar = ""
        // });
        return Results.Ok();
    } 
}

public class Test : ICommand
{
    public ObjectId? Id { get; set; }
    public string Avatar { get; set; }
}

public class TestValidation : AbstractValidator<Test>
{
    public TestValidation()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}