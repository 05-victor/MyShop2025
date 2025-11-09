using System;

namespace MyShop.Server.Factories
{
    /// <summary>
    /// Base class for all factories.
    /// Provides common methods to set audit fields, assign IDs, and set default statuses.
    /// </summary>
    public abstract class BaseFactory<TEntity, TRequest>
        where TEntity : class
        where TRequest : class
    {
        /// <summary>
        /// Each derived factory will implement specific initialization logic.
        /// </summary>
        public abstract TEntity Create(TRequest request);

        /// <summary>
        /// Set common audit fields for the entity.
        /// </summary>
        protected void SetAuditFields(dynamic entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Generate a GUID for the entity (if it has an Id property).
        /// </summary>
        protected void AssignNewId(dynamic entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
        }

        /// <summary>
        /// Set default status if the entity has a "Status" property.
        /// </summary>
        //protected void SetDefaultStatus(dynamic entity, string defaultStatus = "ACTIVE")
        //{
        //    var statusProperty = entity.GetType().GetProperty("Status");
        //    if (statusProperty != null)
        //    {
        //        var currentValue = statusProperty.GetValue(entity) as string;
        //        if (string.IsNullOrWhiteSpace(currentValue))
        //        {
        //            statusProperty.SetValue(entity, defaultStatus);
        //        }
        //    }
        //}
    }
}
