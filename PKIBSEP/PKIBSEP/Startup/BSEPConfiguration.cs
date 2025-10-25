using Microsoft.EntityFrameworkCore;
using PKIBSEP.Database;
using PKIBSEP.Database.Repository;
using PKIBSEP.Interfaces;
using PKIBSEP.Services;

namespace PKIBSEP.Startup
{
    public static class BSEPConfiguration
    {
        public static IServiceCollection ConfigureBSEP(this IServiceCollection services, IConfiguration configuration)
        {
            SetupCore(services);
            SetupInfrastructure(services,configuration);
            return services;
        }

        private static void SetupCore(IServiceCollection services)
        {
            // Register services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
        }

        private static void SetupInfrastructure(IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
