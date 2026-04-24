using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IRefreshTokenRepository : IAsyncRepository<RefreshToken, Guid>, IRepository<RefreshToken, Guid>
{
    Task<List<RefreshToken>> GetOldRefreshTokensAsync(Guid userId, int refreshTokenTTL);

    /// <summary>ExpiresDate geçmiş tüm satırlar (kullanılamaz; veri tabanını inceltmek için silinir).</summary>
    Task<List<RefreshToken>> GetExpiredTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
