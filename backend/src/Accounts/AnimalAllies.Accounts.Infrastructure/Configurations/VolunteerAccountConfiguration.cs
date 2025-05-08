using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Accounts.Infrastructure.Configurations;

public class VolunteerAccountConfiguration : IEntityTypeConfiguration<VolunteerAccount>
{
    public void Configure(EntityTypeBuilder<VolunteerAccount> builder)
    {
        builder.ToTable("volunteer_accounts");

        builder.ComplexProperty(va => va.FullName, vab =>
        {
            vab.Property(f => f.FirstName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("first_name");
            vab.Property(f => f.SecondName)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("second_name");
            vab.Property(f => f.Patronymic)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("patronymic");
        });

        builder.Property(v => v.Requisites)
            .ValueObjectJsonConverter(
                r => new RequisiteDto { Title = r.Title, Description = r.Description },
                dto => Requisite.Create(dto.Title, dto.Description).Value)
            .HasColumnName("requisites");

        builder.Property(v => v.Certificates)
            .ValueObjectJsonConverter(
                c => new CertificateDto
                {
                    Title = c.Title,
                    Description = c.Description,
                    IssuingOrganization = c.IssuingOrganization,
                    IssueDate = c.IssueDate,
                    ExpirationDate = c.ExpirationDate
                },
                dto => Certificate.Create(
                    dto.Title,
                    dto.IssuingOrganization,
                    dto.IssueDate,
                    dto.ExpirationDate,
                    dto.Description).Value)
            .HasColumnName("certificates");

        builder.HasOne(va => va.User)
            .WithOne(u => u.VolunteerAccount)
            .HasForeignKey<VolunteerAccount>(va => va.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(pa => pa.Id);
    }
}