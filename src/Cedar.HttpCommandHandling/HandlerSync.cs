namespace Cedar.HttpCommandHandling
{
    public delegate void HandlerSync<TMessage>(TMessage message)
        where TMessage : class;
}