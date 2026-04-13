using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using MyDDD.Template.Api.Middleware;
using MyDDD.Template.Application.Exceptions;
using MyDDD.Template.Domain.Primitives;
using Xunit;

namespace MyDDD.Template.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock;
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        _envMock = new Mock<IHostEnvironment>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object, _problemDetailsServiceMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return400_When_ValidationExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var errors = new[] { new MyError("Code", "Message", ErrorType.Validation) };
        var validationResult = ValidationResult.WithErrors(errors);
        var exception = new ValidationException(validationResult);

        _envMock.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        _problemDetailsServiceMock.Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, default);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _problemDetailsServiceMock.Verify(x => x.WriteAsync(It.Is<ProblemDetailsContext>(c =>
            c.ProblemDetails.Status == StatusCodes.Status400BadRequest &&
            c.ProblemDetails.Title == "Validation Error" &&
            c.ProblemDetails.Extensions.ContainsKey("errors"))), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return401_When_UnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new UnauthorizedAccessException();
        _envMock.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        _problemDetailsServiceMock.Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, default);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _problemDetailsServiceMock.Verify(x => x.WriteAsync(It.Is<ProblemDetailsContext>(c =>
            c.ProblemDetails.Status == StatusCodes.Status401Unauthorized)), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_Should_IncludeDebugException_When_InDevelopment()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("Critical error");
        _envMock.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        _problemDetailsServiceMock.Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _handler.TryHandleAsync(context, exception, default);

        // Assert
        _problemDetailsServiceMock.Verify(x => x.WriteAsync(It.Is<ProblemDetailsContext>(c =>
            c.ProblemDetails.Extensions.ContainsKey("debug_exception") &&
            c.ProblemDetails.Detail == "Critical error")), Times.Once);
    }
}
