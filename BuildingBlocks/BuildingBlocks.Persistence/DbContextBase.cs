using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence;

public abstract class DbContextBase : DbContext, IUnitOfWork
{
    private readonly string? _userId;

    protected DbContextBase(DbContextOptions options, string? userId = null) : base(options)
    {
        _userId = userId;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy ??= _userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy ??= _userId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                entry.Entity.DeletedBy ??= _userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
