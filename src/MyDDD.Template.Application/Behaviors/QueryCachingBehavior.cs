using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Application.Behaviors;

public sealed partial class QueryCachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Cache hit for {CacheKey}")]
    private static partial void CacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for {CacheKey}")]
    private static partial void CacheMiss(ILogger logger, string cacheKey);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedQuery)
        {
            return await next(cancellationToken);
        }

        var cachedResult = await cache.GetStringAsync(cachedQuery.CacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            CacheHit(logger, cachedQuery.CacheKey);

            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var deserializedValue = JsonSerializer.Deserialize(cachedResult, valueType);

            var successMethod = typeof(Result).GetMethods().First(m => m is { Name: "Success", IsGenericMethod: true });
            var genericSuccess = successMethod.MakeGenericMethod(valueType);

            return (TResponse)genericSuccess.Invoke(null, [deserializedValue])!;
        }

        CacheMiss(logger, cachedQuery.CacheKey);
        var response = await next(cancellationToken);

        if (response.IsSuccess)
        {
            var valueProperty = typeof(TResponse).GetProperty("Value")!;
            var payload = valueProperty.GetValue(response);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cachedQuery.Expiration ?? TimeSpan.FromMinutes(10),
            };

            await cache.SetStringAsync(cachedQuery.CacheKey, JsonSerializer.Serialize(payload), options, cancellationToken);
        }

        return response;
    }
}
