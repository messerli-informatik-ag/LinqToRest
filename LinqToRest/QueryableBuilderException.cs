using System;

namespace Messerli.LinqToRest
{
    public sealed class QueryableBuilderException : Exception
    {
        public QueryableBuilderException()
        {
        }

        public QueryableBuilderException(string message)
            : base(message)
        {
        }

        public QueryableBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
