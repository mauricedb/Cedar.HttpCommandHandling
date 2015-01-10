namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;

    public class CommandHandlerModule
    {
        private readonly HashSet<CommandHandlerRegistration> _handlerRegistrations
            = new HashSet<CommandHandlerRegistration>(CommandHandlerRegistration.MessageTypeComparer);

        internal HashSet<CommandHandlerRegistration> HandlerRegistrations
        {
            get { return _handlerRegistrations; }
        }

        public ICommandHandlerBuilder<CommandMessage<TCommand>> For<TCommand>()
            where TCommand : class
        {
            return new CommandHandlerBuilder<TCommand>(handlerRegistration =>
            {
                if(!_handlerRegistrations.Add(handlerRegistration))
                {
                    throw new InvalidOperationException(
                        "Attempt to register multiple handlers for command type {0}".FormatWith(typeof(TCommand)));
                }
            });
        }

        private class CommandHandlerBuilder<TCommand> : ICommandHandlerBuilder<CommandMessage<TCommand>>
            where TCommand : class
        {
            private readonly Stack<Pipe<CommandMessage<TCommand>>> _pipes = new Stack<Pipe<CommandMessage<TCommand>>>();
            private readonly Action<CommandHandlerRegistration> _registerHandler;

            internal CommandHandlerBuilder(Action<CommandHandlerRegistration> registerHandler)
            {
                _registerHandler = registerHandler;
            }

            public ICommandHandlerBuilder<CommandMessage<TCommand>> Pipe(Pipe<CommandMessage<TCommand>> pipe)
            {
                _pipes.Push(pipe);
                return this;
            }

            public Handler<CommandMessage<TCommand>> Handle(Handler<CommandMessage<TCommand>> handler)
            {
                while(_pipes.Count > 0)
                {
                    var pipe = _pipes.Pop();
                    handler = pipe(handler);
                }

                var registrationType = typeof(Handler<CommandMessage<TCommand>>);

                _registerHandler(new CommandHandlerRegistration(
                    typeof(TCommand),
                    registrationType,
                    handler));
                return handler;
            }
        }
    }
}