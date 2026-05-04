namespace DemoCSharpAgent.Configuration;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public required string BaseUrl { get; set; }

    public required string ApiKey { get; set; }
}