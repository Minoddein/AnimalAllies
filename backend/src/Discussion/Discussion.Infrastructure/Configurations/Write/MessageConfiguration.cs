using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discussion.Infrastructure.Configurations.Write;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                id => MessageId.Create(id));

        builder.ComplexProperty(m => m.CreatedAt, c =>
        {
            c.IsRequired();
            c.Property(x => x.Value)
                .HasColumnName("created_at");
        });

        builder.ComplexProperty(m => m.Text, t =>
        {
            t.IsRequired();
            t.Property(x => x.Value)
                .HasColumnName("text")
                .HasMaxLength(Constraints.MAX_DESCRIPTION_LENGTH);
        });

        builder.ComplexProperty(m => m.IsEdited, i =>
        {
            i.IsRequired();
            i.Property(x => x.Value)
                .HasColumnName("is_edited");
        });

        builder.Property(m => m.UserId)
            .HasColumnName("user_id");
    }
}