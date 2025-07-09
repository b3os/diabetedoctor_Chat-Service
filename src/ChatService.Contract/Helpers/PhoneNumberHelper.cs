using System.Text.RegularExpressions;

namespace ChatService.Contract.Helpers;

public static class PhoneNumberHelper
{
    private static readonly Regex PhoneRegex = new(@"^(0|\+?84)([1-9][0-9]{8})$", RegexOptions.Compiled);

    public static bool IsPhoneNumber(this string phoneNumber)
    {
        return PhoneRegex.IsMatch(phoneNumber);
    }

    public static string ToNormalizePhone(this string phoneNumber)
    {
        if (phoneNumber.StartsWith("+84"))
        {
            return "0" + phoneNumber[3..];
        }

        if (phoneNumber.StartsWith("84"))
        {
            return "0" + phoneNumber[2..];
        }

        return phoneNumber;
    }
}