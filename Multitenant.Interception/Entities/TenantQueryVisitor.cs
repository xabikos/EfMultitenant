using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Multitenant.Interception.Entities
{
    public class TenantQueryVisitor: DefaultExpressionVisitor
    {
        public override DbExpression Visit(DbScanExpression expression)
        {
            var column = TenantAttribute.GetTenantColumnName(expression.Target.ElementType);
            if (column != null)
            {
                var identity = (ClaimsIdentity)Thread.CurrentPrincipal.Identity;
                var userId = identity.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

                var binding = DbExpressionBuilder.Bind(expression);
                return DbExpressionBuilder.Filter(
                    binding,
                    DbExpressionBuilder.Equal(
                        DbExpressionBuilder.Property(
                            DbExpressionBuilder.Variable(binding.VariableType, binding.VariableName),
                            column),
                        DbExpression.FromString(userId)));
            }
            else
            {
                return base.Visit(expression);
            }
        }
    
    }
}