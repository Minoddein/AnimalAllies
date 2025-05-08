using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Accounts.Infrastructure.Configurations;

public class ParticipantAccountConfiguration : IEntityTypeConfiguration<ParticipantAccount>
{
    public void Configure(EntityTypeBuilder<ParticipantAccount> builder)
    {
        builder.ToTable("participant_accounts");

        builder.ComplexProperty(pa => pa.FullName, pab =>
        {
            pab.Property(f => f.FirstName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("first_name");
            pab.Property(f => f.SecondName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("second_name");
            pab.Property(f => f.Patronymic)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("patronymic");
        });

        builder.HasOne(pa => pa.User)
            .WithOne(u => u.ParticipantAccount)
            .HasForeignKey<ParticipantAccount>(pa => pa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(pa => pa.Id);
    }
}