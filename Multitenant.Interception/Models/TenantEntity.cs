using System;
using Multitenant.Interception.Infrastructure;

namespace Multitenant.Interception.Models
{
    /// <summary>
    /// Base class that all entities which support multitenancy should derive from.
    /// </summary>
    [TenantAware("TenantId")]
    public class TenantEntity
    {
        /// <summary>
        /// In this case this is the User Id 
        /// as each user is able to access only his own entities
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// The user the entity belongs to
        /// </summary>
        public ApplicationUser User { get; set; }
    }
}