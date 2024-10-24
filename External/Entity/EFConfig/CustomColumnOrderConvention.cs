

namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class CustomColumnOrderConvention : IModelCustomizer
    {
        public void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(MEntity).IsAssignableFrom(entityType.ClrType))
                {
                    Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? idProperty = entityType.FindProperty(nameof(MEntity.Id));
                    idProperty?.SetColumnOrder(0);

                    Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? entityIdProperty = entityType.FindProperty(nameof(MEntity.EntityId));
                    entityIdProperty?.SetColumnOrder(1);

                    PropertyInfo[]? baseProperties = entityType.ClrType.BaseType?.GetProperties();
                    PropertyInfo[] derivedProperties = entityType.ClrType.GetProperties();

                    int order = 2;

                    foreach (PropertyInfo prop in derivedProperties)
                    {
                        Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? propertyBuilder = entityType.FindProperty(prop.Name);
                        if (propertyBuilder != null &&
                            propertyBuilder.Name != nameof(MEntity.Id) &&
                            propertyBuilder.Name != nameof(MEntity.EntityId))
                        {
                            propertyBuilder.SetColumnOrder(order);
                            order++;
                        }
                    }

                    if (baseProperties != null)
                    {
                        foreach (PropertyInfo baseProp in baseProperties)
                        {
                            Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? propertyBuilder = entityType.FindProperty(baseProp.Name);
                            if (propertyBuilder != null &&
                                propertyBuilder.Name != nameof(MEntity.Id) &&
                                propertyBuilder.Name != nameof(MEntity.EntityId))
                            {
                                propertyBuilder.SetColumnOrder(order);
                                order++;
                            }
                        }
                    }
                }
            }
        }


    }

}
