using DemoCSharpAgent.Agent;
using DemoCSharpAgent.Dtos;
using Spectre.Console;

namespace DemoCSharpAgent.Cli;

public sealed class InteractiveChat
{
    private readonly IAgentService _agent;
    private string? _conversationId;

    public InteractiveChat(IAgentService agent)
    {
        _agent = agent;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Write(
            new FigletText("Agent")
                .LeftJustified()
                .Color(Color.DeepSkyBlue3));

        AnsiConsole.Write(new Rule("[bold dodgerblue1]Demo C# Agent[/]").RuleStyle("grey"));
        AnsiConsole.MarkupLine(
            "[grey]Commands:[/] [yellow]/help[/] [grey]·[/] [yellow]/new[/] [dim](new chat)[/] [grey]·[/] [yellow]/clear[/] [grey]·[/] [yellow]/exit[/]");
        AnsiConsole.WriteLine();

        while (!cancellationToken.IsCancellationRequested)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold green]You[/] [dim]>[/] ")
                    .PromptStyle(Style.Plain)
                    .AllowEmpty());

            var trimmed = input.Trim();
            if (trimmed.Length == 0)
                continue;

            switch (HandleSlashCommand(trimmed))
            {
                case SlashCommandResult.Exit:
                    AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
                    return;
                case SlashCommandResult.Handled:
                    continue;
                case SlashCommandResult.NotASlashCommand:
                    break;
            }

            await SendMessageAsync(trimmed, cancellationToken);
        }
    }

    private SlashCommandResult HandleSlashCommand(string input)
    {
        if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase)
            || input.Equals("/quit", StringComparison.OrdinalIgnoreCase))
            return SlashCommandResult.Exit;

        if (input.Equals("/clear", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.Clear();
            return SlashCommandResult.Handled;
        }

        if (input.Equals("/new", StringComparison.OrdinalIgnoreCase))
        {
            _conversationId = null;
            AnsiConsole.MarkupLine("[grey]Started a new conversation.[/]");
            return SlashCommandResult.Handled;
        }

        if (input.Equals("/help", StringComparison.OrdinalIgnoreCase))
        {
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("[cyan]Command[/]");
            table.AddColumn("[cyan]Description[/]");
            table.AddRow("[yellow]/help[/]", "Show this help");
            table.AddRow("[yellow]/new[/]", "Clear thread memory and start a new conversation");
            table.AddRow("[yellow]/clear[/]", "Clear the screen");
            table.AddRow("[yellow]/exit[/], [yellow]/quit[/]", "Exit the app");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            return SlashCommandResult.Handled;
        }

        return SlashCommandResult.NotASlashCommand;
    }

    private enum SlashCommandResult
    {
        NotASlashCommand,
        Handled,
        Exit,
    }

    private async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var request = new ChatRequest
        {
            Message = message,
            ConversationId = _conversationId,
        };

        try
        {
            ChatResponse response = null!;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots12)
                .SpinnerStyle(new Style(Color.DeepSkyBlue3))
                .StartAsync(
                    "[dim]Thinking…[/]",
                    async _ =>
                    {
                        response = await _agent.ProcessAsync(request, cancellationToken);
                    });

            _conversationId = response.ConversationId;

            var escaped = Markup.Escape(response.Answer ?? string.Empty);
            var panel = new Panel(escaped)
                .Header("[bold cyan]Assistant[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Aqua)
                .Padding(new Padding(1, 0));

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[bold red]Request timed out or was cancelled.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] [red]{Markup.Escape(ex.Message)}[/]");
        }
    }
}
