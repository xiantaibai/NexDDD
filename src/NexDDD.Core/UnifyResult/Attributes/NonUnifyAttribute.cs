namespace NexDDD.Core.UnifyResult.Attributes
{
    /// <summary>
    /// 禁止规范处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class NonUnifyAttribute : Attribute
    {
    }
}
