using System.ComponentModel;
using System.Net.Http.Json;
using McpServerTest.Models;
using McpServerTest.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

public class WeatherTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<WeatherTools> _logger;

    public WeatherTools(IHttpClientFactory httpClientFactory, IOptions<WeatherApiOptions> options, ILogger<WeatherTools> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Gets current weather conditions for the specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return "Please provide a valid city name.";

        var query = string.IsNullOrWhiteSpace(countryCode)
            ? city
            : $"{city},{countryCode}";

        var client = _httpClientFactory.CreateClient("OpenWeather_v2_5");
        var url = $"weather?q={Uri.EscapeDataString(query)}&appid={_options.ApiKey}&units=metric";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get weather for {Query}. Status: {StatusCode}", query, response.StatusCode);
                return $"Could not get weather for {query}.";
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            if (data == null)
                return $"Failed to parse weather data for {query}.";

            var condition = data.Weather.FirstOrDefault()?.Description ?? "unknown";
            return $"Current weather in {data.Name}: {(int)Math.Round(data.Main.Temp)}°C, {condition}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching weather for {Query}", query);
            return $"Error fetching weather for {query}.";
        }
    }

    [McpServerTool]
    [Description("Gets a 3-day weather forecast for the specified city.")]
    public async Task<string> GetWeatherForecast(
    [Description("The city name to get forecast for")] string city,
    [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return "Please provide a valid city name.";

        var query = string.IsNullOrWhiteSpace(countryCode)
            ? city
            : $"{city},{countryCode}";

        var client = _httpClientFactory.CreateClient("OpenWeather_v2_5");
        var url = $"forecast?q={Uri.EscapeDataString(query)}&appid={_options.ApiKey}&units=metric";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Api response failed: {response}", response);
                return $"Failed to get forecast for {query}.";
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherForecastResponse>();
            if (data == null)
            {
                _logger.LogError("Could not parse forecastApi response: {response}", response);
                return $"Could not parse forecast data for {query}.";
            }
               

            var dailyForecasts = data.List
                .GroupBy(f => f.DateTime.ToLocalTime().Date)
                .Take(3)
                .Select(group =>
                {
                    var temps = group.Select(f => f.Main.Temp).ToList();
                    var descriptions = group.Select(f => f.Weather.FirstOrDefault()?.Description ?? "").Distinct().ToList();

                    return $"{group.Key:dd.MM.yyyy}: {(int)Math.Round(temps.Min())}°C to {(int)Math.Round(temps.Max())}°C, {string.Join(", ", descriptions)}";
                });

            return $"3-day forecast for {data.City.Name}:\n" + string.Join("\n", dailyForecasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching forecast for {City}", query);
            return $"Error retrieving forecast for {query}.";
        }
    }

    [McpServerTool]
    [Description("Gets current weather alerts for the specified city if any exist.")]
    public async Task<string> GetWeatherAlerts(
    [Description("The city name to get alerts for")] string city,
    [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return "Please provide a valid city name.";

        var query = string.IsNullOrWhiteSpace(countryCode)
            ? city
            : $"{city},{countryCode}";

        var clientV2 = _httpClientFactory.CreateClient("OpenWeather_v2_5");
        var clientV3 = _httpClientFactory.CreateClient("OpenWeather_v3_0");

        try
        {
            // Step 1: Get coordinates
            var geoUrl = $"weather?q={Uri.EscapeDataString(query)}&appid={_options.ApiKey}";
            var geoResponse = await clientV2.GetAsync(geoUrl);
            if (!geoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("get coordinates response failed: {response}", geoResponse);
                return $"Could not find coordinates for {query}.";
            }
                
            var geoData = await geoResponse.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            if (geoData == null)
                return $"Failed to get location data for {query}.";

            double lat = geoData.Coord.Lat;
            double lon = geoData.Coord.Lon;

            // Step 2: Call One Call API for alerts
            var alertUrl = $"onecall?lat={lat}&lon={lon}&appid={_options.ApiKey}&units=metric";
            var alertResponse = await clientV3.GetAsync(alertUrl);

            if (!alertResponse.IsSuccessStatusCode)
            {
                _logger.LogError("AlertResponse failed: {response}", alertResponse);
                return $"No alerts or service unavailable for {query}.";
            }

            var alertData = await alertResponse.Content.ReadFromJsonAsync<OneCallResponse>();
            if (alertData?.Alerts == null || alertData.Alerts.Count == 0)
                return $"No current weather alerts for {geoData.Name}.";

            var summaries = alertData.Alerts.Select(a =>
                $"- {a.Event}: {a.Description?.Split('\n')[0]} ({DateTimeUtils.UnixToDateTime(a.Start):g} – {DateTimeUtils.UnixToDateTime(a.End):g})");

            return $"Weather alerts for {geoData.Name}:\n" + string.Join("\n", summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather alerts for {City}", query);
            return $"Error retrieving weather alerts for {query}.";
        }
    }
}
