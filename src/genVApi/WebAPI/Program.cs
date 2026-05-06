using Application;
using Application.Services.RealTime;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NArchitecture.Core.CrossCuttingConcerns.Exception.WebApi.Extensions;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Configurations;
using NArchitecture.Core.ElasticSearch.Models;
using NArchitecture.Core.Localization.WebApi;
using NArchitecture.Core.Mailing;
using NArchitecture.Core.Persistence.WebApi;
using NArchitecture.Core.Security.Encryption;
using NArchitecture.Core.Security.JWT;
using NArchitecture.Core.Security.WebApi.Swagger.Extensions;
using Persistence;
using Persistence.Seed;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI;
using WebAPI.Hubs;
using WebAPI.Services;
using Licensing.Shared;
using Infrastructure.Sms;
using Microsoft.AspNetCore.HttpOverrides;

// Müşteri makinesinden parmak izi almak için: WebAPI.exe --license-fingerprint
if (args is { Length: > 0 } && args.Contains("--license-fingerprint", StringComparer.Ordinal))
{
    Console.WriteLine(MachineFingerprintProvider.GetFingerprintSha256Hex());
    return;
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IndustrialIntegrationSettings>(
    builder.Configuration.GetSection(IndustrialIntegrationSettings.SectionName));

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddApplicationServices(
    mailSettings: builder.Configuration.GetSection("MailSettings").Get<MailSettings>()
        ?? throw new InvalidOperationException("MailSettings section cannot found in configuration."),
    fileLogConfiguration: builder
        .Configuration.GetSection("SeriLogConfigurations:FileLogConfiguration")
        .Get<FileLogConfiguration>()
        ?? throw new InvalidOperationException("FileLogConfiguration section cannot found in configuration."),
    elasticSearchConfig: builder.Configuration.GetSection("ElasticSearchConfig").Get<ElasticSearchConfig>()
        ?? throw new InvalidOperationException("ElasticSearchConfig section cannot found in configuration.")
);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.AddSmsServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

const string tokenOptionsConfigurationSection = "TokenOptions";
TokenOptions tokenOptions =
    builder.Configuration.GetSection(tokenOptionsConfigurationSection).Get<TokenOptions>()
    ?? throw new InvalidOperationException($"\"{tokenOptionsConfigurationSection}\" section cannot found in configuration.");
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
        };
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealTimeNotifier, SignalRRealTimeNotifier>();

builder.Services.AddEndpointsApiExplorer();

WebApiConfiguration corsConfiguration =
    builder.Configuration.GetSection("WebAPIConfiguration").Get<WebApiConfiguration>()
    ?? throw new InvalidOperationException("WebAPIConfiguration section cannot found for CORS.");
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins(corsConfiguration.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // NPM / Docker: gerçek istemci IP ve TLS şeması (X-Forwarded-*) güvenilir
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition(
        name: "Bearer",
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer YOUR_TOKEN\". \r\n\r\n"
                + "`Enter your token in the text input below.`"
        }
    );
    opt.OperationFilter<BearerSecurityRequirementOperationFilter>();
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();

OfflineLicenseOptions offlineLicense =
    app.Configuration.GetSection(OfflineLicenseOptions.SectionName).Get<OfflineLicenseOptions>()
    ?? new OfflineLicenseOptions();
OfflineLicenseGuard.EnsureValidOrThrow(app.Environment.ContentRootPath, offlineLicense);

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.DocExpansion(DocExpansion.None);
    });


app.ConfigureCustomExceptionMiddleware();

app.UseDbMigrationApplier();

await app.Services.SeedDataAsync();

// CORS must be registered BEFORE authentication/authorization and endpoint routing
// so that SignalR negotiate/WebSocket upgrades and controller calls both honour the policy.
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CryoHub>("/hubs/cryo");

app.UseResponseLocalization();

app.Run();
