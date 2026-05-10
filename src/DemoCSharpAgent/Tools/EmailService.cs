namespace DemoCSharpAgent.Tools;

using System.ComponentModel;

public class EmailService()
{
    public Task EmailFriend(
        [Description("The friend's name to address the email to.")]
        string friendName,
        [Description("The message body to send.")]
        string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"Emailing {friendName} with: {message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

