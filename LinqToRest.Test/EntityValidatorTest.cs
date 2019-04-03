using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Messerli.LinqToRest.Entities;
using Xunit;

namespace Messerli.LinqToRest.Test
{
    public class EntityValidatorTest
    {
        private IQueryableBuilder _queryableBuilder =
            new QueryableBuilder()
                    .Root(new Uri("https://www.example.com"))
                    .HttpClient(new HttpClientMockBuilder().Build());

        [Fact]
        public void ValidatesValidEntity()
        {
            _queryableBuilder.Build<ValidEntity>();
        }

        private class ValidEntity : IEntity
        {
            public ValidEntity(string foo)
            {
                Foo = foo;
            }

            [UsedImplicitly]
            public string Foo { get; }

            public string UniqueIdentifier => Foo;
        }


        [Fact]
        public void ValidatesValidEntityWithManyProperties()
        {
            _queryableBuilder.Build<ValidEntityWithManyProperties>();
        }

        private class ValidEntityWithManyProperties : IEntity
        {
            public ValidEntityWithManyProperties(string foo, int bar, IReadOnlyCollection<string> baz)
            {
                Foo = foo;
                Bar = bar;
                Baz = baz;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }


            [UsedImplicitly]
            public IReadOnlyCollection<string> Baz { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ValidatesEmptyEntity()
        {
            _queryableBuilder.Build<EmptyEntity>();
        }

        private class EmptyEntity : IEntity
        {
            public string UniqueIdentifier => "";
        }


        [Fact]
        public void ValidatesEntityWithOnlyMethods()
        {
            _queryableBuilder.Build<EntityWithOnlyMethods>();
        }

        private class EntityWithOnlyMethods : IEntity
        {
            [UsedImplicitly]
            public void Foo()
            {
            }

            public string UniqueIdentifier => "";
        }

        [Fact]
        public void ValidatesEntityWithOnlyStaticMethods()
        {
            _queryableBuilder.Build<EntityWithOnlyStaticMethods>();
        }

        private class EntityWithOnlyStaticMethods : IEntity
        {
            [UsedImplicitly]
            public static void Foo()
            {
            }

            public string UniqueIdentifier => "";
        }

        [Fact]
        public void ValidatesEntityWithPropertiesAndMethods()
        {
            _queryableBuilder.Build<EntityWithPropertiesAndMethods>();
        }

        private class EntityWithPropertiesAndMethods : IEntity
        {
            public EntityWithPropertiesAndMethods(string foo, int bar, IReadOnlyCollection<string> baz)
            {
                Foo = foo;
                Bar = bar;
                Baz = baz;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }


            [UsedImplicitly]
            public IReadOnlyCollection<string> Baz { get; }

            [UsedImplicitly]
            public void DoFoo()
            {
            }

            [UsedImplicitly]
            public static void DoBar()
            {
            }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithOnlyFields()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithOnlyFields>()
            );
        }

        private class EntityWithOnlyFields : IEntity
        {
            // Disable warning "Foo is never assigned"
#pragma warning disable 0649
            [UsedImplicitly]
            public string Foo;
#pragma warning restore 0649

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithFieldsInConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithFieldsInConstructor>()
            );
        }

        private class EntityWithFieldsInConstructor : IEntity
        {
            public EntityWithFieldsInConstructor(string foo)
            {
                Foo = foo;
            }

            [UsedImplicitly]
            public string Foo;

            public string UniqueIdentifier => Foo;
        }


        [Fact]
        public void ThrowsOnEntityWithFieldsAndPropertiesInConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithFieldsAndPropertiesInConstructor>()
            );
        }

        private class EntityWithFieldsAndPropertiesInConstructor : IEntity
        {
            public EntityWithFieldsAndPropertiesInConstructor(string foo, int baz)
            {
                Foo = foo;
                Baz = baz;
            }

            [UsedImplicitly]
            public string Foo;

            [UsedImplicitly]
            public int Baz { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithSetterProperties()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithSetterProperties>()
            );
        }

        private class EntityWithSetterProperties : IEntity
        {
            public EntityWithSetterProperties(string foo)
            {
                Foo = foo;
            }

            [UsedImplicitly]
            public string Foo { get; set; }

            public string UniqueIdentifier => Foo;
        }


        [Fact]
        public void ThrowsOnEntityWithPrivateSetterProperties()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithPrivateSetterProperties>()
            );
        }

        private class EntityWithPrivateSetterProperties : IEntity
        {
            public EntityWithPrivateSetterProperties(string foo)
            {
                Foo = foo;
            }

            [UsedImplicitly]
            public string Foo { get; private set; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithIncompleteConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithIncompleteConstructor>()
            );
        }

        private class EntityWithIncompleteConstructor : IEntity
        {
            public EntityWithIncompleteConstructor(string foo)
            {
                Foo = foo;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithTooLargeConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithTooLargeConstructor>()
            );
        }

        private class EntityWithTooLargeConstructor : IEntity
        {
            public EntityWithTooLargeConstructor(string foo, int bar, string baz)
            {
                Foo = foo + baz;
                Bar = bar;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithUnexpectedNamesInConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithUnexpectedNamesInConstructor>()
            );
        }

        private class EntityWithUnexpectedNamesInConstructor : IEntity
        {
            public EntityWithUnexpectedNamesInConstructor(string foo, int baz)
            {
                Foo = foo;
                Bar = baz;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithUnexpectedTypesInConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithUnexpectedTypesInConstructor>()
            );
        }

        private class EntityWithUnexpectedTypesInConstructor : IEntity
        {
            public EntityWithUnexpectedTypesInConstructor(string foo, uint bar)
            {
                Foo = foo;
                Bar = (int)bar;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithUnexpectedOrderInConstructor()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithUnexpectedOrderInConstructor>()
            );
        }

        private class EntityWithUnexpectedOrderInConstructor : IEntity
        {
            public EntityWithUnexpectedOrderInConstructor(int bar, string foo)
            {
                Foo = foo;
                Bar = bar;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityWithMultipleConstructors()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityWithMultipleConstructors>()
            );
        }

        private class EntityWithMultipleConstructors : IEntity
        {
            public EntityWithMultipleConstructors(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }

            public EntityWithMultipleConstructors()
            {
                Foo = "Foo";
                Bar = 1;
            }

            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            public string UniqueIdentifier => Foo;
        }

        [Fact]
        public void ThrowsOnEntityThatDoesNotImplementIEntity()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityThatDoesNotImplementIEntity>()
            );
        }

        private class EntityThatDoesNotImplementIEntity
        {
            public EntityThatDoesNotImplementIEntity(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }


            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }
        }

        [Fact]
        public void ThrowsOnEntityThatDoesNotImplementIEntityButSharesInterface()
        {
            Assert.Throws<MalformedResourceEntityException>
            (
                () => _queryableBuilder.Build<EntityThatDoesNotImplementIEntityButSharesInterface>()
            );
        }

        private class EntityThatDoesNotImplementIEntityButSharesInterface
        {
            public EntityThatDoesNotImplementIEntityButSharesInterface(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }


            [UsedImplicitly]
            public string Foo { get; }

            [UsedImplicitly]
            public int Bar { get; }

            [UsedImplicitly]
            public string UniqueIdentifier => Foo;
        }
    }
}
