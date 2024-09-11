using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DataValidation.Behaviors;
using YSCore.Base.DataValidation.Filters;
using YSCore.Base.DataValidation.Options;
using YSCore.Base.Extensions;

namespace YSCore.Base.DataValidation.Extensions
{
    public static class DataValidationServiceCollectionExtensions
    {
        /// <summary>
        /// MVC 控制器 添加 验证
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddDataValidation(this IServiceCollection services, Action<DataValidationOptions> configure = null)
        {
            // 载入服务配置选项
            var configureOptions = new DataValidationOptions();
            configure?.Invoke(configureOptions);

            // 判断是否启用全局
            if (configureOptions.GlobalEnabled)
            {
                // 启用了全局验证，则默认关闭原生 ModelStateInvalidFilter 验证
                services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressMapClientErrors = configureOptions.SuppressMapClientErrors;
                    options.SuppressModelStateInvalidFilter = configureOptions.SuppressModelStateInvalidFilter;
                });

                // 
                services.AddSingleton<IValidatorInterceptor, ErrorModelInterceptor>();

                // 添加全局数据验证
                //services.AddCollectionFilter<DataValidationFilter>(options =>
                //{
                //    // 关闭空引用对象验证
                //    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = configureOptions.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes;
                //});

            }

            return services;
        }

        public static MediatRServiceConfiguration AddDataValidationBehavior(
        this MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
            return cfg;
        }
    }
}
