using System.Text.RegularExpressions;

namespace ChatService.Contract.Helpers;

public static class NormalizeToRegex
{
    public static string NormalizeInput(string input)
    {
        var trimmed = input.Trim();
        var normalized = Regex.Replace(trimmed, @"\s+", @"\s+");
        return normalized;
    }
}