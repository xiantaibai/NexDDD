using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using testApi.Entites;
using YSCore.Provider.EntityFrameworkCore.Extensions;

namespace testApi.EntityConfigurations;

public class BlogTypeConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> eb)
    {
        eb.ToTable("Blog");
        eb.HasKey(t => t.Id);
        //eb.Property(t => t.Id).ValueGeneratedOnAdd().UseSnowFlakeValueGenerator();
        //eb.Property(t => t.CreateTime).ValueGeneratedOnAdd().UseUtcDateTimeValueGenerator();
        eb.Property(t => t.IsDelete).HasDefaultValue(false).IsRequired();

    }
}