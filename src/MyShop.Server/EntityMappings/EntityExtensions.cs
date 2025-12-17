using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Server.EntityMappings;

public static class EntityExtensions
{
    public static void Patch<T, TDto>(this T entity, TDto dto)
    {
        var dtoProps = typeof(TDto).GetProperties();
        var entityProps = typeof(T).GetProperties();

        foreach (var dtoProp in dtoProps)
        {
            var value = dtoProp.GetValue(dto);
            if (value == null) continue; // only update non-null values

            var entityProp = entityProps.FirstOrDefault(p => p.Name == dtoProp.Name);
            if (entityProp == null) continue;
            if (!entityProp.CanWrite) continue;

            // Do not update foreign key navigation properties
            if (!entityProp.PropertyType.IsValueType && entityProp.PropertyType != typeof(string))
                continue;

            // Handle enum conversion for status properties
            if (entityProp.PropertyType.IsEnum && dtoProp.PropertyType == typeof(string))
            {
                var stringValue = value as string;
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    object? enumValue = null;

                    // Convert based on the target enum type
                    if (entityProp.PropertyType == typeof(ProductStatus))
                    {
                        enumValue = StatusEnumExtensions.ParseApiString<ProductStatus>(stringValue);
                    }
                    else if (entityProp.PropertyType == typeof(OrderStatus))
                    {
                        enumValue = StatusEnumExtensions.ParseApiString<OrderStatus>(stringValue);
                    }
                    else if (entityProp.PropertyType == typeof(PaymentStatus))
                    {
                        enumValue = StatusEnumExtensions.ParseApiString<PaymentStatus>(stringValue);
                    }
                    else if (entityProp.PropertyType == typeof(AgentRequestStatus))
                    {
                        enumValue = StatusEnumExtensions.ParseApiString<AgentRequestStatus>(stringValue);
                    }

                    if (enumValue != null)
                    {
                        entityProp.SetValue(entity, enumValue);
                    }
                }
                continue;
            }

            entityProp.SetValue(entity, value);
        }
    }
}
