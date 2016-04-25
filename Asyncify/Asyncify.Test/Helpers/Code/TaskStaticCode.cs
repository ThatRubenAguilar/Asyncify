using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class TaskStaticCode : SourceCodeUnit
    {
        public const string SourceCode = @"
    using System;
    using System.Threading.Tasks;

    static class AsyncMethods
    {
        public static TaskChild<int> GetNumber()
        {
            return new TaskChild<int>();
        }

        public static TaskChild PerformProcessing()
        {
            return new TaskChild();
        }

        public static TaskChild<AsyncMemberMethods> GetMemberMethods()
        {
            return new TaskChild<AsyncMemberMethods>();
        }

        public static TaskChild<Different.AsyncMemberMethods> GetDifferentMemberMethods()
        {
            return new TaskChild<Different.AsyncMemberMethods>();
        }

        public static TaskChild<Different.Namespace.AsyncMemberMethods> GetReallyDifferentMemberMethods()
        {
            return new TaskChild<Different.Namespace.AsyncMemberMethods>();
        }
    }
";

        public TaskStaticCode() : base(SourceCode)
        {
            
        }
    }
}