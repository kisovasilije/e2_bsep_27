using Microsoft.EntityFrameworkCore;
using PKIBSEP.Crypto.X509;
using PKIBSEP.Database;
using PKIBSEP.Database.Repository;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models.Mappers;
using PKIBSEP.Services;
using PKIBSEP.Services.Crypto;
using PKIBSEP.Services.Issuance;
using PKIBSEP.Services.Keystore;
using PKIBSEP.Services.Security;

namespace PKIBSEP.Startup
{
    public static class BSEPConfiguration
    {
        public static IServiceCollection ConfigureBSEP(this IServiceCollection services, IConfiguration configuration)
        {
            SetupCore(services);
            SetupInfrastructure(services,configuration);

            services.AddAutoMapper(typeof(PkibsepProfile));

            return services;
        }

        private static void SetupCore(IServiceCollection services)
        {
            // Register services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IChainService, BouncyCastleChainService>();
            services.AddScoped<IIssuerValidationService, IssuerValidationService>();
            services.AddScoped<IKeystoreService,KeystoreService>();
            services.AddScoped<ICaUserKeyService, CaUserKeyService>();
            services.AddScoped<ICertificateIssuerService, CertificateIssuerService>();
            services.AddScoped<ICertificateGenerator, CertificateGenerator>();
            services.AddScoped<IKeyService, KeyService>();
            services.AddScoped<IPasswordService, PasswordService>();
        }

        private static void SetupInfrastructure(IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IPasswordRepository, PasswordRepository>();

            services.AddScoped<ICertificateRepository, CertificateRepository>();
            services.AddScoped<ICaUserKeyRepository, CaUserKeyRepository>();
            services.AddScoped<ICaAssignmentRepository, CaAssignmentRepository>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
