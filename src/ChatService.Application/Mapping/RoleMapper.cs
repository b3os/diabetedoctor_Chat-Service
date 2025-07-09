namespace ChatService.Application.Mapping;

public static class RoleMapper
{
    public static Role MapFromInt(int value)
    {
        if (Enum.IsDefined(typeof(Role), value))
        {
            return (Role)value;
        }

        throw new ArgumentException($"Không thể map giá trị [{value}] sang enum {nameof(Role)}");
    }
}