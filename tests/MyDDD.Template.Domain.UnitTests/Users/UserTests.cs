using FluentAssertions;
using MyDDD.Template.Domain.Users;
using MyDDD.Template.Domain.Users.DomainEvents;
using Xunit;

namespace MyDDD.Template.Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_Should_SetPropertiesAndRaiseEvent_When_ArgumentsAreValid()
    {
        // Arrange
        var identityId = "auth0|123456";
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(identityId, email, firstName, lastName);

        // Assert
        user.IdentityId.Should().Be(identityId);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Id.Should().NotBeEmpty();

        var domainEvent = user.GetDomainEvents().OfType<UserRegisteredDomainEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.Id.Should().Be(user.Id);
        domainEvent.IdentityId.Should().Be(identityId);
        domainEvent.Email.Should().Be(email);
    }

    [Fact]
    public void Create_Should_SetExplicitId_When_Provided()
    {
        // Arrange
        var explicitId = Guid.NewGuid();

        // Act
        var user = User.Create("id", "email@test.com", "First", "Last", explicitId);

        // Assert
        user.Id.Should().Be(explicitId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ThrowArgumentException_When_IdentityIdIsInvalid(string? identityId)
    {
        // Act
        Action act = () => User.Create(identityId!, "email@test.com", "First", "Last");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IdentityId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ThrowArgumentException_When_EmailIsInvalid(string? email)
    {
        // Act
        Action act = () => User.Create("id", email!, "First", "Last");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }
}
