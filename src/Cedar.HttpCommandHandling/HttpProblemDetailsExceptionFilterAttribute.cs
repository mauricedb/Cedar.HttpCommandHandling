namespace Cedar.HttpCommandHandling
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Filters;

    /// <summary>
    ///     Will return a HttpProblemDetails json entity if the HttpProblemDetailsException is thrown
    ///     or an exception can be mapped to a HttpProblemDetails type, and Accept-Type is application/problem+json
    /// </summary>
    internal class HttpProblemDetailsExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly MapProblemDetailsFromException _mapProblemDetailsFromException;

        internal HttpProblemDetailsExceptionFilterAttribute(MapProblemDetailsFromException mapProblemDetailsFromException)
        {
            _mapProblemDetailsFromException = mapProblemDetailsFromException;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var httpProblemDetailsException = actionExecutedContext.Exception as IHttpProblemDetailException;
            HttpProblemDetails problemDetails = httpProblemDetailsException != null 
                ? httpProblemDetailsException.ProblemDetails
                : _mapProblemDetailsFromException(actionExecutedContext.Exception); // may return null if no mapping from exceptions to problem details has been setup.

            if(problemDetails == null)
            {
                // No problem details to serialize, just a standard intenal server error.
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return;
            }

            var config = actionExecutedContext.ActionContext.ControllerContext.Configuration;
            var negotiator = config.Services.GetContentNegotiator();
            var formatters = config.Formatters;
            var problemDetailsType = problemDetails.GetType();

            ContentNegotiationResult result = negotiator.Negotiate(
                problemDetailsType,
                actionExecutedContext.Request,
                formatters);

            if (result == null) // When the client didn't have appropriate Accept-Type (application/problem+json)
            {
                base.OnException(actionExecutedContext);
                return;
            }

            var status = problemDetails.Status;
            var response = new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new ObjectContent(
                    problemDetailsType,
                    problemDetails,
                    result.Formatter,
                    result.MediaType)
            };

            // If a HttpProblemDetailsException or derived was specifically thrown use that type
            // otherwise, because of exception -> problem details mapping, we use HttpProblemDetailsException<>
            var exceptionTypeName = httpProblemDetailsException != null
                ? actionExecutedContext.Exception.GetType().AssemblyQualifiedName
                : typeof(HttpProblemDetailsException<>).MakeGenericType(problemDetailsType).AssemblyQualifiedName;

            // .NET Client will use these custom headers to deserialize and activate the correct types
            response.Headers.Add(CommandClient.HttpProblemDetailsExceptionClrType, exceptionTypeName);
            response.Headers.Add(CommandClient.HttpProblemDetailsClrType, problemDetailsType.AssemblyQualifiedName);
            
            actionExecutedContext.Response = response;
        }
    }
}