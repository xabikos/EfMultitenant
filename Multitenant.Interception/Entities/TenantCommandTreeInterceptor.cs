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
    /// <summary>
    /// Custom implementation of <see cref="IDbCommandTreeInterceptor"/> which filters based on tenantId.
    /// </summary>
    public class TenantCommandTreeInterceptor : IDbCommandTreeInterceptor
    {
        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.OriginalResult.DataSpace == DataSpace.SSpace)
            {
                // Check that there is an authenticated user in this context
                var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    return;
                }
                var userIdclaim = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdclaim == null)
                {
                    return;
                }
                
                // In case of query command change the query by adding a filtering based on tenantId 
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

                var userId = userIdclaim.Value;
                if (InterceptInsertCommand(interceptionContext, userId))
                {
                    return;
                }

                if (InterceptUpdate(interceptionContext, userId))
                {
                    return;
                }

                InterceptDeleteCommand(interceptionContext, userId);
            }
        }

        /// <summary>
        /// In case of an insert command we always assign the correct value to the tenantId
        /// </summary>
        private static bool InterceptInsertCommand(DbCommandTreeInterceptionContext interceptionContext, string userId)
        {
            var insertCommand = interceptionContext.Result as DbInsertCommandTree;
            if (insertCommand != null)
            {
                var column = TenantAwareAttribute.GetTenantColumnName(insertCommand.Target.VariableType.EdmType);
                if (!string.IsNullOrEmpty(column))
                {
                    var tenantSetClause =
                        DbExpressionBuilder.SetClause(
                            insertCommand.Target.VariableType.Variable(insertCommand.Target.VariableName)
                                .Property(column), DbExpression.FromString(userId));

                    var filteredSetClauses =
                        insertCommand.SetClauses.Cast<DbSetClause>()
                            .Where(sc => ((DbPropertyExpression)sc.Property).Property.Name != column);

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
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// In case of an update command we always filter based on the tenantId
        /// </summary>
        private static bool InterceptUpdate(DbCommandTreeInterceptionContext interceptionContext, string userId)
        {
            var updateCommand = interceptionContext.Result as DbUpdateCommandTree;
            if (updateCommand != null)
            {
                var column = TenantAwareAttribute.GetTenantColumnName(updateCommand.Target.VariableType.EdmType);
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
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// In case of a delete command we always filter based on the tenantId
        /// </summary>
        private static void InterceptDeleteCommand(DbCommandTreeInterceptionContext interceptionContext, string userId)
        {
            var deleteCommand = interceptionContext.Result as DbDeleteCommandTree;
            if (deleteCommand != null)
            {
                var column = TenantAwareAttribute.GetTenantColumnName(deleteCommand.Target.VariableType.EdmType);

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