using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects.DomainEvents;

namespace MyDDD.Template.Domain.Projects;

public sealed class Project : AggregateRoot
{
    public string Name { get; private set; }

    public Guid UserId { get; private set; }

    private Project(Guid id, string name, Guid userId) : base(id)
    {
        Name = name;
        UserId = userId;
    }

    public static Project Create(string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }

        var project = new Project(Guid.NewGuid(), name, userId);

        project.RaiseDomainEvent(new ProjectCreatedDomainEvent(project.Id, project.UserId));

        return project;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        Name = newName;
    }
}
