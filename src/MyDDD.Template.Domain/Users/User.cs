using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects.DomainEvents;
using MyDDD.Template.Domain.Users.DomainEvents;

namespace MyDDD.Template.Domain.Users;

public sealed class User : AggregateRoot
{
    public string IdentityId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    private User(Guid id, string identityId, string email, string firstName, string lastName) : base(id)
    {
        IdentityId = identityId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public static User Create(Guid id, string identityId, string email, string firstName, string lastName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(identityId))
        {
            throw new ArgumentException("IdentityId cannot be empty", nameof(identityId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        var user = new User(id, identityId, email, firstName, lastName);

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, user.IdentityId, user.Email));

        return user;
    }
}
