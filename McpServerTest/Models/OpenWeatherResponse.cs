
namespace McpServerTest.Models
{
    public partial class OpenWeatherResponse
    {
        public string Name { get; set; }
        public MainInfo Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
    }

    public class CoordInfo
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public partial class OpenWeatherResponse
    {
        public CoordInfo Coord { get; set; }
    }



    public class MainInfo
    {
        public double Temp { get; set; }
    }

    public class WeatherInfo
    {
        public string Description { get; set; }
    }
}
