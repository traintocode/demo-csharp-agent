using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Dtos;
using DemoCSharpAgent.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DemoCSharpAgent.Agent;

public interface IAgentService
{
    Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default);
}

public sealed class AgentService(
    IOptions<OpenAiOptions> openAiOptions,
    IConversationStore conversationStore,
    IAiToolsProvider toolsProvider,
    IServiceProvider serviceProvider,
    ILogger<AgentService> logger) : IAgentService
{
    private const string SystemPrompt =
        "You are a helpful assistant. Use the available tools when they help answer the user.";

    private readonly OpenAiOptions _openAiOptions = openAiOptions.Value;
    private readonly IConversationStore _conversationStore = conversationStore;
    private readonly IAiToolsProvider _toolsProvider = toolsProvider;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<AgentService> _logger = logger;
    private ChatClientAgent? _agent;

    private ChatClientAgent GetOrCreateAgent()
    {
        if (_agent is not null)
            return _agent;

        var tools = _toolsProvider.Tools;
        _logger.LogDebug(
            "Registered {ToolCount} tools: {ToolNames}",
            tools.Count,
            string.Join(", ", tools.Select(t => t.Name)));

        var openAiClient = new OpenAIClient(_openAiOptions.ApiKey);
        var chatClient = openAiClient.GetChatClient(_openAiOptions.Model);

        _agent = chatClient.AsAIAgent(
            instructions: SystemPrompt,
            name: null,
            description: null,
            tools: tools,
            clientFactory: null,
            loggerFactory: null,
            services: _serviceProvider);

        return _agent;
    }

    public async Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString("N");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_openAiOptions.TimeoutSeconds));
        var timeoutToken = cts.Token;

        try
        {
            var agent = GetOrCreateAgent();

            var session = _conversationStore.GetThread(conversationId);
            if (session == null)
            {
                session = await agent.CreateSessionAsync(timeoutToken);
                _conversationStore.SaveThread(conversationId, session);
            }

            var runOptions = new ChatClientAgentRunOptions();
            var result = await agent.RunAsync(request.Message, session, runOptions, timeoutToken);

            var responseText = result.Text ?? "I apologize, but I wasn't able to generate a response.";

            _logger.LogInformation(
                "Agent completed. Response: {Response}",
                responseText);

            return new ChatResponse
            {
                Answer = responseText,
                ConversationId = conversationId,

            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            throw;
        }
    }
}
