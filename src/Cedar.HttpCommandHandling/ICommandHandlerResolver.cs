namespace Cedar.HttpCommandHandling
{
    using Cedar.HttpCommandHandling.Handlers;

    public interface ICommandHandlerResolver
    {
        Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class;
    }
}