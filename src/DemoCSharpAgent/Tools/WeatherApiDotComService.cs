using System.Text.Json;
using DemoCSharpAgent.Configuration;
using Microsoft.Extensions.Options;

namespace DemoCSharpAgent.Tools;

public class WeatherApiDotComService(HttpClient httpClient, IOptions<WeatherApiOptions> weatherOptions) : IWeatherService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly WeatherApiOptions _options = weatherOptions.Value;

    public async Task<string[]> GetWeatherInCity(string city, CancellationToken cancellationToken = default)
    {
        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/current.json?key={Uri.EscapeDataString(_options.ApiKey)}&q={Uri.EscapeDataString(city)}&aqi=no";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error calling Weather API: {response.StatusCode} - {responseContent}");

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        var descriptionElement = root.GetProperty("current").GetProperty("condition").GetProperty("text");

        string[] descriptions = [descriptionElement.GetString()!];

        return descriptions;
    }
}
