using System;
using System.Threading.Tasks;

namespace Messerli.LinqToRest
{
    internal interface IResourceRetriever
    {
        Task<T> RetrieveResource<T>(Uri uri);
    }
}
