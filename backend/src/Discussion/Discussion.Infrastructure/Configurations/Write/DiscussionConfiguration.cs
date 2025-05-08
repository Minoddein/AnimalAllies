using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussion.Infrastructure.Configurations.Write;

public class DiscussionConfiguration : IEntityTypeConfiguration<Domain.Aggregate.Discussion>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregate.Discussion> builder)
    {
        builder.ToTable("discussions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                id => DiscussionId.Create(id));

        builder.ComplexProperty(d => d.DiscussionStatus, d =>
        {
            d.IsRequired();
            d.Property(x => x.Value)
                .HasColumnName("discussion_status")
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH);
        });

        builder.ComplexProperty(d => d.Users, u =>
        {
            u.IsRequired();
            u.Property(x => x.FirstMember)
                .HasColumnName("first_member");

            u.Property(x => x.SecondMember)
                .HasColumnName("second_member");
        });

        builder.Property(d => d.RelationId)
            .HasColumnName("relation_id");

        builder.HasMany(d => d.Messages)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}