using Microsoft.EntityFrameworkCore;

namespace NexDDD.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等模型扩展。
/// 负责幂等执行模块中的 ModelBuilderExtensions 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
internal static class ModelBuilderExtensions
{
    /// <summary>
    /// 添加幂等记录实体映射。
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static void AddIdempotencyModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IdempotencyRecordEntityTypeConfiguration());
    }
}
