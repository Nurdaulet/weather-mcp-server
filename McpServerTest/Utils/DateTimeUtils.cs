

namespace McpServerTest.Utils
{
    public static class DateTimeUtils
    {
        public static DateTime UnixToDateTime(long unixTime) =>
        DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToLocalTime();
    }
}
