using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Threading;
using System.Web;
using Multitenant.Interception.Models;

namespace Multitenant.Interception.Entities
{
    public class TenantQueryVisitor: DefaultExpressionVisitor
    {
        public override DbExpression Visit(DbScanExpression expression)
        {
            var column = TenantAttribute.GetTenantColumnName(expression.Target.ElementType);
            if (column != null)
            {
                var principal = Thread.CurrentPrincipal as ApplicationUser;
                var binding = DbExpressionBuilder.Bind(expression);
                return DbExpressionBuilder.Filter(
                    binding,
                    DbExpressionBuilder.Equal(
                        DbExpressionBuilder.Property(
                            DbExpressionBuilder.Variable(binding.VariableType, binding.VariableName),
                            column),
                        DbExpression.FromString("31fc8805-cfa9-43c0-8c8b-7f0c7ef4842f")));
            }
            else
            {
                return base.Visit(expression);
            }
        }
    
    }
}