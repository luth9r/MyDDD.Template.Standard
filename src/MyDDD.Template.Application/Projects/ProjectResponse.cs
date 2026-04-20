using MyDDD.Template.Domain.Projects;

namespace MyDDD.Template.Application.Projects;

public sealed record ProjectResponse(Guid Id, string Name);
