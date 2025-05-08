using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Infrastructure.Configurations.Write;

public class ProhibitionSendingConfiguration : IEntityTypeConfiguration<ProhibitionSending>
{
    public void Configure(EntityTypeBuilder<ProhibitionSending> builder)
    {
        builder.ToTable("prohibitions_sending");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(
                b => b.Id,
                id => ProhibitionSendingId.Create(id));
    }
}