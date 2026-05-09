using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等记录实体映射。
/// </summary>
internal sealed class IdempotencyRecordEntityTypeConfiguration : IEntityTypeConfiguration<IdempotencyRecordEntity>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecordEntity> builder)
    {
        builder.ToTable("ys_idempotency_records");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TypeCode)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.KeyHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Key)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ExpireTime)
            .IsRequired();

        builder.Property(x => x.RequestId)
               .HasMaxLength(64)
               .IsRequired();

        builder.HasIndex(x => new { x.TypeCode, x.KeyHash })
            .IsUnique();

        builder.HasIndex(x => x.ExpireTime);
    }
}
