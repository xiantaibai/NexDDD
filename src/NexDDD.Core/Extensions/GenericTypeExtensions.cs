using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.Extensions
{
    /// <summary>
    /// 提供NexDDD 核心模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
    public static class GenericTypeExtensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            string typeName;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else  
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }
    }
}
