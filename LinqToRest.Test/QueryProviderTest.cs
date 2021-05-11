using System.Linq;
using Messerli.LinqToRest.Test.Stub;
using Xunit;
using static Messerli.LinqToRest.Test.QueryProviderTestUtility;

namespace Messerli.LinqToRest.Test
{
    public class QueryProviderTest
    {
        [Fact]
        public void ReturnsRestQuery()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .ToString();

            var expected = EntityWithQueryableMemberResult.Query;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestQueryWithSelect()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .Select(entity => new { entity.Name })
                .ToString();

            var expected = UniqueIdentifierNameResult.Query;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestQueryWithSelectedUniqueIdentifier()
        {
            var actual = CreateQuery<EntityWithQueryableMember>()
                .Select(entity => new { entity.UniqueIdentifier, entity.Name })
                .ToString();

            var expected = UniqueIdentifierNameResult.Query;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObject()
        {
            var actual = new QueryResult<EntityWithQueryableMember>(
                CreateQuery<EntityWithQueryableMember>());

            var expected = EntityWithQueryableMemberResult;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithNonCastableMember()
        {
            var actual = new QueryResult<EntityWithUriMember>(
                CreateQuery<EntityWithUriMember>());

            var expected = EntityWithUriMemberResult;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithSelect()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>()
                    .Select(entity => new { entity.Name }));

            var expected = NameResult;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithSelectedUniqueIdentifier()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>()
                    .Select(entity => new { entity.UniqueIdentifier, entity.Name }));

            var expected = UniqueIdentifierNameResult;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsRestObjectWithRenamedSelectedFields()
        {
            var actual = new QueryResult<object>(
                CreateQuery<EntityWithQueryableMember>()
                    .Select(entity => new { Id = entity.UniqueIdentifier, EntityName = entity.Name }));

            var expected = RenamedSelectedFieldsResults;

            Assert.Equal(expected, actual);
        }
    }
}
