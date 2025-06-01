using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Species.Infrastructure.Configurations.Write;

public class SpeciesConfiguration: IEntityTypeConfiguration<Domain.Species>
{
    public void Configure(EntityTypeBuilder<Domain.Species> builder)
    {
        builder.ToTable("species");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Id,
                id => SpeciesId.Create(id));

        builder.ComplexProperty(x => x.Name, b =>
        {
            b.IsRequired();
            b.Property(x => x.Value)
                .HasMaxLength(Constraints.MAX_VALUE_LENGTH)
                .HasColumnName("name");
        });
        
        
        builder.HasMany(x => x.Breeds)
            .WithOne()
            .HasForeignKey("species_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
    }
}