namespace MyDDD.Template.Application.Abstractions;

public interface IUserContext
{
    Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default);
    string IdentityId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
}
