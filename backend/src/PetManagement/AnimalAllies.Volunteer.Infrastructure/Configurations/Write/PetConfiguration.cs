using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Volunteer.Infrastructure.Configurations.Write;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("pets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                Id => PetId.Create(Id));

        builder.ComplexProperty(x => x.AnimalType, b =>
        {
            b.IsRequired();
            b.Property(x => x.SpeciesId)
                .HasColumnName("species_id")
                .HasConversion(
                    id => id.Id,
                    id => SpeciesId.Create(id));
            b.Property(x => x.BreedId)
                .HasColumnName("breed_id");
        });

        builder.ComplexProperty(x => x.Name, b =>
        {
            b.IsRequired();
            b.Property(x => x.Value)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("name");
        });

        builder.ComplexProperty(x => x.PetDetails, b =>
        {
            b.IsRequired();
            b.Property(x => x.Description)
                .HasMaxLength(Constraints.MAX_DESCRIPTION_LENGTH);

            b.Property(x => x.BirthDate)
                .HasColumnName("birth_date");

            b.Property(x => x.CreationTime)
                .HasColumnName("creation_time");
        });

        builder.ComplexProperty(x => x.PetPhysicCharacteristics, ppc =>
        {
            ppc.IsRequired();

            ppc.Property(x => x.Color)
                .HasMaxLength(Constraints.MAX_PET_COLOR_LENGTH)
                .HasColumnName("color");
            ppc.Property(x => x.HealthInformation)
                .HasMaxLength(Constraints.MAX_PET_INFORMATION_LENGTH)
                .HasColumnName("health_information");
            ppc.Property(x => x.Height)
                .HasColumnName("height");
            ppc.Property(x => x.Weight)
                .HasColumnName("weight");
            ppc.Property(x => x.IsCastrated)
                .HasColumnName("is_castrated");
            ppc.Property(x => x.IsVaccinated)
                .HasColumnName("is_vaccinated");
        });

        builder.ComplexProperty(x => x.Address, a =>
        {
            a.IsRequired();
            a.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH);
            a.Property(x => x.Street)
                .HasColumnName("street")
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH);
            a.Property(x => x.State)
                .HasColumnName("state")
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH);
            a.Property(x => x.ZipCode)
                .HasColumnName("zip_code")
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH);
        });

        builder.ComplexProperty(x => x.PhoneNumber, p =>
        {
            p.IsRequired();
            p.Property(x => x.Number)
                .HasColumnName("phone_number")
                .HasMaxLength(Constraints.MAX_PHONENUMBER_LENGTH);
        });

        builder.ComplexProperty(x => x.HelpStatus, h =>
        {
            h.IsRequired();
            h.Property(x => x.Value)
                .HasColumnName("help_status");
        });

        builder.ComplexProperty(p => p.Position, pb =>
        {
            pb.IsRequired();
            pb.Property(p => p.Value)
                .HasColumnName("position");
        });

        builder.Property(p => p.PetPhotoDetails)
            .ValueObjectJsonConverter(
                p => new PetPhotoDto { Path = p.Path.Path, IsMain = p.IsMain },
                dto => new PetPhoto(FilePath.Create(dto.Path).Value, dto.IsMain))
            .HasColumnName("pet_photos");

        builder.Property(p => p.Requisites)
            .ValueObjectJsonConverter(
                r => new RequisiteDto { Title = r.Title, Description = r.Description },
                dto => Requisite.Create(dto.Title, dto.Description).Value)
            .HasColumnName("requisites");

        builder.Property<bool>("IsDeleted")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("is_deleted");
    }
}