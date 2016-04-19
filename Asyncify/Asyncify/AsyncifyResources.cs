using System.Text.RegularExpressions;

namespace Asyncify
{
    internal static class AsyncifyResources
    {
        public static readonly Regex TaskRegex = new Regex(TaskFullName);
        public static readonly Regex TaskGenericRegex = new Regex($"{TaskFullName}<");
        public static readonly Regex TaskAwaiterRegex = new Regex("System.Runtime.CompilerServices.TaskAwaiter");

        public const string TaskFullName = "System.Threading.Tasks.Task";
        public const string TaskNamespace = "System.Threading.Tasks";

        public const string ResultProperty = "Result";
        public const string GetResultMethod = "GetResult";
        public const string GetAwaiterMethod = "GetAwaiter";
        public const string WaitMethod = "Wait";

        public const string KeyUseAwaitResult = "UseAwaitResult";
        public const string KeyUseAwaitGetResult = "UseAwaitGetResult";
        public const string KeyUseAwaitWait = "UseAwaitWait";
        public const string KeyRemoveWait = "RemoveWait";
    }
}