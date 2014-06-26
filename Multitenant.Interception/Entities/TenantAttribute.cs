using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace Multitenant.Interception.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TenantAttribute : Attribute
    {
        public TenantAttribute(string column)
        {
            ColumnName = column;
        }

        public string ColumnName { get; set; }

        public static string GetTenantColumnName(EdmType type)
        {
            // TODO Find the tenant annotation and get the property name
            //      Name of annotation will be something like: 
            //      http://schemas.microsoft.com/ado/2013/11/edm/customannotation:TenantColumnName

            MetadataProperty annotation =
                type.MetadataProperties.SingleOrDefault(p => p.Name.EndsWith("customannotation:TenantColumnName"));

            return annotation == null ? null : (string)annotation.Value;
        }

    }
}