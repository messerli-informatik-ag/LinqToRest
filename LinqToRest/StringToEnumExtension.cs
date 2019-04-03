using System;
using System.ComponentModel;
using System.Reflection;
using Funcky.Extensions;

namespace Messerli.LinqToRest
{
    internal static class StringToEnumExtension
    {
        public static object ParseToEnumElement(this string candidate, Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{type.Name} is not an enum type!");
            }

            var parse = typeof(StringToEnumExtension).GetMethod(nameof(ParseToEnumElement), new Type[] {typeof(string) })?.MakeGenericMethod(type)
                           ?? throw new MissingMethodException();

            try
            {
                return parse.Invoke(null, new object[] { candidate });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }

                throw;
            }
        }

        private static T ParseToEnumElement<T>(this string candidate) where T : struct
        {
            var parsed = candidate.TryParseEnum<T>();

            return parsed.Match(none: false, some: @enum => true)
                ? parsed.Match(none: default(T), some: @enum => @enum)
                : throw new InvalidEnumArgumentException(candidate);
        }
    }
}