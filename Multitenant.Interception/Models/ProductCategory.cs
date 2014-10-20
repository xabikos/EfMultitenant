using System;

namespace Multitenant.Interception.Models
{
    
    public class ProductCategory : TenantEntity
    {
        public long Id { get; set; }

        public string Category { get; set; }

    }
}