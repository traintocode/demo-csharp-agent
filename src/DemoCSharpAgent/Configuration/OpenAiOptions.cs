namespace DemoCSharpAgent.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public required string ApiKey { get; set; }

    public string Model { get; set; } = "gpt-5.4-nano";

    public string SystemPrompt { get; set; } =
        "You are a helpful assistant. Use the available tools when they help answer the user.";

    public int TimeoutSeconds { get; set; } = 120;

}
