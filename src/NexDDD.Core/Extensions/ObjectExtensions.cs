using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Extensions
{
    /// <summary>
    /// 提供NexDDD 核心模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class ObjectExtensions
    {

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static string Format(this string str, params object[] args)
        {
            return args == null || args.Length == 0 ? str : string.Format(str, args);
        }

        /// <summary>
        /// 判断方法是否是异步
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        internal static bool IsAsync(this MethodInfo method)
        {
            return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null
                || method.ReturnType.ToString().StartsWith(typeof(Task).FullName);
        }

        /// <summary>
        /// 获取方法真实返回类型
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        internal static Type GetRealReturnType(this MethodInfo method)
        {
            // 判断是否是异步方法
            var isAsyncMethod = method.IsAsync();

            // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
            var returnType = method.ReturnType;
            return isAsyncMethod ? returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void) : returnType;
        }

        /// <summary>
        /// 判断类型是否实现某个泛型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="generic">泛型类型</param>
        /// <returns>bool</returns>
        internal static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            // 检查接口类型
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 检查类型
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            // 判断逻辑
            bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }


        /// <summary>
        /// 查找方法指定特性，如果没找到则继续查找声明类
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="method"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        internal static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit)
            where TAttribute : Attribute
        {
            // 获取方法所在类型
            var declaringType = method.DeclaringType;

            var attributeType = typeof(TAttribute);

            // 判断方法是否定义指定特性，如果没有再查找声明类
            var foundAttribute = method.IsDefined(attributeType, inherit)
                ? method.GetCustomAttribute<TAttribute>(inherit)
                :
                    declaringType.IsDefined(attributeType, inherit)
                    ? declaringType.GetCustomAttribute<TAttribute>(inherit)
                    : default
                ;

            return foundAttribute;
        }

    }
}
