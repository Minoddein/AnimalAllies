using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Volunteer.Infrastructure.Configurations.Write;

public class VolunteerConfiguration
    : IEntityTypeConfiguration<Domain.VolunteerManagement.Aggregate.Volunteer>
{
    public void Configure(EntityTypeBuilder<Domain.VolunteerManagement.Aggregate.Volunteer> builder)
    {
        builder.ToTable("volunteers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                id => VolunteerId.Create(id));

        builder.ComplexProperty(x => x.Description, b =>
        {
            b.IsRequired();
            b.Property(x => x.Value)
                .HasMaxLength(Constraints.MAX_DESCRIPTION_LENGTH)
                .HasColumnName("description");
        });

        builder.ComplexProperty(x => x.WorkExperience, we =>
        {
            we.IsRequired();
            we.Property(x => x.Value)
                .HasColumnName("work_experience");
        });

        builder.ComplexProperty(x => x.Phone, p =>
        {
            p.IsRequired();
            p.Property(x => x.Number)
                .HasColumnName("phone_number")
                .HasMaxLength(Constraints.MAX_PHONENUMBER_LENGTH);
        });

        builder.ComplexProperty(x => x.Email, p =>
        {
            p.IsRequired();
            p.Property(x => x.Value)
                .HasColumnName("email");
        });

        builder.ComplexProperty(x => x.FullName, f =>
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

        builder.Property(v => v.Requisites)
            .ValueObjectJsonConverter(
                r => new RequisiteDto { Title = r.Title, Description = r.Description },
                dto => Requisite.Create(dto.Title, dto.Description).Value)
            .HasColumnName("requisites");

        builder.Property<bool>("IsDeleted")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("is_deleted");

        builder.HasMany(x => x.Pets)
            .WithOne()
            .HasForeignKey("volunteer_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}