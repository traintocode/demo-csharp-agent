using DemoCSharpAgent.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoCSharpAgent.DependencyInjection;

public static class DemoCSharpAgentServiceCollectionExtensions
{
    public static IServiceCollection AddDemoCSharpAgent(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddWeatherServices(configuration);
        services.AddAgentChatServices();

        return services;
    }
}
