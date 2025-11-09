
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

            entityProp.SetValue(entity, value);
        }
    }
}
