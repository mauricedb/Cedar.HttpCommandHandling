/*
 * This show a mechanism define common pipes to share operations across
 * handlers. Examples of such include logging and authorization.
 */

// ReSharper disable once CheckNamespace
namespace Cedar.HttpCommandHandling.Example.Commands.PipingExtensions
{
    using System;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;

    public class Command1
    {}

    // 1. Second command that will share a pipe
    public class Command2
    {}

    // 2. Define an extensions class to define reusable pipes
    public static class PipingExtensions
    {
        // 3. Example pipeline that ensures the user is authorized by 
        // checking their role.
        internal static ICommandHandlerBuilder<CommandMessage<TMessage>> RequireRole<TMessage>(
            this ICommandHandlerBuilder<CommandMessage<TMessage>> commandHandlerBuilder,
            string role)
        {
            return commandHandlerBuilder.Pipe(next => (commandMessage, ct) =>
            {
                if(!commandMessage.User.IsInRole(role))
                {
                    throw new InvalidOperationException("Not Authorized");
                }
                return next(commandMessage, ct);
            });
        }
    }

    public class MyCommandModule : CommandHandlerModule
    {
        public MyCommandModule()
        {
            For<Command1>()
                .RequireRole("admin") // 4. Use the extension
                .Handle((commandMessage, ct) => Task.FromResult(0));

            For<Command2>()
                .RequireRole("user")
                .Handle((commandMessage, ct) => Task.FromResult(0));
        }
    }
}
