namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class CommandHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;
        private readonly List<object> _receivedCommands = new List<object>();

        public CommandHandlingFixture()
        {
            var module = CreateCommandHandlerModule();
            var handlerResolver = new CommandHandlerResolver(module);
            var commandHandlingSettings = new CommandHandlingSettings(handlerResolver)
            {
                MapProblemDetailsFromException = CreateProblemDetails
            };

            _midFunc = CommandHandlingMiddleware.HandleCommands(commandHandlingSettings);
        }

        public List<object> ReceivedCommands
        {
            get { return _receivedCommands; }
        }

        private CommandHandlerModule CreateCommandHandlerModule()
        {
            var module = new CommandHandlerModule();

            module.For<Command>()
                .Handle(commandMessage => { _receivedCommands.Add(commandMessage); });

            module.For<CommandThatThrowsStandardException>()
                .Handle(_ => { throw new InvalidOperationException(); });

            module.For<CommandThatThrowsProblemDetailsException>()
                .Handle((_, __) =>
                {
                    var problemDetails = new HttpProblemDetails
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Type = "http://localhost/type",
                        Detail = "You done goof'd",
                        Instance = "http://localhost/errors/1",
                        Title = "Jimmies Ruslted"
                    };
                    throw new HttpProblemDetailsException<HttpProblemDetails>(problemDetails);
                });

            module.For<CommandThatThrowsMappedException>()
               .Handle((_, __) => { throw new ApplicationException("Mapped application exception"); });

            module.For<CommandThatThrowsCustomProblemDetailsException>()
                .Handle((_, __) =>
                {
                    var problemDetails = new CustomHttpProblemDetails()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Type = "http://localhost/type",
                        Detail = "You done goof'd",
                        Instance = "http://localhost/errors/1",
                        Title = "Jimmies Ruslted",
                        Name = "Damo"
                    };
                    throw new CustomProblemDetailsException(problemDetails);
                });

            return module;
        }

        private static HttpProblemDetails CreateProblemDetails(Exception ex)
        {
            var applicationExcepion = ex as ApplicationException;
            if(applicationExcepion != null)
            {
                return new HttpProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Application Exception",
                    Detail = applicationExcepion.Message,
                    Type = "urn:ApplicationException"
                };
            }
            return null;
        }

        public HttpClient CreateHttpClient()
        {
            return _midFunc.CreateEmbeddedClient();
        }
    }

    public class Command { }

    public class CommandThatThrowsStandardException { }

    public class CommandThatThrowsProblemDetailsException { }

    public class CommandThatThrowsMappedException { }

    public class CommandThatThrowsCustomProblemDetailsException { }

    public class CustomHttpProblemDetails : HttpProblemDetails
    {
        public string Name { get; set; }
    }

    public class CustomProblemDetailsException : HttpProblemDetailsException<CustomHttpProblemDetails>
    {
        public CustomProblemDetailsException(CustomHttpProblemDetails problemDetails) 
            : base(problemDetails)
        {}
    }
}