using Messerli.LinqToRest.Declarations;
using Messerli.LinqToRest.Entities;
using Messerli.LinqToRest.Expressions;
using Soltys.ChangeCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Messerli.LinqToRest
{
    internal sealed class QueryBinder : ExpressionVisitor
    {
        private readonly FieldProjector _fieldProjector;
        private readonly IEntityValidator _entityValidator;
        private Dictionary<ParameterExpression, Expression> _parametersToProjectors;

        public QueryBinder(IEntityValidator entityValidator)
        {
            _entityValidator = entityValidator;
            _fieldProjector = new FieldProjector(CanBeField);
        }

        private static bool CanBeField(Expression expression)
        {
            return expression.NodeType == (System.Linq.Expressions.ExpressionType)ExpressionType.Field;
        }

        internal Expression Bind(Expression expression)
        {
            _parametersToProjectors = new Dictionary<ParameterExpression, Expression>();
            return Visit(expression);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == System.Linq.Expressions.ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private ProjectedFields ProjectProperties(Expression expression)
        {
            return _fieldProjector.ProjectFields(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression method)
        {
            if (method.Method.DeclaringType != typeof(Queryable) && method.Method.DeclaringType != typeof(Enumerable))
                return base.VisitMethodCall(method);

            switch (method.Method.Name)
            {
                case nameof(Enumerable.Where):
                    return BindWhere(method.Type, method.Arguments.First(), (LambdaExpression)StripQuotes(method.Arguments[1]));

                case nameof(Enumerable.Select):
                    return BindSelect(method.Type, method.Arguments.First(), (LambdaExpression)StripQuotes(method.Arguments[1]));

                default:
                    throw new NotSupportedException($"The method '{method.Method.Name}' is not supported");
            }
        }

        private Expression BindWhere(Type resultType, Expression sourceExpression, LambdaExpression predicate)
        {
            var projection = (ProjectionExpression)Visit(sourceExpression)
                             ?? throw new InvalidOperationException(nameof(ExpressionVisitor.Visit));
            _parametersToProjectors[predicate.Parameters.First()] = projection.Projector;

            var filter = Visit(predicate.Body);
            var projectedProperties = ProjectProperties(projection.Projector);

            return new ProjectionExpression(
                new ResourceExpression(resultType, sourceExpression, projectedProperties.Fields, filter),
                projection.Projector);
        }

        private Expression BindSelect(Type resultType,
            Expression sourceExpression, LambdaExpression selector)
        {
            var projection = (ProjectionExpression)Visit(sourceExpression) ?? throw new InvalidOperationException(nameof(ExpressionVisitor.Visit));
            _parametersToProjectors[selector.Parameters.First()] = projection.Projector;

            var expression = Visit(selector.Body);
            var projectedProperties = ProjectProperties(expression);

            return new ProjectionExpression(
                new ResourceExpression(resultType, sourceExpression, projectedProperties.Fields, null),
                projection.Projector);
        }

        private static bool IsResource(object value)
        {
            return value is IQueryable queryable
                && queryable.Expression.NodeType == System.Linq.Expressions.ExpressionType.Constant;
        }

        private ProjectionExpression GetResourceProjection(IQueryable resource)
        {
            var constructorInfo = resource.ElementType.GetConstructors().First();

            _entityValidator.ValidateResourceEntity(resource.ElementType);

            var parameterExpressions = constructorInfo
                .GetParameters()
                .Select(
                    parameter => new FieldExpression(parameter.ParameterType, parameter.Name, new FieldDeclaration[0])
                        as Expression
                )
                .ToArray();

            var projector = Expression.New(constructorInfo, parameterExpressions);

            var resultType = typeof(IEnumerable<>).MakeGenericType(resource.ElementType);
            var name = Expression.Constant(resource.ElementType);

            return new ProjectionExpression(
                new ResourceExpression(resultType, name, new FieldDeclaration[0], null),
                projector);
        }


        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (IsResource(constant.Value))
            {
                return GetResourceProjection((IQueryable)constant.Value);
            }

            return constant;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            return _parametersToProjectors.TryGetValue(parameter, out var mapped) ? mapped : parameter;
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            var source = Visit(member.Expression) ?? throw new InvalidOperationException(nameof(ExpressionVisitor.Visit));

            switch (source.NodeType)
            {
                case System.Linq.Expressions.ExpressionType.New:
                    var newExpression = (NewExpression)source;

                    var matchingArgument = newExpression
                        .Constructor
                        .GetParameters()
                        .Select(p => p.Name.PascalCase())
                        .Zip(newExpression.Arguments, (argument, parameter) => new { argument, parameter })
                        .FirstOrDefault(type => type.argument == member.Member.Name)?
                        .parameter;

                    if (matchingArgument != null)
                    {
                        return matchingArgument;
                    }

                    if (member.Member.Name == nameof(IEntity.UniqueIdentifier))
                    {
                        return new FieldExpression(typeof(string), nameof(IEntity.UniqueIdentifier).CamelCase(), new FieldDeclaration[0]);
                    }

                    break;
            }

            return source == member.Expression ? member : MakeMemberAccess(source, member.Member);
        }

        private static Expression MakeMemberAccess(Expression source,
            MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                return Expression.Field(source, fieldInfo);
            }

            var propertyInfo = (PropertyInfo)memberInfo;

            return Expression.Property(source, propertyInfo);
        }
    }
}