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
namespace Cedar.HttpCommandHandling.Example.Exceptions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling;
    using Xunit;

    public class CommandThatThrowsStandardException { }

    public class CommandThatThrowsProblemDetailsException { }

    public class CommandThatThrowsMappedException { }

    public class CommandThatThrowsCustomProblemDetailsException { }

    public class CommandModule : CommandHandlerModule
    {
        // 1. Example of handlers thowing the various exceptions
        public CommandModule()
        {
            For<CommandThatThrowsStandardException>()
                .Handle(_ =>
                {
                    // 2. Standard exception that will result in a 404
                    throw new Exception();
                });

            For<CommandThatThrowsProblemDetailsException>()
                .Handle(_ =>
                {
                    // 3. Throwing an explicit HttpProblemDetailsException
                    var details = new HttpProblemDetails { Status = (int)HttpStatusCode.NotImplemented };
                    throw new HttpProblemDetailsException<HttpProblemDetails>(details);
                });

            For<CommandThatThrowsMappedException>()
                .Handle(_ =>
                {
                    // 4. This exception type is mapped to an HttpProblemDetails in the settings.
                    throw new InvalidOperationException("jimms rustled");
                });

            For<CommandThatThrowsCustomProblemDetailsException>()
                .Handle(_ =>
                {
                    // 5. Throwing custom problem details exception.
                    var problemDetails = new CustomHttpProblemDetails
                    {
                        Status = (int)HttpStatusCode.NotImplemented,
                        Name = "Damo"
                    };
                    throw new CustomHttpProblemDetailsException(problemDetails);
                });
        }
    }

    public class CustomHttpProblemDetails : HttpProblemDetails
    {
        // 6. Custom problem details should be easily (de-)serializable
        public string Name { get; set; }
    }

    public class CustomHttpProblemDetailsException : HttpProblemDetailsException<CustomHttpProblemDetails>
    {
        // 7. Custom problem details exception must contain this ctor with a single param
        //    that takes the custom problem details.
        public CustomHttpProblemDetailsException(CustomHttpProblemDetails problemDetails) 
            : base(problemDetails)
        {}
    }

    public class CommandModuleTests
    {
        // 8. Our exception -> ProblemDetails map.
        private static readonly MapProblemDetailsFromException MapExceptionToProblemDetails = exception =>
        {
            var ex = exception as InvalidOperationException;
            if(ex != null)
            {
                return new HttpProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = ex.Message
                };
            }
            // 9. Return null if no map exists. This will fall back to
            //    standard exeption handling behaviour
            return null;
        };

        [Fact]
        public async Task Example_exception_handling()
        {
            var resolver = new CommandHandlerResolver(new CommandModule());
            var settings = new CommandHandlingSettings(resolver)
            {
                // 10. Specify the exception -> HttpProblemDetails mapper here
                MapProblemDetailsFromException = MapExceptionToProblemDetails
            };
            var middleware = CommandHandlingMiddleware.HandleCommands(settings);

            using(HttpClient client = middleware.CreateEmbeddedClient())
            {
                // 11. Handling standard exceptions.
                try
                {
                    await client.PutCommand(new CommandThatThrowsStandardException(), Guid.NewGuid());
                }
                catch(HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // 12. Handling explicit HttpProblemDetailsExceptions
                try
                {
                    await client.PutCommand(new CommandThatThrowsProblemDetailsException(), Guid.NewGuid());
                }
                catch (HttpProblemDetailsException<HttpProblemDetails> ex)
                {
                    Console.WriteLine(ex.ProblemDetails.Detail);
                    Console.WriteLine(ex.ProblemDetails.Status);
                }

                // 13. Handling mapped exceptions, same as #6
                try
                {
                    await client.PutCommand(new CommandThatThrowsMappedException(), Guid.NewGuid());
                }
                catch (HttpProblemDetailsException<HttpProblemDetails> ex)
                {
                    Console.WriteLine(ex.ProblemDetails.Detail);
                    Console.WriteLine(ex.ProblemDetails.Status);
                }

                // 14. Handling custom HttpProblemDetailExceptions
                try
                {
                    await client.PutCommand(new CommandThatThrowsCustomProblemDetailsException(), Guid.NewGuid());
                }
                catch (CustomHttpProblemDetailsException ex)
                {
                    Console.WriteLine(ex.ProblemDetails.Detail);
                    Console.WriteLine(ex.ProblemDetails.Status);
                    Console.WriteLine(ex.ProblemDetails.Name);
                }
            }
        }
    }
}
