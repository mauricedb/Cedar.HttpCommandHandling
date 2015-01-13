/*
 * Simplest example of wiring up a command, it's handler and the command
 * handling middleware
 */

// ReSharper disable once CheckNamespace
namespace Cedar.HttpCommandHandling.Example.Commands.Simple
{
    using System;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;
    using Microsoft.Owin.Hosting;

    // 1. Simple commands.
    public class CommandSyncHandler
    {}

    public class CommandAsyncHandler
    { }


    // 2. A service the command handlers depends.
    public interface IFoo
    {
        void Bar();

        Task BarAsync();
    }

    // 3. Define your handlers.
    public class CommandModule : CommandHandlerModule
    {
        // Modules and handlers are singletons. Services that need to activated per request
        // should be injected as factory methods / funcs.
        public CommandModule(Func<IFoo> getFoo)
        {
            For<CommandSyncHandler>()
                .Handle(commandMessage => getFoo().Bar());

            For<CommandAsyncHandler>()
                .Handle(async (commandMessage, ct) =>
                {
                    var foo = getFoo();
                    await foo.BarAsync();
                });
        }
    }

    // 4. Host it.
    public class Program
    {
        static void Main()
        {
            Func<IFoo> getFoo = () => new DummyFoo();
            var resolver = new CommandHandlerResolver(new CommandModule(getFoo));
            var settings = new CommandHandlingSettings(resolver);
            var commandHandlingMiddleware = CommandHandlingMiddleware.HandleCommands(settings);

            // 5. Add the middleware to your owin pipeline
            using(WebApp.Start("http://localhost:8080",
                app =>
                {
                    app.Use(commandHandlingMiddleware);
                }))
            {
                Console.WriteLine("Press any key");
            }
        }

        private class DummyFoo : IFoo
        {
            public Task BarAsync()
            {
                throw new NotImplementedException();
            }

            void IFoo.Bar()
            {
                throw new NotImplementedException();
            }
        }
    }
}
