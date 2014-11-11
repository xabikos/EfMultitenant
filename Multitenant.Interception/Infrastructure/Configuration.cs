using System;
using System.Data.Entity;

namespace Multitenant.Interception.Infrastructure
{
    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            AddInterceptor(new TenantCommandInterceptor());
            AddInterceptor(new TenantCommandTreeInterceptor());
        }

    }
}