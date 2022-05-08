using System;
using FluentAssertions;
using Velocity.API.Models;
using Xunit;

namespace Velocity.API.Tests.Models;

public class ErrorResponseTests
{
    [Theory]
    [InlineData("Generic exception message"), InlineData("")]
    public void ErrorResponse_Constructs_FromException(string message)
    {
        var e = new Exception(message);

        var errorResponse = new ErrorResponse(e);

        errorResponse.Should().NotBeNull();
        errorResponse.Message.Should().Be(message);
    }
}