﻿using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace Multitenant.Interception.Infrastructure
{
    /// <summary>
    /// Attribute used to mark all entities which should be filtered based on tenantId
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TenantAwareAttribute : Attribute
    {
        public const string TenantAnnotation = "TenantAnnotation";
        public const string TenantIdFilterParameterName = "TenantIdParameter";
        
        public string ColumnName { get; private set; }

        public TenantAwareAttribute(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            ColumnName = columnName;
        }

        public static string GetTenantColumnName(EdmType type)
        {
            MetadataProperty annotation =
                type.MetadataProperties.SingleOrDefault(
                    p => p.Name.EndsWith(string.Format("customannotation:{0}", TenantAnnotation)));

            if (annotation == null)
            {
                var clrTypeMetadataPropName = @"http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType";
                var customAnnotationValue = (Type) type.MetadataProperties.SingleOrDefault(p => p.Name == clrTypeMetadataPropName)?.Value;

                if (customAnnotationValue == null)
                {
                    return null;
                }

                var tenantAwareAttribute = customAnnotationValue.CustomAttributes.SingleOrDefault(ca => ca.AttributeType.Name == nameof(TenantAwareAttribute));
                var columnName = tenantAwareAttribute?.ConstructorArguments.FirstOrDefault();
                return (string) columnName?.Value;
            }

            return (string) annotation.Value;
        }
    }
}
