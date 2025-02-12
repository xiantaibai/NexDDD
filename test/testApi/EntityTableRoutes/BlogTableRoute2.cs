using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using testApi.Entites;

namespace testApi.EntityTableRoutes;

public class BlogTableRoute : AbstractSimpleShardingModKeyIntVirtualTableRoute<Blog>
{
    public BlogTableRoute() : base(1, 3)
    {

    }

    public override void Configure(EntityMetadataTableBuilder<Blog> builder)
    {
        builder.ShardingProperty(s => s.Id);
    }
}