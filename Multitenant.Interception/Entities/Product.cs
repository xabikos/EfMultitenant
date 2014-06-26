using System;
using Multitenant.Interception.Models;

namespace Multitenant.Interception.Entities
{
    public class Product
    {

        public long Id { get; set; }

        /// <summary>
        /// In this case this is the User Id 
        /// as each user is able to access only his own products
        /// </summary>
        public string TenantId { get; set; }

        public ApplicationUser User { get; set; }
        
        public string Description { get; set; }

        public long CategoryId { get; set; }

        public ProductCategory Category { get; set; }

    }
}