namespace ChatService.Contract.Helpers;

public static class SearchHelper
{
    public static string NormalizeSearchText(string input)
    {
        var trimmed = input.Trim();

        return trimmed.IsPhoneNumber() ? trimmed.ToNormalizePhone() : trimmed;
    }
}