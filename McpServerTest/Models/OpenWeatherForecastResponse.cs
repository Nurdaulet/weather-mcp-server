

using System.Text.Json.Serialization;

namespace McpServerTest.Models
{
    public class OpenWeatherForecastResponse
    {
        public CityInfo City { get; set; }
        public List<ForecastItem> List { get; set; }
    }

    public class CityInfo
    {
        public string Name { get; set; }
    }

    public class ForecastItem
    {
        public string Dt_txt { get; set; }
        public DateTime DateTime => DateTime.Parse(Dt_txt);
        public MainInfo Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
    }

}
