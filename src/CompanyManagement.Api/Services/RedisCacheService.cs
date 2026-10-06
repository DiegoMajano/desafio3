using System.Text.Json;
using CompanyManagement.Api.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace CompanyManagement.Api.Services;

// Si Redis no responde se registra el error y la API sigue trabajando contra la base de datos.
public class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly TimeSpan _expiracion;

    public RedisCacheService(IDistributedCache cache, IOptions<CacheSettings> settings, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _expiracion = TimeSpan.FromMinutes(settings.Value.ExpirationMinutes);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            _logger.LogInformation("Cache HIT {Key}", key);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error leyendo la cache para {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiracion
            });
            _logger.LogInformation("Cache SET {Key} ({Minutos} min)", key, _expiracion.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error guardando en cache {Key}", key);
        }
    }

    public async Task RemoveAsync(params string[] keys)
    {
        foreach (var key in keys)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("Cache invalidada {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidando la cache {Key}", key);
            }
        }
    }
}
