using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace Multitenant.Interception.Entities
{
    /// <summary>
    /// Attribute used to mark all entities which should be filtered based on tenantId
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TenantAwareAttribute : Attribute
    {
        public const string TenantIdCollumnName = "TenantId";
        
        public static string GetTenantColumnName(EdmType type)
        {
            MetadataProperty annotation =
                type.MetadataProperties.SingleOrDefault(p => p.Name.EndsWith("customannotation:TenantColumnName"));

            return annotation == null ? null : (string)annotation.Value;
        }

    }
}