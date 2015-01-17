namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Security.Claims;

    public class CommandMessage<TCommand>
    {
        public CommandMessage(
            Guid commandId,
            ClaimsPrincipal user,
            TCommand command)
        {
            CommandId = commandId;
            User = user;
            Command = command;
        }

        public Guid CommandId { get; }

        public ClaimsPrincipal User { get; }

        public TCommand Command { get; }
    }
}