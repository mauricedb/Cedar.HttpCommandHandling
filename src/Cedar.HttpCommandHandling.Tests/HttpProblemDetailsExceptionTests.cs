namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net;
    using FluentAssertions;
    using Xunit;

    public class HttpProblemDetailsExceptionTests
    {
        [Fact]
        public void Create_with_null_details_should_throw()
        {
            Action act = () => new HttpProblemDetailsException(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Can_create_with_status_code()
        {
            var sut = new HttpProblemDetailsException(HttpStatusCode.BadRequest);

            sut.ProblemDetails.Should().NotBeNull();
        }
    }
}