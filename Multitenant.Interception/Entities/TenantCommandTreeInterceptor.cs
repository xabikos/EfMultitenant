using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Multitenant.Interception.Entities
{
    public class TenantCommandTreeInterceptor : IDbCommandTreeInterceptor
    {
        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.OriginalResult.DataSpace == DataSpace.SSpace)
            {
                var queryCommand = interceptionContext.Result as DbQueryCommandTree;
                if (queryCommand != null)
                {
                    var newQuery = queryCommand.Query.Accept(new TenantQueryVisitor());
                    interceptionContext.Result = new DbQueryCommandTree(
                        queryCommand.MetadataWorkspace,
                        queryCommand.DataSpace,
                        newQuery);
                    return;
                }

                var insertCommand = interceptionContext.Result as DbInsertCommandTree;
                if (insertCommand != null)
                {
                    var column = TenantAttribute.GetTenantColumnName(insertCommand.Target.VariableType.EdmType);
                    var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                    if (column != null && identity != null)
                    {
                        var userId = identity.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

                        var tenantSetClause =
                            DbExpressionBuilder.SetClause(
                                    DbExpressionBuilder.Property(
                                        DbExpressionBuilder.Variable(insertCommand.Target.VariableType, insertCommand.Target.VariableName),
                                        column),
                                    DbExpression.FromString(userId));

                        var filteredSetClauses =
                            insertCommand.SetClauses.Cast<DbSetClause>()
                                .Where(sc => ((DbPropertyExpression) sc.Property).Property.Name != column);

                        var finalModificationClauses = new List<DbModificationClause>(filteredSetClauses)
                        {
                            tenantSetClause
                        };

                        var newInsertCommand = new DbInsertCommandTree(
                            insertCommand.MetadataWorkspace,
                            insertCommand.DataSpace,
                            insertCommand.Target,
                            finalModificationClauses.AsReadOnly(),
                            insertCommand.Returning);

                        interceptionContext.Result = newInsertCommand;
                    }
                }
            }
        }

    }
}