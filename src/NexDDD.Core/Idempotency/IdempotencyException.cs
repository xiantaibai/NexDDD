using NexDDD.Core.Exceptions;

namespace NexDDD.Core.Idempotency;

/// <summary>
/// 幂等校验失败异常。
/// 负责幂等执行模块中的 IdempotencyException 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public sealed class IdempotencyException : AppFriendlyException
{
    /// <summary>
    /// 构造幂等校验失败异常。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="typeCode">业务类型编码。</param>
    /// <param name="keyHash">幂等键哈希值。</param>
    public IdempotencyException(string message, string typeCode, string keyHash)
        : base(message, IdempotencyErrorCodes.RepeatSubmit)
    {
        TypeCode = typeCode;
        KeyHash = keyHash;
    }

    /// <summary>
    /// 业务类型编码。
    /// </summary>
    public string TypeCode { get; }

    /// <summary>
    /// 幂等键哈希值。
    /// </summary>
    public string KeyHash { get; }
}
