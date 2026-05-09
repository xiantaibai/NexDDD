using Microsoft.EntityFrameworkCore;

namespace YSCore.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等模型扩展。
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
