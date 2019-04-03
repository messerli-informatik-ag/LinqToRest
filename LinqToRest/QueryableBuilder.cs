using System;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Messerli.QueryProvider;
using QueryProviderBase = Messerli.QueryProvider.QueryProvider;

namespace Messerli.LinqToRest
{
    public sealed class QueryableBuilder: IQueryableBuilder
    {
        private HttpClient _httpClient = new HttpClient();
        [CanBeNull] private Uri _uri;

        public IQueryableBuilder Root(Uri uri)
        {
            _uri = uri;
            return this;
        }
        
        public IQueryableBuilder HttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            return this;
        }
        
        public IQueryable<T> Build<T>()
        {
            ValidateConfiguration();
            
            var resourceRetriever = CreateResourceRetriever();
            var queryBinderFactory = CreateQueryBinderFactory();
            
            var queryProvider = new QueryProvider(resourceRetriever, queryBinderFactory, _uri);
            var queryableFactory = CreateQueryableFactory(resourceRetriever, queryBinderFactory);
            
            // Resolve circular dependency
            resourceRetriever.QueryableFactory = queryableFactory;

            return new Query<T>(queryProvider);
        }

        private void ValidateConfiguration()
        {
            if (_uri is null)
            {
                throw new QueryableBuilderException(
                    $"Root uri was not configured. Call .{nameof(Root)}(...) before .{nameof(Build)}().");
            }

            if (_httpClient is null)
            {
                throw new QueryableBuilderException(
                    $"HTTP client was configured as null. Call .{nameof(HttpClient)}(...) with a non-null value or leave it at its default value"); 
            }
        }

        private static QueryableFactory CreateQueryableFactory(IResourceRetriever resourceRetriever, QueryBinderFactory queryBinderFactory)
        {
            return (type, uri) =>
            {
                var subQueryProvider = new QueryProvider(resourceRetriever, queryBinderFactory, uri);
                return Activator.CreateInstance(typeof(Query<>).MakeGenericType(type), subQueryProvider) as
                        IQueryable<object>;
            };
        }

        private ResourceRetriever CreateResourceRetriever()
        {
            return new ResourceRetriever(_httpClient);
        }

        private static QueryBinderFactory CreateQueryBinderFactory()
        {
            var entityValidator = CreateEntityValidator();
            return () => new QueryBinder(entityValidator);
        }

        private static EntityValidator CreateEntityValidator()
        {
            return new EntityValidator();
        }
    }
}
