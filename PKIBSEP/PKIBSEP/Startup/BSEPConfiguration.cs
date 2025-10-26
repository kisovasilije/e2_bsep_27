using Microsoft.EntityFrameworkCore;
using PKIBSEP.Database;
using PKIBSEP.Database.Repository;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models.Mappers;
using PKIBSEP.Services;

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
        }

        private static void SetupInfrastructure(IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
