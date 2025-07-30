
namespace McpServerTest.Models
{
    public class OneCallResponse
    {
        public List<WeatherAlert> Alerts { get; set; }
    }

    public class WeatherAlert
    {
        public string Event { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Description { get; set; }
    }

}
