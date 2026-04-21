namespace MyDDD.Template.Application.Abstractions;

public interface IUserContext
{
    string IdentityId { get; }

    string? Email { get; }

    string? FirstName { get; }

    string? LastName { get; }

    Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default);
}
