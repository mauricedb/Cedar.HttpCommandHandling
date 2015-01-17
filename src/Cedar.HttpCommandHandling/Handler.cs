namespace Cedar.HttpCommandHandling
{
    using System.Threading;
    using System.Threading.Tasks;

    // ReSharper disable once TypeParameterCanBeVariant
    public delegate Task Handler<TMessage>(TMessage message, CancellationToken ct)
        where TMessage: class;
}