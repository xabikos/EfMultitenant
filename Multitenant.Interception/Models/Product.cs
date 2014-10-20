using System;

namespace Multitenant.Interception.Models
{
    public class Product : TenantEntity
    {

        public long Id { get; set; }

        public string Description { get; set; }

        public long CategoryId { get; set; }

        public ProductCategory Category { get; set; }

    }
}