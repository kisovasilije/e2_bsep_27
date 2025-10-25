namespace PKIBSEP.Startup
{
    public static class CorsConfiguration
    {
        private const string _corsPolicyName = "_allowDevClients";

        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicyName, policy =>
                {
                    policy.WithOrigins(configuration["AppSettings:FrontendUrl"]!)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}
