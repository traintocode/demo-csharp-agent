using Microsoft.Extensions.AI;

namespace DemoCSharpAgent.Tools;

public interface IAiToolsProvider
{
    IList<AITool> Tools { get; }
}

public sealed class AiToolsProvider : IAiToolsProvider
{
    public IList<AITool> Tools { get; }

    public AiToolsProvider(IWeatherService weatherService, EmailService emailService)
    {
        var getWeatherFn = typeof(IWeatherService)
            .GetMethod(
                nameof(IWeatherService.GetWeatherInCity),
                [typeof(string), typeof(CancellationToken)])!;

        var emailFriendFn = typeof(EmailService)
            .GetMethod(
                nameof(EmailService.EmailFriend),
                [typeof(string), typeof(string)])!;

        Tools = new List<AITool>
        {
            AIFunctionFactory.Create(
                getWeatherFn,
                weatherService,
                new AIFunctionFactoryOptions
                {
                    Name = "get_weather",
                    Description = "Get the current weather descriptions in a specified city",
                }),
            AIFunctionFactory.Create(
                emailFriendFn,
                emailService,
                new AIFunctionFactoryOptions
                {
                    Name = "email_friend",
                    Description = "Sends an email to my friend with this name",
                }),
        };
    }
}
