namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net;

    /// <summary>
    ///     An exception that represents a Problem Detail for HTTP APIs
    ///     https://datatracker.ietf.org/doc/draft-ietf-appsawg-http-problem/
    /// </summary>
    public class HttpProblemDetailsException : HttpProblemDetailsException<HttpProblemDetails>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpProblemDetailsException"/> class.
        /// </summary>
        /// <param name="status">
        ///     The HttpStatusCode. You can set more values via the ProblemDetails property.
        /// </param>
        public HttpProblemDetailsException(HttpStatusCode status)
            : this(new HttpProblemDetails { Status = (int)status })
        {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpProblemDetailsException"/> class.
        /// </summary>
        /// <param name="problemDetails">
        ///     An instance of <see cref="ProblemDetails"/>
        /// </param>
        public HttpProblemDetailsException(HttpProblemDetails problemDetails)
            : base(problemDetails)
        {}
    }

    public class HttpProblemDetailsException<T> : Exception
        where T : HttpProblemDetails
    {
        private readonly T _problemDetails;

        public HttpProblemDetailsException(T problemDetails)
        {
            if (problemDetails == null)
            {
                throw new ArgumentNullException("problemDetails");
            }
            _problemDetails = problemDetails;
        }

        public T ProblemDetails
        {
            get { return _problemDetails; }
        }
    }
}