using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Multitenant.Interception.Entities
{
    internal class TenantCommandInterceptor : IDbCommandInterceptor
    {
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            SetTenantParameterValue(command);
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            SetTenantParameterValue(command);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            SetTenantParameterValue(command);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        private void SetTenantParameterValue(DbCommand command)
        {
            var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            if ((command == null) || (command.Parameters.Count == 0) || identity == null)
            {
                return;
            }
            var userClaim = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
            {
                var userId = userClaim.Value;
                foreach (DbParameter param in command.Parameters)
                {
                    if (!param.ParameterName.StartsWith("TenantId"))
                        return;
                    param.Value = userId;
                }
            }
        }

    }
}