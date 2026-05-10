using DemoCSharpAgent.Agent;
using DemoCSharpAgent.Cli;
using DemoCSharpAgent.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDemoCSharpAgent(builder.Configuration);

using var host = builder.Build();

using var scope = host.Services.CreateScope();
var agentService = scope.ServiceProvider.GetRequiredService<IAgentService>();

var chat = new InteractiveChat(agentService);
await chat.RunAsync(CancellationToken.None);
