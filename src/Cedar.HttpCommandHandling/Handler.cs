namespace Cedar.HttpCommandHandling
{
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task Handler<TMessage>(TMessage message, CancellationToken ct)
        where TMessage: class;
}