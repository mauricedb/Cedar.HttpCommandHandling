/*
 * Example of how to exceptions are handled.
 * 
 * You have three ways to deal with exceptions:
 * 
 * 1. Throw a standard exception.
 * 
 *    If the exception can't be mapped to a ProblemDetails (see 3 below) this will result
 *    in a status code 500 and an empty response entity.
 * 
 * 2. Specically throw a HttpProblemDetailsException.
 * 
 *    If the request Accept-Type contains a 'application/problem+json', this will result
 *    in a Problems Details json entity being returned according to IETF draft:
 *    https://datatracker.ietf.org/doc/draft-ietf-appsawg-http-problem/?include_text=1.
 *    
 *    Otherwise the response it will an empty entity with corresponding status code from
 *    the HttpProblemDetailsException.
 *    
 *    When using the .NET client this will be deserialized into HttpProblemDetailsException 
 *    on the caller side so it can be try-caught. If the response is not a 'application/problem+json'
 *    a HttpRequestException will be raised.
 *    
 *    Non .NET clients (Javascript etc) will need to deal with the JSON response entity directly.
 *    
 * 3. Map standard / custom exceptions to ProblemDetails
 * 
 *    You may be thowing custom exceptions from your domain and try-catching and throwin
 *    an HttpProblemDetailsException could seem ardous. CommandHandlingSettings, via
 *    the CreateProblemDetails delegate property, allows you to create a mapping between your
 *    exeption types and ProblemDetails.
 *    
 *    This delegate will only be invoked if the Accept-Type contains a 'application/problem+json'. 
 */

// ReSharper disable once CheckNamespace
namespace Cedar.HttpCommandHandling.Example.Commands.Exceptions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;
    using Xunit;

    public class CommandThatThrowsStandardException
    {}

    public class CommandThatThrowsProblemDetailsException
    {}

    public class CommandThatThrowsMappedException
    {}

    public class CommandModule : CommandHandlerModule
    {
        public CommandModule()
        {
            // 1. Example of handlers thowing the various exceptions
            For<CommandThatThrowsStandardException>()
                .Handle(_ =>
                {
                    throw new Exception();
                });

            For<CommandThatThrowsProblemDetailsException>()
                .Handle(_ =>
                {
                    var details = new HttpProblemDetails(HttpStatusCode.NotImplemented);
                    throw new HttpProblemDetailsException(details);
                });

            For<CommandThatThrowsMappedException>()
                .Handle(_ =>
                {
                    // 2. We'll create the map for this in the settings.
                    throw new InvalidOperationException("jimms rustled");
                });
        }
    }

    public class CommandModuleTests
    {
        // 3. Our exception -> ProblemDetails map.
        private static readonly CreateProblemDetails MapExceptionToProblemDetails = exception =>
        {
            var ex = exception as InvalidOperationException;
            if(exception != null)
            {
                return new HttpProblemDetails(HttpStatusCode.BadRequest)
                {
                    Detail = ex.Message
                };
            }
            // 4. Return null if no map exists. This will fall back to
            //    standard exeption handling behaviour
            return null;
        };

        [Fact]
        public async Task Can_invoke_command_over_http()
        {
            var resolver = new CommandHandlerResolver(new CommandModule());
            var settings = new CommandHandlingSettings(resolver)
            {
                CreateProblemDetails = MapExceptionToProblemDetails
            };
            var middleware = CommandHandlingMiddleware.HandleCommands(settings);

            using(HttpClient client = middleware.CreateEmbeddedClient())
            {

                // 5. How to handle the exceptions thrown.
                try
                {
                    await client.PutCommand(new CommandThatThrowsStandardException(), Guid.NewGuid());
                }
                catch(HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    await client.PutCommand(new CommandThatThrowsProblemDetailsException(), Guid.NewGuid());
                }
                catch (HttpProblemDetailsException ex)
                {
                    Console.WriteLine(ex.ProblemDetails.Detail);
                    Console.WriteLine(ex.ProblemDetails.Status);
                }

                try
                {
                    await client.PutCommand(new CommandThatThrowsMappedException(), Guid.NewGuid());
                }
                catch (HttpProblemDetailsException ex)
                {
                    Console.WriteLine(ex.ProblemDetails.Detail);
                    Console.WriteLine(ex.ProblemDetails.Status);
                }
            }
        }
    }
}
