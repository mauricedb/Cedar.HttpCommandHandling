/*
 * Example of how to test a handler
 */

// ReSharper disable once CheckNamespace
namespace Cedar.HttpCommandHandling.Example.UnitTestinHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;
    using FakeItEasy;
    using Xunit;

    public class Command
    {
        public string Name { get; set; }
    }

    public interface IFoo
    {
        Task Bar(string name);
    }

    public class CommandModule : CommandHandlerModule
    {
        public CommandModule(Func<IFoo> getFoo)
        {
            For<Command>()
                .Handle(async (commandMessage, ct) =>
                {
                    var foo = getFoo();
                    await foo.Bar(commandMessage.Command.Name);
                });
        }
    }

    public class CommandModuleTests
    {
        [Fact]
        public void Command_should_call_service()
        {
            const string name = "jrustle";
            var foo = A.Fake<IFoo>();
            A.CallTo(() => foo.Bar(name));
            var sut = new CommandHandlerResolver(new CommandModule(() => foo));
            var command = new Command { Name = name };


            sut.Resolve<Command>()(
                new CommandMessage<Command>(Guid.NewGuid(), null, command),
                CancellationToken.None);

            A.CallTo(() => foo.Bar(name)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
