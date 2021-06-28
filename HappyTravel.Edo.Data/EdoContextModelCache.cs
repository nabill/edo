using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HappyTravel.Edo.Data
{
    internal static class EdoContextModelExtensions
    {
        public static EntityDbMappingInfo GetEntityInfo<TEntity>(this EdoContext context)
        {
            var entityType = typeof(TEntity);
            return EntityInfos.GetOrAdd(entityType, (prop, dbContext) =>
                {
                    var entity = dbContext.Model.FindEntityType(entityType);
                    var tableName = entity.GetTableName();
                    return new EntityDbMappingInfo()
                    {
                        Table = tableName,
                        Schema = entity.GetSchema() ?? DefaultSchema,
                        PropertyMapping = entity.GetProperties()
                            .ToDictionary(property => property.Name, property => property.GetDefaultColumnName(StoreObjectIdentifier.Table(tableName, DefaultSchema)))
                    };
                },
                context);
        }
        
        
        private const string DefaultSchema = "public";

        private static readonly ConcurrentDictionary<Type, EntityDbMappingInfo> EntityInfos = new ConcurrentDictionary<Type, EntityDbMappingInfo>();
    }
}