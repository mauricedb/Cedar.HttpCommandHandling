namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;

    internal class CommandHandlerRegistration
    {
        internal CommandHandlerRegistration(Type messageType, Type registrationType, object handlerInstance)
        {
            MessageType = messageType;
            RegistrationType = registrationType;
            HandlerInstance = handlerInstance;
        }

        internal static IEqualityComparer<CommandHandlerRegistration> MessageTypeComparer
            { get; } = new MessageTypeEqualityComparer();

        public Type RegistrationType { get; }

        public Type MessageType { get; }

        public object HandlerInstance { get; }

        private sealed class MessageTypeEqualityComparer : IEqualityComparer<CommandHandlerRegistration>
        {
            public bool Equals(CommandHandlerRegistration x, CommandHandlerRegistration y)
            {
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                if(ReferenceEquals(x, null))
                {
                    return false;
                }
                if(ReferenceEquals(y, null))
                {
                    return false;
                }
                if(x.GetType() != y.GetType())
                {
                    return false;
                }
                return x.MessageType == y.MessageType;
            }

            public int GetHashCode(CommandHandlerRegistration obj)
            {
                return obj.MessageType.GetHashCode();
            }
        }
    }
}