namespace MyDDD.Template.Application.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }
    string IdentityId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
}
