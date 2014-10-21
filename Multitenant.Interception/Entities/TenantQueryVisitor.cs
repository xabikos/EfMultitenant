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
                var current = base.Visit(expression).Bind();
                var columnProperty = current.VariableType.Variable(current.VariableName).Property(column);
                var param = columnProperty.Property.TypeUsage.Parameter(TenantAwareAttribute.TenantIdFilterName);
                return current.Filter(columnProperty.Equal(param));
            }

            return base.Visit(expression);
        }
    
    }
}