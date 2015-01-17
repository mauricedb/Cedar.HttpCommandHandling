namespace Cedar.HttpCommandHandling
{
    // ReSharper disable once TypeParameterCanBeVariant
    public delegate void HandlerSync<TMessage>(TMessage message)
        where TMessage : class;
}