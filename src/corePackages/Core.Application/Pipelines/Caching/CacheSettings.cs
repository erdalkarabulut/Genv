namespace NArchitecture.Core.Application.Pipelines.Caching;

public class CacheSettings
{
    /// <summary>
    /// false iken MediatR sorgu yanıtları dağıtık önbelleğe yazılmaz / okunmaz; her zaman veritabanından üretilir.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int SlidingExpiration { get; set; }
}
