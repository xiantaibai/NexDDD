using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using testApi.Entites;
using YSCore.Provider.EntityFrameworkCore.Extensions;

namespace testApi.EntityConfigurations;

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