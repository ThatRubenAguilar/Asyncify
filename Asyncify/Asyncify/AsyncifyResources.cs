using System.Text.RegularExpressions;

namespace Asyncify
{
    internal class AsyncifyResources
    {
        public static readonly Regex TaskRegex = new Regex("System.Threading.Tasks.Task");
        public static readonly Regex TaskGenericRegex = new Regex("System.Threading.Tasks.Task<");
        public static readonly Regex TaskAwaiterRegex = new Regex("System.Runtime.CompilerServices.TaskAwaiter");

        public const string ResultProperty = "Result";
        public const string GetResultMethod = "GetResult";
        public const string GetAwaiterMethod = "GetAwaiter";

        public const string KeyUseAwaitResult = "UseAwaitResult";
        public const string KeyUseAwaitGetResult = "UseAwaitGetResult";
    }
}