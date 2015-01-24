namespace Cedar.HttpCommandHandling
{
    internal interface IHttpProblemDetailException
    {
        HttpProblemDetails ProblemDetails { get; }
    }
}