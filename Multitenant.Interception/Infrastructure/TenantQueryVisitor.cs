using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;

namespace Multitenant.Interception.Infrastructure
{
    /// <summary>
    /// Visitor pattern implementation class that adds filtering for tenantId column if applicable
    /// </summary>
    public class TenantQueryVisitor : DefaultExpressionVisitor
    {
        /// <summary>
        /// Flag prevents applying the custom filtering twice per query 
        /// </summary>
        private bool _injectedDynamicFilter;
        private readonly bool _isSSpace;
        
        public TenantQueryVisitor(DataSpace originalResultDataSpace)
        {
            _isSSpace = originalResultDataSpace == DataSpace.SSpace;
        }

        /// <summary>
        /// This method called before the one below it when a filtering is already exists in the query (e.g. fetch an entity by id)
        /// so we apply the dynamic filtering at this level
        /// </summary>
        public override DbExpression Visit(DbFilterExpression expression)
        {
            var column = TenantAwareAttribute.GetTenantColumnName(expression.Input.Variable.ResultType.EdmType);
            if (!_injectedDynamicFilter && !string.IsNullOrEmpty(column) && !_isSSpace)
            {
                var newFilterExpression = BuildFilterExpression(expression.Input, expression.Predicate, column);
                if (newFilterExpression != null)
                {
                    //  If not null, a new DbFilterExpression has been created with our dynamic filters.
                    return base.Visit(newFilterExpression);
                }
                
            }
            return base.Visit(expression);
        }

        public override DbExpression Visit(DbScanExpression expression)
        {
            var column = TenantAwareAttribute.GetTenantColumnName(expression.Target.ElementType);
            if (!_injectedDynamicFilter && !string.IsNullOrEmpty(column) && !_isSSpace)
            {
                // Get the current expression
                var dbExpression = base.Visit(expression);
                // Get the current expression binding 
                var currentExpressionBinding = DbExpressionBuilder.Bind(dbExpression);
                var newFilterExpression = BuildFilterExpression(currentExpressionBinding, null, column);
                if (newFilterExpression != null)
                {
                    // If not null, a new DbFilterExpression has been created with our dynamic filters.
                    return base.Visit(newFilterExpression);
                }
            }

            return base.Visit(expression);
        }

        /// <summary>
        /// Helper method creating the correct filter expression based on the supplied parameters
        /// </summary>
        private DbFilterExpression BuildFilterExpression(DbExpressionBinding binding, DbExpression predicate, string column)
        {
            _injectedDynamicFilter = true;

            var variableReference = DbExpressionBuilder.Variable(binding.VariableType, binding.VariableName);
            // Create the property based on the variable in order to apply the equality
            var tenantProperty = DbExpressionBuilder.Property(variableReference, column);
            // Create the parameter which is an object representation of a sql parameter.
            // We have to create a parameter and not perform a direct comparison with Equal function for example
            // as this logic is cached per query and called only once
            var tenantParameter = DbExpressionBuilder.Parameter(tenantProperty.Property.TypeUsage,
                TenantAwareAttribute.TenantIdFilterParameterName);
            // Apply the equality between property and parameter.
            DbExpression newPredicate = DbExpressionBuilder.Equal(tenantProperty, tenantParameter);

            // If an existing predicate exists (normally when called from DbFilterExpression) execute a logical AND to get the result
            if (predicate != null)
                newPredicate = newPredicate.And(predicate);

            return DbExpressionBuilder.Filter(binding, newPredicate);
        }

    }
}
