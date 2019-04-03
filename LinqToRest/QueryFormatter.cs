using Messerli.LinqToRest.Declarations;
using Messerli.LinqToRest.Expressions;
using Messerli.Utility.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Messerli.LinqToRest
{
    internal sealed class QueryFormatter : ExpressionVisitor
    {
        private StringBuilder _stringBuilder;
        private readonly Uri _root;
        private bool _hasParameters = false;

        public QueryFormatter(Uri root)
        {
            _root = root;
        }

        internal string Format(Expression expression)
        {
            _stringBuilder = new StringBuilder();
            Visit(expression);

            return _stringBuilder.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression method)
        {
            throw new NotSupportedException($"The method '{method.Method.Name}' is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            throw new NotSupportedException($"The unary operator '{unary.NodeType}' is not supported");
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            AppendParameterSeparator();
            Visit(binary.Left);

            var expressionToOperator = new Dictionary<System.Linq.Expressions.ExpressionType, string>()
            {
                {  System.Linq.Expressions.ExpressionType.And, "&" },
                {  System.Linq.Expressions.ExpressionType.Equal, "=" },
            };

            if (!expressionToOperator.TryGetValue(binary.NodeType, out var @operator))
            {
                throw new NotSupportedException($"The binary operator '{binary.NodeType}' is not supported");
            }

            _stringBuilder.Append(@operator);
            Visit(binary.Right);

            return binary;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            switch (constant.Value)
            {
                case IQueryable q:
                    _stringBuilder.Append(FormatRoute(q.ElementType.Name));
                    break;

                default:
                    switch (Type.GetTypeCode(constant.Value.GetType()))
                    {
                        case TypeCode.String:
                            _stringBuilder.Append(constant.Value);
                            break;

                        case TypeCode.Object:
                            _stringBuilder.Append(FormatRoute(constant.Value.ToString()));
                            break;

                        default:
                            _stringBuilder.Append(constant.Value);
                            break;
                    }
                    break;
            }
            return constant;
        }

        protected override Expression VisitResource(ResourceExpression resource)
        {
            _stringBuilder.Append(_root);
            Visit(resource.Name);

            if (resource.Fields.Any())
            {
                AppendParameterSeparator();
                _stringBuilder.Append("fields=");
                VisitFieldDeclarations(resource.Fields);
            }

            Visit(resource.Filter);

            return resource;
        }

        protected override Expression VisitField(FieldExpression field)
        {
            _stringBuilder.Append(field.Name);

            if (!field.Properties.Any())
            {
                return field;
            }

            _stringBuilder.Append("(");

            foreach (var (property, index) in field.Properties.WithIndex())
            {
                if (index > 0)
                {
                    _stringBuilder.Append(",");
                }

                _stringBuilder.Append(property.Name);
                Visit(property.Expression);
            }

            _stringBuilder.Append(")");

            return field;
        }

        private void VisitFieldDeclarations(IEnumerable<FieldDeclaration> fields)
        {
            foreach (var (field, index) in fields.WithIndex())
            {
                if (index > 0)
                {
                    _stringBuilder.Append(",");
                }

                VisitFieldDeclaration(field);
            }
        }

        private void VisitFieldDeclaration(FieldDeclaration field)
        {
            Visit(field.Expression);
        }

        private static string FormatRoute(string route)
        {
            // Todo: search NuGet for Pluralisation
            return route.Split('.').Last().ToLower() + "s";
        }

        private void AppendParameterSeparator()
        {
            var separator = GetParameterSeparator();
            _stringBuilder.Append(separator);
        }

        private string GetParameterSeparator()
        {
            var separator = _hasParameters ? "&" : "?";
            _hasParameters = true;
            return separator;
        }
    }
}
