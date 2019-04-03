using Messerli.LinqToRest.Declarations;
using Messerli.LinqToRest.Expressions;
using Messerli.Utility.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Messerli.LinqToRest
{
    internal class ExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        public override Expression Visit(Expression exp)
        {
            if (exp is null)
            {
                return null;
            }

            switch ((ExpressionType)exp.NodeType)
            {
                case ExpressionType.Resource:
                    return VisitResource((ResourceExpression)exp);
                case ExpressionType.Field:
                    return VisitField((FieldExpression)exp);
                case ExpressionType.Projection:
                    return VisitProjection((ProjectionExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        protected virtual Expression VisitResource(ResourceExpression e)
        {
            var fields = VisitFieldDeclarations(e.Fields);
            var filter = Visit(e.Filter);

            return Equals(fields, e.Fields) && filter == e.Filter
                ? e
                : new ResourceExpression(e.Type, e.Name, fields, filter);
        }

        protected virtual Expression VisitField(FieldExpression field)
        {
            var properties = VisitFieldDeclarations(field.Properties);

            return properties.Equals(field.Properties)
                ? field
                : new FieldExpression(field.Type, field.Name, properties);
        }

        protected virtual Expression VisitProjection(ProjectionExpression projection)
        {
            var projector = Visit(projection.Projector);
            var source = Visit(projection.Source);

            return projector == projection.Projector && source == projection.Source
                ? projection
                : new ProjectionExpression(source, projector);
        }

        private IReadOnlyCollection<FieldDeclaration> VisitFieldDeclarations(
            IReadOnlyCollection<FieldDeclaration> fields)
        {
            // Todo: where is alternate used?!?
            List<FieldDeclaration> alternate = null;
            foreach (var (field, index) in fields.WithIndex())
            {
                var e = Visit(field.Expression);
                if (alternate is null && e != field.Expression)
                {
                    alternate = fields.Take(index).ToList();
                }

                alternate?.Add(new FieldDeclaration(field.Name, e));
            }

            return alternate?.AsReadOnly() ?? fields;
        }
    }
}