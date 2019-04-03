using System;

namespace Messerli.LinqToRest
{
    public sealed class MalformedResourceEntityException : Exception
    {
        public MalformedResourceEntityException()
        {
        }

        public MalformedResourceEntityException(string message)
            : base(message)
        {
        }

        public MalformedResourceEntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
