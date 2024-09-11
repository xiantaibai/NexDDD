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
            //throw new KnownException(message: result.Errors[0].ErrorMessage, errorCode: 400, errorData: result.Errors
            //    .Select(p =>
            //        new { errorCode = p.ErrorCode, errorMessage = p.ErrorMessage, propertyName = p.PropertyName }
            //    ).ToArray());
            throw ExceptionHelper.Bah("Command Validation Errors for type {0}", exception);
        }
    }
}
