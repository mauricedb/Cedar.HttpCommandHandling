namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net;
    using FluentAssertions;
    using Xunit;

    public class HttpProblemDetailsTests
    {
        [Fact]
        public void Can_create_exception_with_status_code()
        {
            var sut = new HttpProblemDetails { Status = (int)HttpStatusCode.BadRequest };

            sut.Status.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public void When_setting_type_with_a_relative_uri_should_throw()
        {
            var sut = new HttpProblemDetails { Status = (int)HttpStatusCode.BadRequest };

            Action act = () => sut.Type = "/relative";

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void When_setting_instance_with_a_relative_uri_should_throw()
        {
            var sut = new HttpProblemDetails { Status = (int)HttpStatusCode.BadRequest };

            Action act = () => sut.Instance = "/relative";

            act.ShouldThrow<InvalidOperationException>();
        }
    }
}