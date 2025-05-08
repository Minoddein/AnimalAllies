using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Accounts.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(u => u.SocialNetworks)
            .ValueObjectJsonConverter(
                s => new SocialNetworkDto { Url = s.Url, Title = s.Title },
                dto => SocialNetwork.Create(dto.Title, dto.Url).Value)
            .HasColumnName("social_networks");

        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users);
    }
}