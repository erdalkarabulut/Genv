using Application.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NArchitecture.Core.Persistence.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;

namespace Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BaseDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddDbMigrationApplier(buildServices => buildServices.GetRequiredService<BaseDbContext>());

        services.AddScoped<IEmailAuthenticatorRepository, EmailAuthenticatorRepository>();
        services.AddScoped<IOperationClaimRepository, OperationClaimRepository>();
        services.AddScoped<IOtpAuthenticatorRepository, OtpAuthenticatorRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserOperationClaimRepository, UserOperationClaimRepository>();

        services.AddScoped<IBagRepository, BagRepository>();
        services.AddScoped<IBagMovementRepository, BagMovementRepository>();
        services.AddScoped<IBoxRepository, BoxRepository>();
        services.AddScoped<ICollectionSessionRepository, CollectionSessionRepository>();
        services.AddScoped<IDliProductRepository, DliProductRepository>();
        services.AddScoped<IDonorRepository, DonorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRackRepository, RackRepository>();
        services.AddScoped<ISlotRepository, SlotRepository>();
        services.AddScoped<IBagCellRepository, BagCellRepository>();
        services.AddScoped<ITankRepository, TankRepository>();
        services.AddScoped<IPlcSensorPointRepository, PlcSensorPointRepository>();
        services.AddScoped<IPlcTelemetryReadingRepository, PlcTelemetryReadingRepository>();
        services.AddScoped<IPlcAlarmContactRepository, PlcAlarmContactRepository>();
        services.AddScoped<IClinicalSettingsRepository, ClinicalSettingsRepository>();
        return services;
    }
}
