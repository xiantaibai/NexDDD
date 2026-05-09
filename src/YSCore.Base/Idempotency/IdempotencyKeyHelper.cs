using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace YSCore.Base.Idempotency;

/// <summary>
/// 幂等键帮助类。
/// </summary>
public static class IdempotencyKeyHelper
{
    /// <summary>
    /// 创建幂等记录。
    /// </summary>
    /// <param name="settings">命令级幂等配置。</param>
    /// <param name="options">幂等全局配置。</param>
    /// <returns></returns>
    public static IdempotencyRecord CreateRecord(IdempotencySettings settings, IOptions<IdempotencyOptions> options)
    {
        var now = DateTime.UtcNow;
        var expireIn = settings.ExpireIn.HasValue && settings.ExpireIn.Value > TimeSpan.Zero
            ? settings.ExpireIn.Value
            : options.Value.DefaultExpire;

        return new IdempotencyRecord
        {
            TypeCode = settings.TypeCode,
            Key = settings.Key,
            KeyHash = ToSha256(settings.Key),
            CreateTime = now,
            ExpireTime = now.Add(expireIn),
            Status = IdempotencyStatus.Processing
        };
    }

    /// <summary>
    /// 生成幂等存储键。
    /// </summary>
    /// <param name="prefix">存储前缀。</param>
    /// <param name="typeCode">业务类型编码。</param>
    /// <param name="keyHash">幂等键哈希值。</param>
    /// <returns></returns>
    public static string BuildStorageKey(string prefix, string typeCode, string keyHash)
    {
        return $"{prefix}:{typeCode}:{keyHash}";
    }

    private static string ToSha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
