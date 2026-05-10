using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoCSharpAgent.DependencyInjection;

internal static class WeatherServicesExtensions
{
    internal static IServiceCollection AddWeatherServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WeatherApiOptions>(configuration.GetSection(WeatherApiOptions.SectionName));
        services.AddHttpClient<IWeatherService, WeatherService>();
        return services;
    }
}
