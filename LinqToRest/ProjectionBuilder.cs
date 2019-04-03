using Messerli.LinqToRest.Expressions;
using System;
using System.Linq.Expressions;

namespace Messerli.LinqToRest
{
    internal sealed class ProjectionBuilder : ExpressionVisitor
    {
        private ParameterExpression _resource;

        internal LambdaExpression Build(Expression expression)
        {
            _resource = Expression.Parameter(typeof(ResourceProjection), nameof(ResourceProjection));
            var body = Visit(expression) ?? throw new InvalidOperationException();

            return Expression.Lambda(body, _resource);
        }

        protected override Expression VisitField(FieldExpression field)
        {
            var valuePropertyMethodInfo = typeof(ResourceProjection)
                .GetProperty(nameof(ResourceProjection.Value))
                ?.GetGetMethod()
                ?? throw new InvalidOperationException();
            return Expression.Convert(Expression.Call(_resource, valuePropertyMethodInfo), field.Type);
        }
    }
}
