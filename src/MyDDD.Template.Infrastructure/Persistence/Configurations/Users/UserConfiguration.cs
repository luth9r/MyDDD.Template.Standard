using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyDDD.Template.Infrastructure.Persistence.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<Template.Domain.Users.User>
{
    public void Configure(EntityTypeBuilder<Template.Domain.Users.User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.IdentityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(p => p.IdentityId)
            .IsUnique();

        builder.Property(p => p.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(p => p.Email)
            .IsUnique();
    }
}
