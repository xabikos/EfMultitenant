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

                var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    return;
                }
                var userIdclaim = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdclaim == null) {
                    return;
                }
                var userId = userIdclaim.Value;

                var insertCommand = interceptionContext.Result as DbInsertCommandTree;
                if (insertCommand != null)
                {
                    var column = TenantAttribute.GetTenantColumnName(insertCommand.Target.VariableType.EdmType);
                    if (!string.IsNullOrEmpty(column))
                    {
                        var tenantSetClause =
                            DbExpressionBuilder.SetClause(
                                insertCommand.Target.VariableType.Variable(insertCommand.Target.VariableName)
                                    .Property(column), DbExpression.FromString(userId));

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
                        return;
                    }
                }

                var updateCommand = interceptionContext.Result as DbUpdateCommandTree;
                if (updateCommand != null)
                {
                    var column = TenantAttribute.GetTenantColumnName(updateCommand.Target.VariableType.EdmType);
                    if (!string.IsNullOrEmpty(column))
                    {
                        // Remove from set clauses the userId in case it was added by accident
                        var setClausesWithoutSiteId =
                            updateCommand.SetClauses.Cast<DbSetClause>()
                                .Where(sc => ((DbPropertyExpression) sc.Property).Property.Name != column);

                        var userIdPropertyExpression =
                            updateCommand.Target.VariableType.Variable(updateCommand.Target.VariableName)
                                .Property(column);
                        var userIdExpression = userIdPropertyExpression.Equal(DbExpression.FromString(userId));

                        var newUpdateCommand = new DbUpdateCommandTree(
                            updateCommand.MetadataWorkspace,
                            updateCommand.DataSpace,
                            updateCommand.Target,
                            updateCommand.Predicate.And(userIdExpression),
                            new List<DbModificationClause>(setClausesWithoutSiteId).AsReadOnly(),
                            updateCommand.Returning);

                        interceptionContext.Result = newUpdateCommand;
                        return;
                    }
                }

                var deleteCommand = interceptionContext.Result as DbDeleteCommandTree;
                if (deleteCommand != null)
                {
                    var column = TenantAttribute.GetTenantColumnName(deleteCommand.Target.VariableType.EdmType);

                    if (!string.IsNullOrEmpty(column))
                    {
                        var userIdPropertyExpression =
                            deleteCommand.Target.VariableType.Variable(deleteCommand.Target.VariableName)
                                .Property(column);
                        var userIdExpression = userIdPropertyExpression.Equal(DbExpression.FromString(userId));

                        var newDeleteCommand = new DbDeleteCommandTree(
                            deleteCommand.MetadataWorkspace,
                            deleteCommand.DataSpace,
                            deleteCommand.Target,
                            deleteCommand.Predicate.And(userIdExpression));

                        interceptionContext.Result = newDeleteCommand;
                    }
                }
            }
        }

    }
}