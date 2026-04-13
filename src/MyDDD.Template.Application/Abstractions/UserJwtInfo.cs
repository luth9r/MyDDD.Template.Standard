namespace MyDDD.Template.Application.Abstractions;

public record UserJwtInfo(
    string IdentityId,
    string Email,
    bool IsEmailVerified,
    Guid? InternalUserId,
    string FirstName,
    string LastName);
