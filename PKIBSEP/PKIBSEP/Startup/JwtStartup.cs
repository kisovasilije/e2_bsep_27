using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
                    var secret = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey missing");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("adminPolicy", p => p.RequireRole("Admin"));
                options.AddPolicy("caUserPolicy", p => p.RequireRole("CAUser"));
                options.AddPolicy("regularUserPolicy", p => p.RequireRole("RegularUser"));
                options.AddPolicy("adminOrCaPolicy", p => p.RequireRole("Admin", "CAUser"));
            });

            return services;
        }
    }
}
