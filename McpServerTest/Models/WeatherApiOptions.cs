

namespace McpServerTest.Models
{
    public class WeatherApiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string CurrentForecastBaseUrl { get; set; } = string.Empty;
        public string OneCallBaseUrl { get; set; } = string.Empty;
    }
}
