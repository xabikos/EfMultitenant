using System;
using System.Data.Entity;

namespace Multitenant.Interception.Entities
{
    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            AddInterceptor(new TenantInterceptor());
        }

    }
}