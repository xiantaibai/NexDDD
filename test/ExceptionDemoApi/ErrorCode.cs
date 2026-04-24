using Microsoft.AspNetCore.Mvc.Filters;
using YSCore.Base.Exceptions.Handlers;

namespace ExceptionDemoApi;

public static class ErrorCodeConst
{
    /// <summary>
    /// 系统异常，通用
    /// </summary>
    public static class System
    {
        public const string ServerError = "S0001";
    }

    public static class Payment
    {
        public const string OrderNotFound = "P0001";
    }
}
