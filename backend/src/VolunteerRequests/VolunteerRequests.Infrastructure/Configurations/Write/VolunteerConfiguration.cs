using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Infrastructure.Configurations.Write;

public class VolunteerConfiguration: IEntityTypeConfiguration<VolunteerRequest>
{
    public void Configure(EntityTypeBuilder<VolunteerRequest> builder)
    {
        builder.ToTable("volunteer_requests");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                id => VolunteerRequestId.Create(id));
        
        builder.ComplexProperty(v => v.CreatedAt, c =>
        {
            c.IsRequired();
            c.Property(x => x.Value)
                .HasColumnName("created_at");
        });
        
        builder.ComplexProperty(v => v.RequestStatus, r =>
        {
            r.IsRequired();
            r.Property(x => x.Value)
                .HasColumnName("request_status");
        });
        
        builder.ComplexProperty(v => v.RejectionComment, r =>
        {
            r.Property(x => x.Value)
                .HasColumnName("rejection_comment")
                .IsRequired(false);
        });

        builder.ComplexProperty(v => v.VolunteerInfo, vb =>
        {
            vb.ComplexProperty(v => v.FullName, f =>
            {
                f.Property(x => x.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(Constraints.MIDDLE_NAME_LENGTH)
                    .IsRequired();
                f.Property(x => x.SecondName)
                    .HasColumnName("second_name")
                    .HasMaxLength(Constraints.MIDDLE_NAME_LENGTH)
                    .IsRequired();
                f.Property(x => x.Patronymic)
                    .HasColumnName("patronymic")
                    .IsRequired(false);
            });

            vb.ComplexProperty(v => v.Email, e =>
            {
                e.Property(x => x.Value)
                    .HasColumnName("email")
                    .IsRequired();
            });

            vb.ComplexProperty(v => v.PhoneNumber, p =>
            {
                p.Property(x => x.Number)
                    .HasColumnName("phone_number")
                    .HasMaxLength(Constraints.MAX_PHONENUMBER_LENGTH)
                    .IsRequired();
            });

            vb.ComplexProperty(v => v.WorkExperience, w =>
            {
                w.Property(x => x.Value)
                    .HasColumnName("work_experience")
                    .IsRequired();
            });

            vb.ComplexProperty(v => v.VolunteerDescription, vd =>
            {
                vd.Property(x => x.Value)
                    .HasMaxLength(Constraints.MAX_DESCRIPTION_LENGTH)
                    .HasColumnName("description");
            });
        });


        builder.Property(v => v.AdminId)
            .HasColumnName("admin_id");

        builder.Property(v => v.DiscussionId)
            .HasColumnName("discussion_id");
        
        builder.Property(v => v.UserId)
            .HasColumnName("user_id");

    }
}