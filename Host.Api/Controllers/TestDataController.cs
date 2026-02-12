using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Modules.Identity.Infrastructure.Services;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestDataController : ControllerBase
{
    private readonly AuthDbContext _context;

    public TestDataController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpPost("i18n/test-data")]
    public async Task<IActionResult> SeedTestData()
    {
        try
        {
            // Clear existing test data
            var existingTestResources = await _context.I18nResources
                .Where(r => r.Key.StartsWith("test."))
                .ToListAsync();
            
            if (existingTestResources.Any())
            {
                _context.I18nResources.RemoveRange(existingTestResources);
                await _context.SaveChangesAsync();
            }

            // Add comprehensive test data
            var testResources = new List<I18nResource>
            {
                // Turkish test data with special characters
                new() { TenantId = null, Key = "test.turkish.çalışma", Lang = "tr", Value = "Çalışma alanı yönetimi", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.şifre", Lang = "tr", Value = "Şifre sıfırlama linki", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.güvenlik", Lang = "tr", Value = "Güvenlik olayları", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.işlem", Lang = "tr", Value = "İşlem tamamlandı", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.özellik", Lang = "tr", Value = "Özellik yönetimi", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.ürün", Lang = "tr", Value = "Ürün geliştirme", VersionNo = 1 },

                // English equivalents
                new() { TenantId = null, Key = "test.turkish.çalışma", Lang = "en", Value = "Workspace management", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.şifre", Lang = "en", Value = "Password reset link", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.güvenlik", Lang = "en", Value = "Security events", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.işlem", Lang = "en", Value = "Operation completed", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.özellik", Lang = "en", Value = "Feature management", VersionNo = 1 },
                new() { TenantId = null, Key = "test.turkish.ürün", Lang = "en", Value = "Product development", VersionNo = 1 },

                // Page-specific test data
                new() { TenantId = null, Key = "test.pages.login.title", Lang = "tr", Value = "Giriş Yap", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.login.title", Lang = "en", Value = "Sign In", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.dashboard.title", Lang = "tr", Value = "Panel", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.dashboard.title", Lang = "en", Value = "Dashboard", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.users.title", Lang = "tr", Value = "Kullanıcılar", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.users.title", Lang = "en", Value = "Users", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.tenants.title", Lang = "tr", Value = "Tenantlar", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.tenants.title", Lang = "en", Value = "Tenants", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.roles.title", Lang = "tr", Value = "Roller", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.roles.title", Lang = "en", Value = "Roles", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.settings.title", Lang = "tr", Value = "Ayarlar", VersionNo = 1 },
                new() { TenantId = null, Key = "test.pages.settings.title", Lang = "en", Value = "Settings", VersionNo = 1 },

                // Additional page titles
                new() { TenantId = null, Key = "pages.users.title", Lang = "tr", Value = "Kullanıcı Yönetimi", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.users.title", Lang = "en", Value = "User Management", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.users.description", Lang = "tr", Value = "Platform kullanıcılarını yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.users.description", Lang = "en", Value = "Manage platform users", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.tenants.title", Lang = "tr", Value = "Tenant Yönetimi", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenants.title", Lang = "en", Value = "Tenant Management", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenants.description", Lang = "tr", Value = "Tenantları yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenants.description", Lang = "en", Value = "Manage tenants", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.roles.title", Lang = "tr", Value = "Rol Yönetimi", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.roles.title", Lang = "en", Value = "Role Management", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.roles.description", Lang = "tr", Value = "Sistem rollerini yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.roles.description", Lang = "en", Value = "Manage system roles", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.dashboard.title", Lang = "tr", Value = "Panel", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.dashboard.title", Lang = "en", Value = "Dashboard", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.dashboard.description", Lang = "tr", Value = "Sistem durumunu görüntüleyin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.dashboard.description", Lang = "en", Value = "View system status", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.settings.title", Lang = "tr", Value = "Ayarlar", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.settings.title", Lang = "en", Value = "Settings", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.settings.description", Lang = "tr", Value = "Sistem ayarlarını yapılandırın", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.settings.description", Lang = "en", Value = "Configure system settings", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.security_events.title", Lang = "tr", Value = "Güvenlik Olayları", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.security_events.title", Lang = "en", Value = "Security Events", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.security_events.description", Lang = "tr", Value = "Sistem güvenlik olaylarını görüntüleyin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.security_events.description", Lang = "en", Value = "View system security events", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.audit.title", Lang = "tr", Value = "Denetim Kayıtları", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.audit.title", Lang = "en", Value = "Audit Logs", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.audit.description", Lang = "tr", Value = "Sistem denetim kayıtlarını görüntüleyin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.audit.description", Lang = "en", Value = "View system audit logs", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.feature_flags.title", Lang = "tr", Value = "Özellik Bayrakları", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.feature_flags.title", Lang = "en", Value = "Feature Flags", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.feature_flags.description", Lang = "tr", Value = "Sistem özelliklerini yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.feature_flags.description", Lang = "en", Value = "Manage system features", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.plans.title", Lang = "tr", Value = "Abonelik Planları", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.plans.title", Lang = "en", Value = "Subscription Plans", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.plans.description", Lang = "tr", Value = "Abonelik planlarını yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.plans.description", Lang = "en", Value = "Manage subscription plans", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.rate_limit.title", Lang = "tr", Value = "Hız Limitleri", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.rate_limit.title", Lang = "en", Value = "Rate Limits", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.rate_limit.description", Lang = "tr", Value = "API hız limitlerini yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.rate_limit.description", Lang = "en", Value = "Manage API rate limits", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.sessions.title", Lang = "tr", Value = "Oturumlar", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.sessions.title", Lang = "en", Value = "Sessions", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.sessions.description", Lang = "tr", Value = "Kullanıcı oturumlarını yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.sessions.description", Lang = "en", Value = "Manage user sessions", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.webhooks.title", Lang = "tr", Value = "Webhook'lar", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.webhooks.title", Lang = "en", Value = "Webhooks", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.webhooks.description", Lang = "tr", Value = "Webhook'ları yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.webhooks.description", Lang = "en", Value = "Manage webhooks", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.tenant_usage.title", Lang = "tr", Value = "Tenant Kullanımı", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenant_usage.title", Lang = "en", Value = "Tenant Usage", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenant_usage.description", Lang = "tr", Value = "Tenant kullanım istatistiklerini görüntüleyin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.tenant_usage.description", Lang = "en", Value = "View tenant usage statistics", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.user_roles.title", Lang = "tr", Value = "Kullanıcı Rolleri", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.user_roles.title", Lang = "en", Value = "User Roles", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.user_roles.description", Lang = "tr", Value = "Kullanıcı rollerini yönetin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.user_roles.description", Lang = "en", Value = "Manage user roles", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.forgot_password.title", Lang = "tr", Value = "Şifremi Unuttum", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.forgot_password.title", Lang = "en", Value = "Forgot Password", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.forgot_password.description", Lang = "tr", Value = "Şifre sıfırlama linki gönderin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.forgot_password.description", Lang = "en", Value = "Send password reset link", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.register.title", Lang = "tr", Value = "Kayıt Ol", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.register.title", Lang = "en", Value = "Register", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.register.description", Lang = "tr", Value = "Yeni hesap oluşturun", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.register.description", Lang = "en", Value = "Create new account", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.reset_password.title", Lang = "tr", Value = "Şifre Sıfırlama", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.reset_password.title", Lang = "en", Value = "Reset Password", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.reset_password.description", Lang = "tr", Value = "Şifrenizi sıfırlayın", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.reset_password.description", Lang = "en", Value = "Reset your password", VersionNo = 1 },

                new() { TenantId = null, Key = "pages.health.title", Lang = "tr", Value = "Sistem Sağlığı", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.health.title", Lang = "en", Value = "System Health", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.health.description", Lang = "tr", Value = "Sistem sağlık durumunu görüntüleyin", VersionNo = 1 },
                new() { TenantId = null, Key = "pages.health.description", Lang = "en", Value = "View system health status", VersionNo = 1 },

                // Common actions
                new() { TenantId = null, Key = "test.common.save", Lang = "tr", Value = "Kaydet", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.save", Lang = "en", Value = "Save", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.cancel", Lang = "tr", Value = "İptal", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.cancel", Lang = "en", Value = "Cancel", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.edit", Lang = "tr", Value = "Düzenle", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.edit", Lang = "en", Value = "Edit", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.delete", Lang = "tr", Value = "Sil", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.delete", Lang = "en", Value = "Delete", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.loading", Lang = "tr", Value = "Yükleniyor...", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.loading", Lang = "en", Value = "Loading...", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.search", Lang = "tr", Value = "Ara", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.search", Lang = "en", Value = "Search", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.logout", Lang = "tr", Value = "Çıkış Yap", VersionNo = 1 },
                new() { TenantId = null, Key = "test.common.logout", Lang = "en", Value = "Logout", VersionNo = 1 },
            };

            await _context.I18nResources.AddRangeAsync(testResources);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Test data seeded successfully",
                count = testResources.Count,
                turkish_chars_detected = testResources.Any(r => 
                    r.Value.Contains("ç") || r.Value.Contains("ş") || 
                    r.Value.Contains("ğ") || r.Value.Contains("ı") ||
                    r.Value.Contains("ö") || r.Value.Contains("ü"))
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    //[HttpPost("create-demo-data")]
    //public async Task<IActionResult> CreateDemoData()
    //{
    //    try
    //    {
    //        // Create demo users with Turkish characters
    //        var passwordService = new Modules.Identity.Infrastructure.Services.PasswordService();
    //        var demoUsers = new[]
    //        {
    //            new AppUser
    //            {
    //                Email = "admin@demo.com",
    //                UserName = "admin@demo.com",
    //                FirstName = "Demo",
    //                LastName = "Admin",
    //                IsActive = true,
    //                IsAdmin = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Admin123!")
    //            },
    //            new AppUser
    //            {
    //                Email = "calisma@demo.com",
    //                UserName = "calisma@demo.com",
    //                FirstName = "Çalışma",
    //                LastName = "Demo",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Calisma123!")
    //            },
    //            new AppUser
    //            {
    //                Email = "guvenlik@demo.com",
    //                UserName = "guvenlik@demo.com",
    //                FirstName = "Güvenlik",
    //                LastName = "User",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Guvenlik123!")
    //            }
    //        };

    //        // Add to database
    //        await _context.Users.AddRangeAsync(demoUsers);
    //        await _context.SaveChangesAsync();

    //        return Ok(new { 
    //            message = "Demo data created successfully",
    //            users_created = demoUsers.Length,
    //            turkish_chars_in_data = demoUsers.Any(u => u.Email.Contains("ç") || u.FirstName.Contains("ç"))
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { error = ex.Message });
    //    }
    //}
    //{
    //    try
    //    {
    //        // Create demo users with Turkish characters
    //        var passwordService = new Modules.Identity.Infrastructure.Services.PasswordService();
    //        var demoUsers = new[]
    //        {
    //            new AppUser
    //            {
    //                Email = "admin@demo.com",
    //                UserName = "admin@demo.com",
    //                FirstName = "Demo",
    //                LastName = "Admin",
    //                IsActive = true,
    //                IsAdmin = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Admin123!")
    //            },
    //            new AppUser
    //            {
    //                Email = "calisma@demo.com",
    //                UserName = "calisma@demo.com",
    //                FirstName = "Çalışma",
    //                LastName = "Demo",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Calisma123!")
    //            },
    //            new AppUser
    //            {
    //                Email = "guvenlik@demo.com",
    //                UserName = "guvenlik@demo.com",
    //                FirstName = "Güvenlik",
    //                LastName = "User",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo",
    //                PasswordHash = passwordService.Hash("Guvenlik123!")
    //            }
    //        };

    //        // Create demo tenants with Turkish characters
    //        var demoTenants = new[]
    //        {
    //            new Tenant
    //            {
    //                Name = "Demo Tenant Çalışma",
    //                Domain = "demo-çalışma.example.com",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            },
    //            new Tenant
    //            {
    //                Name = "Güvenlik Tenant",
    //                Domain = "güvenlik-tenant.example.com",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            },
    //            new Tenant
    //            {
    //                Name = "Özellik Tenant",
    //                Domain = "özellik-tenant.example.com",
    //                IsActive = true,
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            }
    //        };

    //        // Create demo roles with Turkish characters
    //        var demoRoles = new[]
    //        {
    //            new AppRole
    //            {
    //                Name = "DemoAdmin",
    //                Description = "Demo administrator rolü",
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            },
    //            new AppRole
    //            {
    //                Name = "ÇalışmaUser",
    //                Description = "Çalışma alanı kullanıcısı rolü",
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            },
    //            new AppRole
    //            {
    //                Name = "GüvenlikUser",
    //                Description = "Güvenlik kullanıcısı rolü",
    //                CreatedAt = DateTime.UtcNow,
    //                UpdatedAt = DateTime.UtcNow,
    //                CreatedBy = "demo",
    //                UpdatedBy = "demo"
    //            }
    //        };

    //        // Add to database
    //        await _context.Users.AddRangeAsync(demoUsers);
    //        await _context.Tenants.AddRangeAsync(demoTenants);
    //        await _context.Roles.AddRangeAsync(demoRoles);
    //        await _context.SaveChangesAsync();

    //        return Ok(new { 
    //            message = "Demo data created successfully",
    //            users_created = demoUsers.Length,
    //            tenants_created = demoTenants.Length,
    //            roles_created = demoRoles.Length,
    //            turkish_chars_in_data = demoUsers.Any(u => u.Email.Contains("ç") || u.FirstName.Contains("ç")) ||
    //                                   demoTenants.Any(t => t.Name.Contains("ç") || t.Domain.Contains("ç")) ||
    //                                   demoRoles.Any(r => r.Name.Contains("ç") || r.Description.Contains("ç"))
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { error = ex.Message });
    //    }
    //}

    [HttpGet("test-summary")]
    public async Task<IActionResult> GetTestSummary()
    {
        try
        {
            var summary = new
            {
                timestamp = DateTime.UtcNow,
                database = new
                {
                    users = await _context.Users.CountAsync(),
                    tenants = await _context.Tenants.CountAsync(),
                    roles = await _context.Roles.CountAsync(),
                    i18n_resources = await _context.I18nResources.CountAsync()
                },
                turkish_support = new
                {
                    turkish_users = await _context.Users.Where(u => u.Email.Contains("ç") || u.FirstName.Contains("ç")).CountAsync(),
                    turkish_tenants = await _context.Tenants.Where(t => t.Name.Contains("ç") || t.Domain.Contains("ç")).CountAsync(),
                    turkish_roles = await _context.Roles.Where(r => r.Name.Contains("ç") || r.Description.Contains("ç")).CountAsync(),
                    turkish_i18n = await _context.I18nResources
                        .Where(r => r.Lang == "tr" && (r.Value.Contains("ç") || r.Value.Contains("ş") || r.Value.Contains("ğ") || r.Value.Contains("ı") || r.Value.Contains("ö") || r.Value.Contains("ü")))
                        .CountAsync()
                },
                api_endpoints = new
                {
                    i18n_bundle_tr = "GET /api/i18n/bundle?lang=tr",
                    i18n_bundle_en = "GET /api/i18n/bundle?lang=en",
                    test_i18n_data = "POST /api/test/i18n/test-data",
                    create_demo_data = "POST /api/test/create-demo-data",
                    test_summary = "GET /api/test/test-summary"
                }
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("cleanup-test-data")]
    public async Task<IActionResult> CleanupTestData()
    {
        try
        {
            // Remove test data
            var testUsers = await _context.Users.Where(u => u.Email.Contains("demo") || u.FirstName == "Demo").ToListAsync();
            var testTenants = await _context.Tenants.Where(t => t.Name.Contains("Demo")).ToListAsync();
            var testRoles = await _context.Roles.Where(r => r.Name.Contains("Demo") || r.Name.Contains("Çalışma") || r.Name.Contains("Güvenlik")).ToListAsync();
            var testI18n = await _context.I18nResources.Where(r => r.Key.StartsWith("test.")).ToListAsync();

            _context.Users.RemoveRange(testUsers);
            _context.Tenants.RemoveRange(testTenants);
            _context.Roles.RemoveRange(testRoles);
            _context.I18nResources.RemoveRange(testI18n);

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Test data cleaned up successfully",
                users_removed = testUsers.Count,
                tenants_removed = testTenants.Count,
                roles_removed = testRoles.Count,
                i18n_resources_removed = testI18n.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
