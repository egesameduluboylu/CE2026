using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence
{
    public static class SoftDeletableQueryFilter
    {
        public static ModelBuilder ApplySoftDeletableQueryFilter(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseEntity>().HasQueryFilter(e => !e.IsDeleted);
            return modelBuilder;
        }
    }
}
