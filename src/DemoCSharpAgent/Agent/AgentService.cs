using DemoCSharpAgent.Configuration;
using DemoCSharpAgent.Dtos;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemoCSharpAgent.Agent;

public interface IAgentService
{
    Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default);
}

public sealed class AgentService(
    IOptions<OpenAiOptions> openAiOptions,
    IConversationStore conversationStore,
    ChatClientAgent agent,
    ILogger<AgentService> logger) : IAgentService
{
    private readonly OpenAiOptions _openAiOptions = openAiOptions.Value;
    private readonly IConversationStore _conversationStore = conversationStore;
    private readonly ChatClientAgent _agent = agent;
    private readonly ILogger<AgentService> _logger = logger;

    public async Task<ChatResponse> ProcessAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString("N");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_openAiOptions.TimeoutSeconds));
        var timeoutToken = cts.Token;

        try
        {
            var session = _conversationStore.GetThread(conversationId);
            if (session == null)
            {
                session = await _agent.CreateSessionAsync(timeoutToken);
                _conversationStore.SaveThread(conversationId, session);
            }

            var runOptions = new ChatClientAgentRunOptions();
            var result = await _agent.RunAsync(request.Message, session, runOptions, timeoutToken);

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
