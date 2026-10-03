using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Storage.Configurations.Users;

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Email).IsRequired().HasMaxLength(320);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(32);
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.PasswordSalt).IsRequired();

        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.Phone).IsUnique();

        b.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserCredential>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}