using System;
using System.Linq;
using System.Net.Http;
using Messerli.LinqToRest.Test.Stub;

namespace Messerli.LinqToRest.Test
{
    public static class QueryProviderTestUtility
    {
        #region Helper

        public static IQueryable<T> CreateQuery<T>()
        {
            return new QueryableBuilder()
                .Root(RootUri)
                .HttpClient(MockHttpClient())
                .Build<T>();
        }

        public static IQueryable<T> CreateQuery<T>(string subPath)
        {
            return new QueryableBuilder()
                .Root(new Uri(RootUri, subPath))
                .HttpClient(MockHttpClient())
                .Build<T>();
        }

        #endregion

        #region Mock

        public static HttpClient MockHttpClient()
        {
            return new HttpClientMockBuilder()
                .JsonResponse(UniqueIdentifierNameRequestUri.ToString(), UniqueIdentifierNameJson)
                .JsonResponse(EntityWithQueryableMemberRequestUri.ToString(), EntityWithQueryableMemberJson)
                .JsonResponse(EntityWithUriMemberRequestUri.ToString(), EntityWithUriMemberJson)
                .Build();
        }

        #endregion

        #region Data

        public static Uri RootUri => new Uri("http://www.example.com/api/v1/", UriKind.Absolute);

        public static Uri EntityWithQueryableMemberRequestUri => new Uri(RootUri, "entitywithqueryablemembers");

        public static string EntityWithQueryableMemberJson => @"
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

        public static QueryResult<EntityWithQueryableMember> EntityWithQueryableMemberResult => new QueryResult<EntityWithQueryableMember>(
            EntityWithQueryableMemberRequestUri,
            new[]
            {
                new EntityWithQueryableMember("Test1", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test1/")),
                new EntityWithQueryableMember("Test2", CreateQuery<EntityWithSimpleMembers>("entitywithqueryablemembers/Test2/")),
            });

        public static Uri EntityWithUriMemberRequestUri => new Uri(RootUri, "entitywithurimembers");

        public static string EntityWithUriMemberJson => @"
[
    {
        ""uniqueIdentifier"": ""Test1"",
        ""name"": ""Test1"",
        ""uri"": ""https://www.example.com/1""
    },
    {
        ""uniqueIdentifier"": ""Test2"",
        ""name"": ""Test2"",
        ""uri"": ""https://www.example.com/2""
    }
]
";

        public static QueryResult<EntityWithUriMember> EntityWithUriMemberResult => new QueryResult<EntityWithUriMember>(
            EntityWithUriMemberRequestUri,
            new[]
            {
                new EntityWithUriMember("Test1", new Uri("https://www.example.com/1")),
                new EntityWithUriMember("Test2", new Uri("https://www.example.com/2")),
            });

        public static Uri UniqueIdentifierNameRequestUri =>
            new Uri(RootUri, "entitywithqueryablemembers?fields=uniqueIdentifier,name");

        public static string UniqueIdentifierNameJson => @"
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

        public static QueryResult<object> UniqueIdentifierNameResult => new QueryResult<object>(
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

        public static QueryResult<object> RenamedSelectedFieldsResults => new(
            UniqueIdentifierNameRequestUri,
            new object[]
            {
                new
                {
                    Id = "Test1",
                    EntityName = "Test1",
                },
                new
                {
                    Id = "Test2",
                    EntityName = "Test2",
                }
            });

        public static QueryResult<object> NameResult => new QueryResult<object>(
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
