using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BuildingBlocks.Infrastructure.Options;
using BuildingBlocks.Abstractions.Auditing;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Infrastructure.Services;
using Modules.Identity.Infrastructure.Auditing;
using Modules.Identity.Infrastructure.Abstractions;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Configuration;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

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
            services.AddScoped<Modules.Identity.Application.Admin.IAdminUsersQuery, Modules.Identity.Infrastructure.Admin.AdminUsersQuery>();
            services.AddScoped<PasswordService>();
            services.AddScoped<TokenService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<Modules.Identity.Contracts.Notifications.INotificationService, Modules.Identity.Infrastructure.Notifications.NotificationService>();
            
            // Background Jobs
            services.AddScoped<Modules.Identity.Contracts.BackgroundJobs.IBackgroundJobService, Modules.Identity.Infrastructure.BackgroundJobs.BackgroundJobService>();
            services.AddScoped<Modules.Identity.Contracts.BackgroundJobs.IJobQueue, Modules.Identity.Infrastructure.BackgroundJobs.JobQueue>();
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.EmailJobHandler>();
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.DataProcessingJobHandler>();
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.ScheduledTaskHandler>();
            
            // Email Service (mock implementation for now)
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.IEmailService, Modules.Identity.Infrastructure.BackgroundJobs.Handlers.MockEmailService>();
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.IDataProcessingService, Modules.Identity.Infrastructure.BackgroundJobs.Handlers.MockDataProcessingService>();
            services.AddScoped<Modules.Identity.Infrastructure.BackgroundJobs.Handlers.IScheduledTaskService, Modules.Identity.Infrastructure.BackgroundJobs.Handlers.MockScheduledTaskService>();

            services.AddValidatedOptions<EmailOptions>(configuration, "Email")
                .Validate(o =>
                    o.Provider.Equals("File", StringComparison.OrdinalIgnoreCase)
                    || o.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase),
                    "Email:Provider must be 'File' or 'Smtp'.")
                .Validate(o =>
                    !o.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
                    || !string.IsNullOrWhiteSpace(o.SmtpHost),
                    "Email:SmtpHost is required when Email:Provider is 'Smtp'.")
                .Validate(o =>
                    !o.Provider.Equals("File", StringComparison.OrdinalIgnoreCase)
                    || !string.IsNullOrWhiteSpace(o.FilePickupDirectory),
                    "Email:FilePickupDirectory is required when Email:Provider is 'File'.");
            services.AddValidatedOptions<PasswordResetOptions>(configuration, "PasswordReset");

            services.AddScoped<IEmailSender>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
                return opt.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
                    ? new SmtpEmailSender(sp.GetRequiredService<IOptions<EmailOptions>>())
                    : new FileEmailSender(sp.GetRequiredService<IOptions<EmailOptions>>());
            });

            services.AddValidatedOptions<JwtOptions>(configuration, "Jwt")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key missing.");

            services.AddValidatedOptions<AuthCookiesOptions>(configuration, "AuthCookies");

            services.AddValidatedOptions<AuthSecurityOptions>(configuration, "AuthSecurity");

            services.AddScoped<IAuditLogger, EfAuditLogger>();
            
            // Register user context service for audit trail
            services.AddScoped<IUserContext, UserContextService>();

            return services;
        }
    }
}
