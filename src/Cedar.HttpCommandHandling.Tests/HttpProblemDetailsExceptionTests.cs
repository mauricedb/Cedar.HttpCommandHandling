/*namespace Cedar.HttpCommandHandling
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class HttpProblemDetailsExceptionTests
    {
        [Fact]
        public void Create_with_null_details_should_throw()
        {
            Action act = () => new HttpProblemDetailsException((HttpProblemDetails)null);

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}*/