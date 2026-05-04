using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace DemoCSharpAgent.Tools;

public static class ToolRegistry
{
    public static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var weatherService = sp.GetRequiredService<IWeatherService>();

        var getWeatherFn = typeof(IWeatherService)
                 .GetMethod(nameof(IWeatherService.GetWeatherInCity),
                            [typeof(string), typeof(CancellationToken)])!;

        yield return AIFunctionFactory.Create(
            getWeatherFn,
            weatherService,
            new AIFunctionFactoryOptions
            {
                Name = "get_weather",
                Description = "Get the current weather descriptions in a specified city",
            });


        var emailService = sp.GetRequiredService<EmailService>();

        var emailFriendFn = typeof(EmailService)
                 .GetMethod(nameof(EmailService.EmailFriend),
                            [typeof(string), typeof(string)])!;

        yield return AIFunctionFactory.Create(
            emailFriendFn,
            emailService,
            new AIFunctionFactoryOptions
            {
                Name = "email_friend",
                Description = "Sends an email to my friend with this name",
            });

    }
}
