namespace DemoCSharpAgent.Tools;

public class EmailService()
{
    public Task EmailFriend(string friendName, string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"Emailing {friendName} with: {message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

