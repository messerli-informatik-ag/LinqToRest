using JetBrains.Annotations;
using Messerli.LinqToRest.Entities;
using Messerli.Utility.Extension;
using Soltys.ChangeCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Messerli.LinqToRest
{
    [UsedImplicitly]
    internal sealed class EntityValidator : IEntityValidator
    {
        public void ValidateResourceEntity(Type type)
        {
            ValidateImplementations(type);

            ValidateMethods(type);

            ValidateFields(type);

            var properties = ValidateProperties(type);

            var constructor = ValidateConstructors(type);

            var parameters = ValidateParameters(type, constructor, properties);

            ValidateThatPropertiesAndParateresMatch(type, properties, parameters);
        }

        private static void ValidateImplementations(Type type)
        {
            var interfaces = type.GetInterfaces();

            if (!interfaces.Contains(typeof(IEntity)))
            {
                throw new MalformedResourceEntityException
                (
                    $"Type {type.Name} has to implement the interface {typeof(IEntity)}."
                );
            }
        }

        private static void ValidateThatPropertiesAndParateresMatch(MemberInfo type, IEnumerable<PropertyInfo> properties, IEnumerable<ParameterInfo> parameters)
        {
            parameters
                .Zip(properties, (parameter, property) => new { parameter, property })
                .ForEach(tuple =>
                {
                    var propertyName = tuple.property.Name;
                    var expectedName = ChangeCaseExtensions.CamelCase(propertyName);
                    var expectedType = tuple.property.PropertyType;
                    var actualName = tuple.parameter.Name;
                    var actualType = tuple.parameter.ParameterType;
                    if (!(actualType == expectedType && actualName == expectedName))
                    {
                        throw new MalformedResourceEntityException
                        (
                            $@"Parameter in constructor of type {type.Name} didn't match the its property.
Expected: {expectedType} {expectedName}
Which would match property: {expectedType} {propertyName} {{ get; }}
Actually got: {actualType} {expectedName}"
                        );
                    }
                });
        }

        private static IEnumerable<ParameterInfo> ValidateParameters(MemberInfo type, MethodBase constructorInfo, PropertyInfo[] properties)
        {
            var parameters = constructorInfo.GetParameters();
            var implementedProperties = typeof(IEntity).GetProperties().Length;
            if (parameters.Length != properties.Length - implementedProperties)
            {
                throw new MalformedResourceEntityException
                (
                    $@"Type {type.Name} has a different amount of parameters than properties.
Parameters: {parameters}
Properties: {properties}"
                );
            }

            return parameters;
        }

        private static PropertyInfo[] ValidateProperties(Type type)
        {
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (!(property.Attributes == PropertyAttributes.None
                      && property.CanRead &&
                      !property.CanWrite))
                {
                    throw new MalformedResourceEntityException
                    (
                        $@"The property {property.PropertyType} {property.Name} is not immutable.
Help: Change its definition to the following and initialize it from the constructor:
public {property.PropertyType} {property.Name} {{ get; }}"
                    );
                }
            }

            return properties;
        }

        private static void ValidateMethods(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                const string getPropertyMethodPrefix = "get_";
                if (!method.IsSpecialName || !method.Name.StartsWith(getPropertyMethodPrefix))
                {
                    continue;
                }

                var property = type.GetProperty(method.Name.Substring(getPropertyMethodPrefix.Length),
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                var signature = $"{method.ReturnType} {method.Name} (...)";
                if (property is null)
                {
                    throw new MalformedResourceEntityException
                    (
                        $"Method {signature} of type {type.Name} " +
                        $@"is marked as the generated method of a get property, but is not backed by any property
Help: Remove the following method
{signature}"
                    );
                }
                var accessor = property.GetGetMethod();
                if (method != accessor)
                {
                    throw new MalformedResourceEntityException
                    (
                        $@"Type {type.Name} is not allowed to have any methods.
Help: Remove the following method
{signature}"
                    );
                }

            }
        }

        private static ConstructorInfo ValidateConstructors(Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length > 1)
            {
                throw new MalformedResourceEntityException
                (
                    $"Constructor of type {type.Name} is not allowed to have more than one constructor"
                );
            }

            return constructors.First();
        }

        private static void ValidateFields(Type type)
        {
            if (type.GetFields().Any())
            {
                throw new MalformedResourceEntityException
                (
                    $"Type {type.Name} is not allowed to have any fields."
                );
            }
        }

    }
}