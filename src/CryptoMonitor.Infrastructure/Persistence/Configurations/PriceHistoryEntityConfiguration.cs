using CryptoMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoMonitor.Infrastructure.Persistence.Configurations;

internal sealed class PriceHistoryEntityConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("PriceHistories");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.AssetId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PriceUsd)
            .HasPrecision(28, 10);

        builder.HasOne(p => p.Asset)
            .WithMany()
            .HasForeignKey(p => p.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.AssetId, p.RecordedAt });
    }
}
