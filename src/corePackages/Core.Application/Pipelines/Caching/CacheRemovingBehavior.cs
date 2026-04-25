using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NArchitecture.Core.Application.Pipelines.Caching;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheRemovingBehavior<TRequest, TResponse>> _logger;
    private readonly CacheSettings _cacheSettings;

    public CacheRemovingBehavior(
        IDistributedCache cache,
        ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger,
        IConfiguration configuration
    )
    {
        _cache = cache;
        _logger = logger;
        _cacheSettings =
            configuration.GetSection("CacheSettings").Get<CacheSettings>()
            ?? new CacheSettings { SlidingExpiration = 2 };
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request.BypassCache)
            return await next();

        TResponse response = await next();

        if (!_cacheSettings.Enabled)
            return response;

        if (request.CacheGroupKey != null)
            for (int i = 0; i < request.CacheGroupKey.Count(); i++)
            {
                byte[]? cachedGroup = await _cache.GetAsync(request.CacheGroupKey[i], cancellationToken);
                if (cachedGroup != null)
                {
                    HashSet<string> keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(
                        Encoding.Default.GetString(cachedGroup)
                    )!;
                    foreach (string key in keysInGroup)
                    {
                        await _cache.RemoveAsync(key, cancellationToken);
                        _logger.LogInformation($"Removed Cache -> {key}");
                    }

                    await _cache.RemoveAsync(request.CacheGroupKey[i], cancellationToken);
                    _logger.LogInformation("Removed Cache -> {GroupKey}", request.CacheGroupKey[i]);
                    string groupKey = request.CacheGroupKey[i];
                    await _cache.RemoveAsync($"{groupKey}SlidingExpiration", cancellationToken);
                    _logger.LogInformation("Removed Cache -> {SlidingKey}", $"{groupKey}SlidingExpiration");
                }
            }

        if (request.CacheKey != null)
        {
            await _cache.RemoveAsync(request.CacheKey, cancellationToken);
            _logger.LogInformation($"Removed Cache -> {request.CacheKey}");
        }

        return response;
    }
}
