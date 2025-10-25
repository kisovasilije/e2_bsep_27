using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Text;
using PKIBSEP.Common;

namespace PKIBSEP.Startup
{
    public static class JwtStartup
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSection = configuration.GetSection("Jwt");
                    var issuer = jwtSection["Issuer"];
                    var audience = jwtSection["Audience"];
                    var secret = jwtSection["SecretKey"];

                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("adminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("caUserPolicy", policy => policy.RequireRole("CAUser"));
                options.AddPolicy("regularUserPolicy", policy => policy.RequireRole("RegularUser"));
            });

            return services;
        }
    }
}
