using System.Threading.Tasks;

namespace Asyncify.Extensions
{
    static class TaskExtensions
    {
        public static Task<T> AsTask<T>(this T obj)
        {
            return Task.FromResult(obj);
        }
    }
}