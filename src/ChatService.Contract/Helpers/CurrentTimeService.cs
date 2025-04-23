namespace ChatService.Contract.Helpers;

public static class CurrentTimeService
{
    private const string VietnamTimeZoneInfo = "SE Asia Standard Time"; 
    public static DateTime GetCurrentTime() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneInfo));
}