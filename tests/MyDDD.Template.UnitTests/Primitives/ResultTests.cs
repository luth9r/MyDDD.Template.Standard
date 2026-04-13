using FluentAssertions;
using MyDDD.Template.Domain.Primitives;
using Xunit;

namespace MyDDD.Template.UnitTests.Primitives;

public class ResultTests
{
    [Fact]
    public void Success_Should_SetIsSuccessToTrueAndErrorToNone()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(MyError.None);
    }

    [Fact]
    public void Success_WithValue_Should_SetPropertiesAndValue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_Should_SetIsSuccessToFalseAndError()
    {
        // Arrange
        var error = MyError.Validation("Test.Error", "Error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithValue_Should_SetProperties()
    {
        // Arrange
        var error = MyError.Validation("Test.Error", "Error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_Should_ThrowException_When_ResultIsFailure()
    {
        // Arrange
        var result = Result.Failure<string>(MyError.Validation("Test.Error", "Error message"));

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failure result can not be accessed.");
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnSuccess_When_ValueIsNotNull()
    {
        // Arrange
        string value = "test";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnFailure_When_ValueIsNull()
    {
        // Act
        Result<string> result = (string)null!;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MyError.NullValue);
    }

    [Fact]
    public void Constructor_Should_ThrowException_When_SuccessAndErrorProvided()
    {
        // Act
        Action act = () => _ = new TestResult(true, MyError.Validation("Code", "Message"));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid error*");
    }

    [Fact]
    public void Constructor_Should_ThrowException_When_FailureAndErrorNone()
    {
        // Act
        Action act = () => _ = new TestResult(false, MyError.None);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid error*");
    }

    private sealed class TestResult : Result
    {
        public TestResult(bool isSuccess, MyError error) : base(isSuccess, error) { }
    }
}
