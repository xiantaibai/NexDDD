using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using testApi.Entites;

namespace testApi.EntityTableRoutes;

/// <summary>
/// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
/// </summary>
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