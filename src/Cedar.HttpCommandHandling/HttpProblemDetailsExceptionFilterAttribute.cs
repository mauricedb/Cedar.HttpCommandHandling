namespace Cedar.HttpCommandHandling
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Filters;

    internal class HttpProblemDetailsExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly CreateProblemDetails _createProblemDetails;

        internal HttpProblemDetailsExceptionFilterAttribute(CreateProblemDetails createProblemDetails)
        {
            _createProblemDetails = createProblemDetails;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var httpProblemDetailsException = actionExecutedContext.Exception as HttpProblemDetailsException;
            HttpProblemDetails problemDetails = httpProblemDetailsException != null 
                ? httpProblemDetailsException.ProblemDetails
                : _createProblemDetails(actionExecutedContext.Exception); // may return null if no mapping from exceptions to problem details has been setup.

            if(problemDetails == null)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return;
            }

            var config = actionExecutedContext.ActionContext.ControllerContext.Configuration;
            var negotiator = config.Services.GetContentNegotiator();
            var formatters = config.Formatters;
            var dtoType = typeof(HttpProblemDetails);

            ContentNegotiationResult result = negotiator.Negotiate(
                dtoType,
                actionExecutedContext.Request,
                formatters);

            if (result == null)
            {
                base.OnException(actionExecutedContext);
                return;
            }

            var status = problemDetails.Status;// ?? HttpStatusCode.InternalServerError;
            var response = new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new ObjectContent(
                    dtoType,
                    problemDetails,
                    result.Formatter,
                    result.MediaType)
            };
            actionExecutedContext.Response = response;
        }
    }
}