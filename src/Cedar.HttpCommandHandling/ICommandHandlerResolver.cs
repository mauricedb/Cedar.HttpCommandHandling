namespace Cedar.HttpCommandHandling
{
    public interface ICommandHandlerResolver
    {
        Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class;
    }
}