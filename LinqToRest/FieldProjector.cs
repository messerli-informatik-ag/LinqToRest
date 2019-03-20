﻿using Messerli.LinqToRest.Declarations;
using Messerli.LinqToRest.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Messerli.LinqToRest
{
    internal class FieldProjector : ExpressionVisitor
    {
        private readonly Nominator _nominator;
        private List<FieldDeclaration> _fields;
        private HashSet<Expression> _candidates;

        internal FieldProjector(Predicate<Expression> canBeFieldPredicate)
        {
            _nominator = new Nominator(canBeFieldPredicate);
        }

        internal ProjectedFields ProjectFields(Expression expression)
        {
            _fields = new List<FieldDeclaration>();
            _candidates = _nominator.Nominate(expression);

            return new ProjectedFields(Visit(expression), _fields.AsReadOnly());
        }

        public override Expression Visit(Expression expression)
        {
            if (!_candidates.Contains(expression))
            {
                return base.Visit(expression);
            }

            switch (expression.NodeType)
            {
                case (System.Linq.Expressions.ExpressionType)ExpressionType.Field:
                    {
                        var field = (FieldExpression)expression;

                        _fields.Add(new FieldDeclaration(field.Name, field));

                        return field;
                    }
                case System.Linq.Expressions.ExpressionType.Constant:
                    {
                        var name = (ConstantExpression)expression;
                        _fields.Add(new FieldDeclaration(name.ToString(), expression));

                        return new FieldExpression(expression.Type, name.ToString(), new FieldDeclaration[0]);
                    }
                default:
                    throw new InvalidEnumArgumentException(expression.NodeType.ToString());
            }
        }

        private class Nominator : ExpressionVisitor
        {
            private readonly Predicate<Expression> _canBeFieldPredicate;
            private bool _isField = true;
            private HashSet<Expression> _candidates;

            internal Nominator(Predicate<Expression> canBeFieldPredicate)
            {
                _canBeFieldPredicate = canBeFieldPredicate;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                _candidates = new HashSet<Expression>();
                _isField = true;
                Visit(expression);

                return _candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is null)
                {
                    return null;
                }

                var saveIsField = _isField;
                _isField = true;

                base.Visit(expression);

                if (_isField && (_isField = _canBeFieldPredicate(expression)))
                {
                    _candidates.Add(expression);
                }

                _isField &= saveIsField;

                return expression;
            }
        }
    }
}