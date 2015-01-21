namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Cedar.Annotations;
    using Cedar.HttpCommandHandling.TypeResolution;

    internal class CommandHandlerController : ApiController
    {
        internal static readonly MethodInfo DispatchCommandMethodInfo = typeof(CommandHandlerController)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly CommandHandlingSettings _settings;

        public CommandHandlerController(CommandHandlingSettings settings)
        {
            _settings = settings;
        }

        [Route("{commandId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCommand(Guid commandId, CancellationToken cancellationToken)
        {
            IParsedMediaType parsedMediaType = ParseMediaType();
            Type commandType = ResolveCommandType(parsedMediaType);
            if(!string.Equals(parsedMediaType.SerializationType, "json", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            object command = await DeserializeCommand(commandType);
            var user = (User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            MethodInfo dispatchCommandMethod = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());

            Func<Task> func = async () => await ((Task)dispatchCommandMethod.Invoke(null,
               new[]
                {
                    _settings.HandlerResolver, commandId, user, command, cancellationToken
                })).NotOnCapturedContext();

            await func();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private IParsedMediaType ParseMediaType()
        {
            string mediaType = Request.Content.Headers.ContentType.MediaType;
            IParsedMediaType parsedMediaType = _settings.ParseMediaType(mediaType);
            if (parsedMediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            return parsedMediaType;
        }

        private Type ResolveCommandType(IParsedMediaType parsedMediaType)
        {
            Type commandType = _settings.ResolveCommandType(parsedMediaType.TypeName, parsedMediaType.Version);
            if (commandType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            return commandType;
        }

        private async Task<object> DeserializeCommand(Type commandType)
        {
            var commandString = await Request.Content.ReadAsStringAsync();
            return SimpleJson.DeserializeObject(commandString, commandType, CommandClient.JsonSerializerStrategy);
        }

        [UsedImplicitly]
        private static async Task DispatchCommand<TCommand>(
            ICommandHandlerResolver handlerResolver,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            await handlerResolver.Resolve<TCommand>()(commandMessage, cancellationToken);
        }
    }
}