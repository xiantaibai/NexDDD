using Company.Project.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Project.Infrastructure.EntityFrameworkCore.Configurations;

/// <summary>
/// 将商品聚合的领域属性映射为数据库表、字段约束和唯一索引。
/// </summary>
public sealed class ProductTypeConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsDelete)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(x => x.Sku)
            .IsUnique();
    }
}
