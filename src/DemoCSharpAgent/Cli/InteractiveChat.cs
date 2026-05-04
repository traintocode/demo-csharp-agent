using DemoCSharpAgent.Agent;
using DemoCSharpAgent.Dtos;

namespace DemoCSharpAgent.Cli;

public sealed class InteractiveChat
{
    private readonly IAgentService _agent;
    private string _conversationId;

    public InteractiveChat(IAgentService agent)
    {
        _agent = agent;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Demo C# Agent");
        Console.WriteLine("Empty line to exit.");
        Console.WriteLine(
            $"(pid {Environment.ProcessId}) Type here--not the \"build Task\" terminal from compile.");
        Console.WriteLine();

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("> ");
            string input;
            try
            {
                input = Console.ReadLine() ?? string.Empty;
            }
            catch (IOException)
            {
                break;
            }
            finally
            {
                Console.ResetColor();
            }

            if (string.IsNullOrWhiteSpace(input))
                break;

            try
            {
                var response = await _agent.ProcessAsync(
                    new ChatRequest
                    {
                        Message = input.Trim(),
                        ConversationId = _conversationId,
                    },
                    cancellationToken);

                _conversationId = response.ConversationId;
                Console.WriteLine(response.Answer);
                Console.WriteLine();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Request timed out or was cancelled.");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine();
            }
        }
    }
}
