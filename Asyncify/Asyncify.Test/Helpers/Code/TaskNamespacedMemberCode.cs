namespace Asyncify.Test.Helpers.Code
{
    public class TaskNamespacedMemberCode : SourceCodeUnit
    {
        public const string SourceCode = @"
    using System;
    using System.Threading.Tasks;

namespace Different.Namespace 
{
    class AsyncMemberMethods
    {
        public AsyncMemberMethods Field1 = null;
        public AsyncMemberMethods Property1 => null;
        public Task Result = null;

        public TaskAwaiter<AsyncMemberMethods> GetAwaiter() 
        {
            return null;
        }
        
        public AsyncMemberMethods GetResult() 
        {
            return null;
        }

        public void Wait() 
        {

        }

        public TaskChild<int> GetNumber()
        {
            return new TaskChild<int>();
        }

        public TaskChild PerformProcessing()
        {
            return new TaskChild();
        }
        
        public TaskChild<AsyncMemberMethods> GetMemberMethods()
        {
            return new TaskChild<AsyncMemberMethods>();
        }
        
    }
}
";

        public TaskNamespacedMemberCode() : base(SourceCode)
        {
            
        }
    }
}