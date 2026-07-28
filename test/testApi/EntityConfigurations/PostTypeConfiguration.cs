using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using testApi.Entites;
using NexDDD.Provider.EntityFrameworkCore.Extensions;

namespace testApi.EntityConfigurations;

/// <summary>
/// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
/// </summary>
public class PostTypeConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> eb)
    {
        eb.ToTable("Post");
        eb.HasKey(t => t.Id);
        eb.Property(t => t.Id).ValueGeneratedOnAdd().UseSnowFlakeValueGenerator();
        eb.Property(t => t.CreateTime).ValueGeneratedOnAdd().UseUtcDateTimeValueGenerator();
        eb.Property(t => t.IsDelete).HasDefaultValue(false).IsRequired();


        // 配置与 Blog 的关系
        eb.HasOne(ur => ur.Blog)
            .WithMany(u => u.Posts)
            .HasForeignKey(ur => ur.BlogId);
    }
}