using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Identity.Infrastructure.Persistence
{
    public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            // Migration çalıştırırken varsayılan olarak Host.Api appsettings'i de okuyabilsin:
            var basePath = Directory.GetCurrentDirectory();

            // Eğer komutu Host.Api klasöründen çalıştırırsan bu zaten doğru olur.
            // Infra'dan çalıştırırsan, path'i Host.Api'ye göre ayarlamak isteyebilirsin.
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? config["ConnectionStrings:DefaultConnection"]
                     ?? throw new InvalidOperationException("DefaultConnection missing.");

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer(cs);

            // For design-time operations, we can create a null service provider
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            return new AuthDbContext(optionsBuilder.Options, serviceProvider);
        }
    }
}
