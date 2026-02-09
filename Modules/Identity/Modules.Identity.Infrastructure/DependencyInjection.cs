using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Identity.Application.Admin;
using Modules.Identity.Infrastructure.Abstractions;
using Modules.Identity.Infrastructure.Admin;
using Modules.Identity.Infrastructure.Configuration;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var cs = configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("DefaultConnection missing.");

            services.AddDbContext<AuthDbContext>(opt =>
            {
                opt.UseSqlServer(cs, sql =>
                {
                    sql.EnableRetryOnFailure(5);
                    sql.CommandTimeout(30);
                });
            });
            //services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<TokenService>();

            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services.Configure<PasswordResetOptions>(configuration.GetSection("PasswordReset"));

            services.AddScoped<IEmailSender>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
                return opt.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
                    ? new SmtpEmailSender(sp.GetRequiredService<IOptions<EmailOptions>>())
                    : new FileEmailSender(sp.GetRequiredService<IOptions<EmailOptions>>());
            });

            services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key missing.")
            .ValidateOnStart();

            services.AddOptions<AuthCookiesOptions>()
                .Bind(configuration.GetSection("AuthCookies"))
                .ValidateOnStart();

            services.AddOptions<AuthSecurityOptions>()
                .Bind(configuration.GetSection("AuthSecurity"))
                .ValidateOnStart();


            services.AddScoped<IAdminUsersQuery, AdminUsersQuery>();





            return services;
        }
    }
}
