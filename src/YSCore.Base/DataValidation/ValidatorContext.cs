using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using YSCore.Base.DataValidation.Internal;

namespace YSCore.Base.DataValidation
{
    /// <summary>
    /// 验证上下文
    /// </summary>
    internal static class ValidatorContext
    {
        /// <summary>
        /// 获取验证错误信息
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        internal static ValidationMetadata GetValidationMetadata(object errors)
        {
            ModelStateDictionary _modelState = null;
            object validationResults = null;
            (string message, string firstErrorMessage, string firstErrorProperty) = (default, default, default);

            // 判断是否是集合类型
            if (errors is System.Collections.IEnumerable && errors is not string)
            {
                // 如果是模型验证字典类型
                if (errors is ModelStateDictionary modelState)
                {
                    _modelState = modelState;
                    // 将验证错误信息转换成字典并序列化成 Json
                    validationResults = modelState.Where(u => modelState[u.Key].ValidationState == ModelValidationState.Invalid)
                            .ToDictionary(u => u.Key, u => modelState[u.Key].Errors.Select(c => c.ErrorMessage).ToArray());
                }
                // 如果是 ValidationProblemDetails 特殊类型
                else if (errors is ValidationProblemDetails validation)
                {
                    validationResults = validation.Errors
                        .ToDictionary(u => u.Key, u => u.Value.ToArray());
                }
                // 如果是字典类型
                else if (errors is Dictionary<string, string[]> dicResults)
                {
                    validationResults = dicResults;
                }
                // FluentValidation
                else if (errors is List<ValidationFailure> validationFailures)
                {
                    validationResults = validationFailures.GroupBy(s => s.PropertyName).ToDictionary(u => u.Key, u =>
                    validationFailures.Where(s => s.PropertyName == u.Key).Select(s => s.ErrorMessage).ToArray());
                }

                message = JsonSerializer.Serialize(validationResults, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });
                firstErrorMessage = (validationResults as Dictionary<string, string[]>).First().Value[0];
                firstErrorProperty = (validationResults as Dictionary<string, string[]>).First().Key;
            }
            // 其他类型
            else
            {
                validationResults = firstErrorMessage = message = errors?.ToString();
            }

            return new ValidationMetadata
            {
                ValidationResult = validationResults,
                Message = message,
                ModelState = _modelState,
                FirstErrorProperty = firstErrorProperty,
                FirstErrorMessage = firstErrorMessage
            };
        }
    }
}
