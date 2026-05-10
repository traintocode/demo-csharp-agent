using DemoCSharpAgent.Agent;
using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DemoCSharpAgent.DependencyInjection;

internal static class AgentChatServicesExtensions
{
    internal static IServiceCollection AddAgentChatServices(this IServiceCollection services)
    {
        services.AddSingleton<IConversationStore, InMemoryConversationStore>();
        services.AddSingleton<EmailService>();

        services.AddScoped<IAiToolsProvider, AiToolsProvider>();

        services.AddScoped<ChatClientAgent>(sp =>
        {
            var tools = sp.GetRequiredService<IAiToolsProvider>().Tools;
            var openAi = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            var openAiClient = new OpenAIClient(openAi.ApiKey);
            var chatClient = openAiClient.GetChatClient(openAi.Model);

            return chatClient.AsAIAgent(
                instructions: AgentPrompts.System,
                name: null,
                description: null,
                tools: tools,
                clientFactory: null,
                loggerFactory: null,
                services: sp);
        });

        services.AddScoped<IAgentService, AgentService>();

        return services;
    }
}
