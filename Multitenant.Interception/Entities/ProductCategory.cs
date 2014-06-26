using System;
using Multitenant.Interception.Models;

namespace Multitenant.Interception.Entities
{
    [Tenant("TenantId")]
    public class ProductCategory
    {
        public long Id { get; set; }

        public string Category { get; set; }

        public ApplicationUser User { get; set; }

        /// <summary>
        /// In this case this is the User Id 
        /// as each user is able to access only his own products
        /// </summary>
        public string TenantId { get; set; }

    }
}