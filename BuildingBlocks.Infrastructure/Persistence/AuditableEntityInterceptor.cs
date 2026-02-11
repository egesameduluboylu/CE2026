using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext _userContext;

        public AuditableEntityInterceptor(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var entries = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || 
                          e.State == EntityState.Modified || 
                          e.State == EntityState.Deleted);

            var currentUser = _userContext.UserId ?? _userContext.UserName ?? "system";
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var entity = entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                    entity.CreatedBy = currentUser;
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = currentUser;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entity.IsDeleted = true;
                    entity.DeletedAt = now;
                    entity.DeletedBy = currentUser;
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = currentUser;
                    
                    // Change state to Modified instead of Deleted for soft delete
                    entry.State = EntityState.Modified;
                }
            }
        }
    }
}
