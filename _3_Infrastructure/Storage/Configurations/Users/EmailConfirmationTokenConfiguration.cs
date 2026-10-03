using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Storage.Configurations.Users;

public class EmailConfirmationTokenConfiguration : IEntityTypeConfiguration<EmailConfirmationToken>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationToken> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Token).IsRequired().HasMaxLength(64);
        b.Property(x => x.ExpiresAt).IsRequired();

        b.HasIndex(x => x.Token).IsUnique();
        b.HasIndex(x => x.UserId);

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}