using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Dtos;
using DemoCSharpAgent.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DemoCSharpAgent.Agent;

public interface IAgentService
{
    Task<Dtos.ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default);
}

public sealed class AgentService(
    IOptions<OpenAiOptions> openAiOptions,
    IConversationStore conversationStore,
    IServiceProvider serviceProvider,
    ILogger<AgentService> logger) : IAgentService
{
    private readonly OpenAiOptions _openAiOptions = openAiOptions.Value;
    private readonly IConversationStore _conversationStore = conversationStore;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<AgentService> _logger = logger;
    private AIAgent? _agent;

    private AIAgent GetOrCreateAgent()
    {
        if (_agent is not null)
            return _agent;

        var tools = _serviceProvider.GetTools().ToArray();
        _logger.LogDebug(
            "Registered {ToolCount} tools: {ToolNames}",
            tools.Length,
            string.Join(", ", tools.Select(t => t.Name)));

        var openAiClient = new OpenAIClient(_openAiOptions.ApiKey);
        var chatClient = openAiClient.GetChatClient(_openAiOptions.Model);

        _agent = chatClient.CreateAIAgent(
            instructions: _openAiOptions.SystemPrompt,
            tools: tools);

        return _agent;
    }

    public async Task<Dtos.ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString("N");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_openAiOptions.TimeoutSeconds));
        var timeoutToken = cts.Token;

        try
        {
            var agent = GetOrCreateAgent();

            var thread = _conversationStore.GetThread(conversationId);
            if (thread == null)
            {
                thread = agent.GetNewThread();
                _conversationStore.SaveThread(conversationId, thread);
            }

            var runOptions = new AgentRunOptions();
            var result = await agent.RunAsync(request.Message, thread, options: runOptions, cancellationToken: timeoutToken);

            var responseText = result.Text ?? "I apologize, but I wasn't able to generate a response.";

            _logger.LogInformation(
                "Agent completed. Response: {Response}",
                responseText);

            return new Dtos.ChatResponse
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
