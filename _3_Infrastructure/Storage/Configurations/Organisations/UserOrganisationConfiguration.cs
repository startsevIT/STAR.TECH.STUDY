using Domain.Organisations;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Storage.Configurations.Organisations;

public class UserOrganisationConfiguration : IEntityTypeConfiguration<UserOrganisation>
{
    public void Configure(EntityTypeBuilder<UserOrganisation> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => new { x.UserId, x.OrganisationId, x.Role }).IsUnique();
        b.HasIndex(x => new { x.Id, x.OrganisationId }).IsUnique();
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.OrganisationId);

        b.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}