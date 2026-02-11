using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Abstractions.Domain.Extensions
{
    public static class EntityExtensions
    {
        public static void SoftDelete<T>(this T entity) where T : BaseEntity
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.IsActive = false;
        }

        public static void Restore<T>(this T entity) where T : BaseEntity
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.IsActive = true;
        }

        public static void SetAuditInfo<T>(this T entity, string? updatedBy = null) where T : BaseEntity
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;
        }
    }
}
