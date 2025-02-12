using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using ShardingCore.VirtualRoutes.Months;
using testApi.Entites;

namespace testApi.EntityTableRoutes;

public class BlogTableRoute2 : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Blog>
{
    public override DateTime GetBeginTime()
    {
        return new DateTime(2025, 1, 1);
    }

    public override void Configure(EntityMetadataTableBuilder<Blog> builder)
    {
        builder.ShardingProperty(s => s.CreateTime);
    }

    public override bool AutoCreateTableByTime()
    {
        return true;
    }
}