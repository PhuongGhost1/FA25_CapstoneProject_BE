namespace CusomMapOSM_Commons;

public static class Utilities
{
    private static readonly char[] chars =
   "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly Random random = new Random();
    public static string Capitalize(string sequence)
    {
        return sequence.Substring(0, 1).ToUpper() + sequence.Substring(1);
    }
    public static string GenerateRandomString(int length)
    {
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public static DateTimeOffset EndOfMonth()
    {
        var now = DateTimeOffset.Now;
        return new DateTimeOffset(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, 999, now.Offset);
    }
}