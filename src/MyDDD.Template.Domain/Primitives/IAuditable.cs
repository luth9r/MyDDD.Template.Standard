namespace MyDDD.Template.Domain.Primitives;

public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }

    DateTime? ModifiedAtUtc { get; set; }
}
