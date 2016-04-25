using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class TaskChildCode : SourceCodeUnit
    {
        public const string SourceCode = @"
    using System;
    using System.Threading.Tasks;

    class TaskChild : Task
    {
        public TaskChild() : base(() => { })
        {
        
        }
    }

    class TaskChild<T> : Task<T>
    {
        public TaskChild() : base(() => default(T))
        {
        
        }
    
    }
";

        public TaskChildCode() : base(SourceCode)
        {
            
        }
    }
}