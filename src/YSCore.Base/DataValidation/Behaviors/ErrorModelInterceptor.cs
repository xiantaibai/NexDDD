using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YSCore.Base.DataValidation.Internal;
using YSCore.Base.Exceptions;

namespace YSCore.Base.DataValidation.Behaviors
{
    public class ErrorModelInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext,
            ValidationResult result)
        {
            if (result.Errors == null || result.Errors.Count == 0)
                return result;

            // 获取验证的对象
            var validatedObject = validationContext.InstanceToValidate;

            // 获取对象的类型名称
            var objectTypeName = validatedObject.GetType().Name;

            var failures = result.Errors.ToList();
            var exception = new ValidationException("Validation failed", failures);

            actionContext.HttpContext.Items[nameof(ValidationMetadata) + nameof(AppFriendlyException)] = failures;
            throw ExceptionHelper.Bah("Collection Validation failed for type {0}", exception, objectTypeName);
        }
    }
}
