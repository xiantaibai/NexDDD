using Microsoft.AspNetCore.Mvc.Filters;
using NexDDD.Core.Exceptions.Handlers;

namespace ExceptionDemoApi;

/// <summary>
/// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
/// </summary>
public static class ErrorCodeConst
{
    /// <summary>
    /// 系统异常，通用
    /// 用于验证示例与回归验证相关行为，确保公开契约和运行结果保持正确。
    /// </summary>
    public static class System
    {
        public const string ServerError = "S0001";
    }

    /// <summary>
    /// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
    /// </summary>
    public static class Payment
    {
        public const string OrderNotFound = "P0001";
    }
}
