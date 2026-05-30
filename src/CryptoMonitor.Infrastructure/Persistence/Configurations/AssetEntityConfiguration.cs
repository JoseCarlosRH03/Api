using CryptoMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoMonitor.Infrastructure.Persistence.Configurations;

internal sealed class AssetEntityConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Symbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.PriceUsd)
            .HasPrecision(28, 10);

        builder.Property(a => a.MarketCapUsd)
            .HasPrecision(28, 10);

        builder.Property(a => a.VolumeUsd24Hr)
            .HasPrecision(28, 10);

        builder.Property(a => a.ChangePercent24Hr)
            .HasPrecision(10, 4);
    }
}
