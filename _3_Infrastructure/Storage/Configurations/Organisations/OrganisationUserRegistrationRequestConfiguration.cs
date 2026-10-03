using Domain.Organisations;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Storage.Configurations.Organisations;

public class OrganisationUserRegistrationRequestConfiguration
    : IEntityTypeConfiguration<OrganisationUserRegistrationRequest>
{
    public void Configure(EntityTypeBuilder<OrganisationUserRegistrationRequest> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => new { x.UserId, x.OrganisationId, x.Role })
            .HasFilter("\"status\" = 'Pending'")
            .IsUnique();

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.OrganisationId);

        b.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}