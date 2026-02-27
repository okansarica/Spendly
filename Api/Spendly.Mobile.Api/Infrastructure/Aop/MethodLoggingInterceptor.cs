namespace Spendly.Mobile.Api.Infrastructure.Aop;

using AspectCore.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

public class MethodLoggingInterceptor : IInterceptor
{
    // Cache for method metadata
    private readonly static ConcurrentDictionary<MethodBase, MethodMetadata> MethodCache = new();

    // Cache for Task<T> result extractors
    private readonly static ConcurrentDictionary<Type, Func<Task, object?>> TaskResultExtractors = new();

    // Shared empty args
    private readonly static (string Name, object? Value)[] EmptyArgs = [];

    public bool AllowMultiple { get; }
    public bool Inherited { get; set; }
    public int Order { get; set; }
    
    private readonly static string[] IgnoredNamespaces =
    {
        "SimplePay.Shared.DataLayer",
    };

    // Removed constructor injection; will resolve from context.ServiceProvider

    public async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var method = context.ImplementationMethod ?? context.ServiceMethod;
        if (method == null)
        {
            Log.Debug("Intercepted call but could not resolve method metadata. " +
                      "Type={Type}, Service={Service}",
                context.Implementation?.GetType().FullName ?? "<unknown>",
                context.ServiceMethod?.Name ?? "<unknown>");
            await next(context);
            return;
        }
        
        var declaringType = method.DeclaringType;
        if (declaringType != null && ShouldSkipNamespace(declaringType.Namespace))
        {
            // loglama yapmadan devam et
            await next(context);
            return;
        }

        // Get or build cached metadata
        var metadata = MethodCache.GetOrAdd(method, CreateMethodMetadata);

        // Capture arguments
        var args = metadata.ParameterNames.Length == 0
            ? EmptyArgs
            : CaptureArguments(context.Parameters, metadata.ParameterNames);

        // Resolve IHttpContextAccessor lazily via ServiceProvider
        var httpContextAccessor = context.ServiceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        var sid = httpContextAccessor?.HttpContext != null ? RequestSession.Get(httpContextAccessor.HttpContext) : null;
        var logger = Log.ForContext("Type", metadata.TypeName)
            .ForContext("Method", metadata.MethodName)
            .ForContext("SessionId", sid ?? string.Empty);

        logger.Debug("Entering {Type}.{Method} with {@Args}", 
            metadata.TypeName, metadata.MethodName, args);

        try
        {
            await next(context);

            var returnValue = await ExtractReturnValue(context, metadata);
            logger.Debug("Exiting {Type}.{Method} with return {@Return}",
                metadata.TypeName, metadata.MethodName, returnValue);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in {Type}.{Method} with {@Args}",
                metadata.TypeName, metadata.MethodName, args);
            throw;
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (string Name, object? Value)[] CaptureArguments(object[] parameters, string[] parameterNames)
    {
        var length = Math.Min(parameters.Length, parameterNames.Length);
        var args = new (string Name, object? Value)[length];
        for (int i = 0; i < length; i++)
        {
            args[i] = (parameterNames[i], parameters[i]);
        }
        return args;
    }

    private static MethodMetadata CreateMethodMetadata(MethodBase method)
    {
        var typeName = method.DeclaringType?.FullName ?? "unknown";
        var methodName = method.Name;
        var parameters = method.GetParameters();
        var parameterNames = new string[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameterNames[i] = parameters[i].Name ?? $"arg{i}";
        }

        Type? returnType = null;
        bool isTask = false;
        bool isGenericTask = false;

        if (method is MethodInfo methodInfo)
        {
            returnType = methodInfo.ReturnType;
            isTask = typeof(Task).IsAssignableFrom(returnType);
            isGenericTask = isTask &&
                            returnType.IsGenericType &&
                            returnType.GetGenericTypeDefinition() == typeof(Task<>);
        }

        return new MethodMetadata
        {
            TypeName = typeName,
            MethodName = methodName,
            ParameterNames = parameterNames,
            ReturnType = returnType,
            IsTask = isTask,
            IsGenericTask = isGenericTask
        };
    }

    private async static Task<object?> ExtractReturnValue(AspectContext context, MethodMetadata metadata)
    {
        if (metadata.ReturnType == null || metadata.ReturnType == typeof(void))
        {
            return null;
        }

        if (!metadata.IsTask)
        {
            return context.ReturnValue;
        }

        if (!metadata.IsGenericTask)
        {
            return null; // plain Task
        }

        // Extract Task<T> result via cached delegate
        var task = (Task)context.ReturnValue!;
        await task.ConfigureAwait(false);

        var extractor = TaskResultExtractors.GetOrAdd(metadata.ReturnType!, CreateTaskResultExtractor);
        return extractor(task);
    }

    private static Func<Task, object?> CreateTaskResultExtractor(Type taskType)
    {
        var resultProperty = taskType.GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
        if (resultProperty == null)
        {
            return _ => null;
        }

        // Build delegate for fast invocation
        return task => resultProperty.GetValue(task);
    }

    private sealed class MethodMetadata
    {
        public string TypeName { get; init; } = string.Empty;
        public string MethodName { get; init; } = string.Empty;
        public string[] ParameterNames { get; init; } = [];
        public Type? ReturnType { get; init; }
        public bool IsTask { get; init; }
        public bool IsGenericTask { get; init; }
    }
    
    private static bool ShouldSkipNamespace(string? ns)
    {
        if (string.IsNullOrEmpty(ns))
        {
            return false;
        }
        foreach (var ignored in IgnoredNamespaces)
        {
            if (ns.StartsWith(ignored, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }
}
