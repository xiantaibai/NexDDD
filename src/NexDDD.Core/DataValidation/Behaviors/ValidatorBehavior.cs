using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NexDDD.Core.DataValidation.Internal;
using NexDDD.Core.Events;
using NexDDD.Core.Exceptions;
using NexDDD.Core.Extensions;

namespace NexDDD.Core.DataValidation.Behaviors
{
    /// <summary>
    /// 负责数据验证流程的处理与编排，协调上游请求和下游服务完成当前用例。
    /// </summary>
    public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseCommand
    {
        private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly IHttpContextAccessor _httpContext;

        public ValidatorBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger, IHttpContextAccessor httpContext)
        {
            _validators = validators;
            _logger = logger;
            _httpContext = httpContext;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var typeName = request.GetGenericTypeName();

            _logger.LogInformation("Validating command {CommandType}", typeName);

            var failures = _validators
                .Select(v => v.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning("Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName, request, failures);
                // var metadata = ValidatorContext.GetValidationMetadata(failures);
                var exception = new ValidationException("Validation failed", failures);
                // 上下文设置
                _httpContext.HttpContext.Items[nameof(ValidationMetadata) + nameof(AppFriendlyException)] = failures;

                throw ExceptionHelper.Bah("Command Validation Errors for type {0}", exception, typeof(TRequest).Name);
            }

            return await next();
        }
    }
}
