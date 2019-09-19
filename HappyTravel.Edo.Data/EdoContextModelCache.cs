using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Data
{
    internal static class EdoContextModelExtensions
    {
        public static EntityDbMappingInfo GetEntityInfo<TEntity>(this EdoContext context)
        {
            var entityType = typeof(TEntity);
            return EntityInfos.GetOrAdd(entityType, (prop, dbContext) =>
                {
                    var ent = dbContext.Model.FindEntityType(entityType);
                    return new EntityDbMappingInfo()
                    {
                        Table = ent.Relational().TableName,
                        Schema = ent.Relational().Schema ?? DefaultSchema,
                        PropertyMapping = ent.GetProperties()
                            .ToDictionary(property => property.Name, property => property.Relational().ColumnName)
                    };
                },
                context);
        }
        
        private const string DefaultSchema = "public";

        private static readonly ConcurrentDictionary<Type, EntityDbMappingInfo> EntityInfos = new ConcurrentDictionary<Type, EntityDbMappingInfo>();
    }
}