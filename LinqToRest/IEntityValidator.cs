using System;

namespace Messerli.LinqToRest
{
    internal interface IEntityValidator
    {
        void ValidateResourceEntity(Type type);
    }
}