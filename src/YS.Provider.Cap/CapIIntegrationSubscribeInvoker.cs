using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Filter;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using YSCore.Base.Integration;

namespace YSCore.Provider.Cap
{
    /// <summary>
    /// 替换Cap 原生的处理方法
    /// </summary>
    public class CapIIntegrationSubscribeInvoker : ISubscribeInvoker
    {
        private readonly ConcurrentDictionary<string, ObjectMethodExecutor> _executors;
        private readonly ISerializer _serializer;
        private readonly IServiceProvider _serviceProvider;

        public CapIIntegrationSubscribeInvoker(IServiceProvider serviceProvider, ISerializer serializer)
        {
            _serviceProvider = serviceProvider;
            _serializer = serializer;
            _executors = new ConcurrentDictionary<string, ObjectMethodExecutor>();
        }

        public async Task<ConsumerExecutedResult> InvokeAsync(ConsumerContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var methodInfo = context.ConsumerDescriptor.MethodInfo;
            var reflectedTypeHandle = methodInfo.ReflectedType!.TypeHandle.Value;
            var methodHandle = methodInfo.MethodHandle.Value;
            var key = $"{reflectedTypeHandle}_{methodHandle}";

            var executor = _executors.GetOrAdd(key,
                _ => ObjectMethodExecutor.Create(methodInfo, context.ConsumerDescriptor.ImplTypeInfo));

            await using var scope = _serviceProvider.CreateAsyncScope();

            var provider = scope.ServiceProvider;

            var obj = GetInstance(provider, context);

            var message = context.DeliverMessage;

            var integrationEvent = message.Value as IIntegrationEvent;
            if (integrationEvent == null)
            {
                throw new InvalidOperationException("Message content does not implement IIntegrationEvent");
            }

            // 创建 IntegrationEventHandlerContext 实例
            var handlerContext = new IntegrationEventHandlerContext(
                integrationEvent, 
                message.Headers.ToDictionary(kvp => (object)kvp.Key, kvp => (object?)kvp.Value));

            var parameterDescriptors = context.ConsumerDescriptor.Parameters;
            var executeParameters = new object?[parameterDescriptors.Count];
            for (var i = 0; i < parameterDescriptors.Count; i++)
            {
                var parameterDescriptor = parameterDescriptors[i];
                if (parameterDescriptor.IsFromCap)
                {
                    executeParameters[i] = GetCapProvidedParameter(parameterDescriptor, message, cancellationToken);
                }
                else
                {
                    executeParameters[i] = handlerContext;
                    //if (message.Value != null)
                    //{
                    //    // use ISerializer when reading from storage, skip other objects if not Json
                    //    if (_serializer.IsJsonType(message.Value))
                    //    {
                    //        executeParameters[i] =
                    //            _serializer.Deserialize(message.Value, parameterDescriptor.ParameterType);
                    //    }
                    //    else
                    //    {
                    //        var converter = TypeDescriptor.GetConverter(parameterDescriptor.ParameterType);
                    //        if (converter.CanConvertFrom(message.Value.GetType()))
                    //        {
                    //            executeParameters[i] = converter.ConvertFrom(message.Value);
                    //        }
                    //        else
                    //        {
                    //            if (parameterDescriptor.ParameterType.IsInstanceOfType(message.Value))
                    //                executeParameters[i] = message.Value;
                    //            else
                    //                executeParameters[i] =
                    //                    Convert.ChangeType(message.Value, parameterDescriptor.ParameterType);
                    //        }
                    //    }
                    //}
                }
            }

            var filter = provider.GetService<ISubscribeFilter>();
            object? resultObj = null;
            try
            {
                if (filter != null)
                {
                    var etContext = new ExecutingContext(context, executeParameters);
                    await filter.OnSubscribeExecutingAsync(etContext).ConfigureAwait(false);
                    executeParameters = etContext.Arguments;
                }

                resultObj = await ExecuteWithParameterAsync(executor, obj, executeParameters).ConfigureAwait(false);

                if (filter != null)
                {
                    var edContext = new ExecutedContext(context, resultObj);
                    await filter.OnSubscribeExecutedAsync(edContext).ConfigureAwait(false);
                    resultObj = edContext.Result;
                }
            }
            catch (Exception e)
            {
                if (filter != null)
                {
                    var exContext = new ExceptionContext(context, e);
                    await filter.OnSubscribeExceptionAsync(exContext).ConfigureAwait(false);
                    if (!exContext.ExceptionHandled) ExceptionDispatchInfo.Capture(exContext.Exception).Throw();

                    if (exContext.Result != null) resultObj = exContext.Result;
                }
                else
                {
                    throw;
                }
            }

            var callbackName = message.GetCallbackName();
            if (string.IsNullOrEmpty(callbackName))
            {
                return new ConsumerExecutedResult(resultObj, message.GetId(), null, null);
            }
            else
            {
                var capHeader = executeParameters.FirstOrDefault(x => x is CapHeader) as CapHeader;
                return new ConsumerExecutedResult(resultObj, message.GetId(), callbackName, message.Headers);
            }

        }

        private static object GetCapProvidedParameter(ParameterDescriptor parameterDescriptor, Message message,
        CancellationToken cancellationToken)
        {
            if (typeof(CancellationToken).IsAssignableFrom(parameterDescriptor.ParameterType)) return cancellationToken;

            if (parameterDescriptor.ParameterType.IsAssignableFrom(typeof(CapHeader)))
                return new CapHeader(message.Headers);

            throw new ArgumentException(parameterDescriptor.Name);
        }

        protected virtual object GetInstance(IServiceProvider provider, ConsumerContext context)
        {
            var srvType = context.ConsumerDescriptor.ServiceTypeInfo?.AsType();
            var implType = context.ConsumerDescriptor.ImplTypeInfo.AsType();

            object? obj = null;
            if (srvType != null) obj = provider.GetServices(srvType).FirstOrDefault(o => o?.GetType() == implType);

            if (obj == null) obj = ActivatorUtilities.GetServiceOrCreateInstance(provider, implType);

            return obj;
        }

        private async Task<object?> ExecuteWithParameterAsync(ObjectMethodExecutor executor, object @class,
            object?[] parameter)
        {
            if (executor.IsMethodAsync) return await executor.ExecuteAsync(@class, parameter);

            return executor.Execute(@class, parameter);
        }
    }
}
