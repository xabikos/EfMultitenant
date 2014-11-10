using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Multitenant.Interception.Entities
{
    /// <summary>
    /// Visitor pattern implementation class that adds filtering for tenantId column if applicable
    /// </summary>
    public class TenantQueryVisitor: DefaultExpressionVisitor
    {
        public override DbExpression Visit(DbScanExpression expression)
        {
            var column = TenantAwareAttribute.GetTenantColumnName(expression.Target.ElementType);
            if (!string.IsNullOrEmpty(column))
            {
                // Get the current expression
                var dbExpression = base.Visit(expression);
                // Get the current expression binding 
                var currentExpressionBinding = DbExpressionBuilder.Bind(dbExpression);
                // Create the variable reference in order to create the property
                var variableReference = DbExpressionBuilder.Variable(currentExpressionBinding.VariableType,
                    currentExpressionBinding.VariableName);
                // Create the property based on the variable in order to apply the equality
                var tenantProperty = DbExpressionBuilder.Property(variableReference, column);
                // Create the parameter which is an object representation of a sql parameter.
                // We have to create a parameter and not perform a direct comparison with Equal function for example
                // as this logic is cached per query and called only once
                var tenantParameter = DbExpressionBuilder.Parameter(tenantProperty.Property.TypeUsage,
                    TenantAwareAttribute.TenantIdFilterParameterName);
                // Apply the equality between property and parameter.
                var filterExpression = DbExpressionBuilder.Equal(tenantProperty, tenantParameter);
                // Apply the filtering to the initial query
                return DbExpressionBuilder.Filter(currentExpressionBinding, filterExpression);
            }

            return base.Visit(expression);
        }
    
    }
}