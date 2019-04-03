using System;
using System.Linq;
using System.Net.Http;
using Messerli.LinqToRest.Test.Stub;
using Messerli.QueryProvider;
using Xunit;
using QueryProviderBase = Messerli.QueryProvider.QueryProvider;

namespace Messerli.LinqToRest.Test
{
    public class QueryTest
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

        #region Helper

        private static IQueryable<T> CreateQuery<T>()
        {
            return new QueryableBuilder()
                .Root(RootUri)
                .HttpClient(MockHttpClient())
                .Build<T>();
        }

        private static IQueryable<T> CreateQuery<T>(string subPath)
        {
            return new QueryableBuilder()
                .Root(new Uri(RootUri, subPath))
                .HttpClient(MockHttpClient())
                .Build<T>();
        }

        #endregion

        #region Mock

        private static HttpClient MockHttpClient()
        {
            return new HttpClientMockBuilder()
                .JsonResponse(UniqueIdentifierNameRequestUri.ToString(), UniqueIdentifierNameJson)
                .JsonResponse(EntityWithQueryableMemberRequestUri.ToString(), EntityWithQueryableMemberJson)
                .Build();
        }

        #endregion

        #region Data

        private static Uri RootUri => new Uri("http://www.example.com/api/v1/", UriKind.Absolute);

        private static Uri EntityWithQueryableMemberRequestUri => new Uri(RootUri, "entitywithqueryablemembers");

        private static string EntityWithQueryableMemberJson => @"
[
    {
        ""uniqueIdentifier"": ""Test1"",
        ""name"": ""Test1""
    },
    {
        ""uniqueIdentifier"": ""Test2"",
        ""name"": ""Test2""
    }
]
";

        private static QueryResult<EntityWithQueryableMember> EntityWithQueryableMemberResult => new QueryResult<EntityWithQueryableMember>(
            EntityWithQueryableMemberRequestUri,
            new[]
            {
                new EntityWithQueryableMember("Test1", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test1/")),
                new EntityWithQueryableMember("Test2", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test2/")),
            });

        private static Uri UniqueIdentifierNameRequestUri =>
            new Uri(RootUri, "entitywithqueryablemembers?fields=uniqueIdentifier,name");

        private static string UniqueIdentifierNameJson => @"
[
    {
        ""uniqueIdentifier"": ""Test1"",
        ""name"": ""Test1""
    },
    {
        ""uniqueIdentifier"": ""Test2"",
        ""name"": ""Test2""
    }
]
";

        private static QueryResult<object> UniqueIdentifierNameResult => new QueryResult<object>(
            UniqueIdentifierNameRequestUri,
            new object[]
            {
                new
                {
                    UniqueIdentifier = "Test1",
                    Name = "Test1"
                },
                new
                {
                    UniqueIdentifier = "Test2",
                    Name = "Test2"
                }
            });

        private static QueryResult<object> NameResult => new QueryResult<object>(
            UniqueIdentifierNameRequestUri,
            new object[]
            {
                new
                {
                    Name = "Test1"
                },
                new
                {
                    Name = "Test2"
                }
            });

        #endregion
    }
}
