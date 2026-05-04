using DemoCSharpAgent.Agent;
using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoCSharpAgent.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<WeatherApiOptions>(configuration.GetSection(WeatherApiOptions.SectionName));

        services.AddHttpClient<IWeatherService, WeatherApiDotComService>();

        services.AddSingleton<IConversationStore, InMemoryConversationStore>();
        services.AddSingleton<EmailService>();

        services.AddScoped<IAiToolsProvider, AiToolsProvider>();
        services.AddScoped<IAgentService, AgentService>();

        return services;
    }
}


