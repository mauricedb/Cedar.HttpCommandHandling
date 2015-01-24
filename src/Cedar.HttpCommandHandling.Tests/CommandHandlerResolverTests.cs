namespace Cedar.HttpCommandHandling
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlerResolverTests
    {
        [Fact]
        public void Can_resolve_handler()
        {
            var module = new TestCommandHandlerModule();
            var resolver = new CommandHandlerResolver(module);

            Handler<CommandMessage<Command>> handler = resolver.Resolve<Command>();

            handler.Should().NotBeNull();
        }

        private class TestCommandHandlerModule : CommandHandlerModule
        {
            public TestCommandHandlerModule()
            {
                For<Command>()
                    .Handle((_, __) => Task.FromResult(0));
            }
        }
    }
}
