/*
 * Example of how to invoke the CommandHandlingMiddleware using an embedded HTTP pipline
 * 
 * This is a achieved by using OwinHttpMessageHandler (https://github.com/damianh/OwinHttpMessageHandler)
 * that allows an instance of HTTP client send an HTTP request directly to the middleware
 * without requiring an actual HTTP server, and the associated enviromental dependencies.
 * 
 * This is useful for two scenarios:
 * 
 * 1. Tests where you want to cover the entire pipline of handlers, serialization,
 *    type resolution etc.
 *    
 * 2. Invoking your "remote" HTTP API in proc. The has the advantage of using the
 *    exact same pipline as remote clients. (Disadvantage is a slower invocation than
 *    a method call)
 * 
 * Here we are showing a test.
 */

// ReSharper disable once CheckNamespace
namespace Cedar.HttpCommandHandling.Example.Commands.Embedded
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;
    using Xunit;

    public class Command
    {}

    public class CommandModule : CommandHandlerModule
    {
        public CommandModule()
        {
            For<Command>()
                .Handle((_, __) => Task.FromResult(0));
        }
    }

    public class CommandModuleTests
    {
        [Fact]
        public async Task Can_invoke_command_over_http()
        {
            // 1. Setup the middlware
            var resolver = new CommandHandlerResolver(new CommandModule());
            var settings = new CommandHandlingSettings(resolver);
            var midfunc = CommandHandlingMiddleware.HandleCommands(settings);

            // 2. Allows configuring an HttpClient to invoke the middleware directly
            var handler = new OwinHttpMessageHandler(midfunc);

            using(var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost")
            })
            {
                // 3. This is as close as you can get to simulating a real client call
                //    without needing real server. 
                await client.PutCommand(new Command(), Guid.NewGuid());
            }
        }
    }
}
