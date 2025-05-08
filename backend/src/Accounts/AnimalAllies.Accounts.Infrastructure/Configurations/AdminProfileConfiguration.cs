using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Accounts.Infrastructure.Configurations;

public class AdminProfileConfiguration : IEntityTypeConfiguration<AdminProfile>
{
    public void Configure(EntityTypeBuilder<AdminProfile> builder)
    {
        builder.ToTable("admin_profiles");

        builder.ComplexProperty(ap => ap.FullName, apb =>
        {
            apb.Property(f => f.FirstName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("first_name");
            apb.Property(f => f.SecondName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("second_name");
            apb.Property(f => f.Patronymic)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("patronymic");
        });

        builder.HasOne(ap => ap.User)
            .WithOne()
            .HasForeignKey<AdminProfile>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(pa => pa.Id);
    }
}