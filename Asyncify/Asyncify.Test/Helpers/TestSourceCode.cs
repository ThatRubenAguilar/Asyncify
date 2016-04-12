using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Test.Extensions;

namespace TestHelper
{
    public static class TestSourceCode
    {

        // NOTE: Indent code using this to 2 tabs or face the wrath of the whitespace formatter
        public static readonly string FullTriviaText = @"// One Line Comment 
#if Directive
        /* Comment If */
#else
        /* Comment Else */
#endif
        /*
        Multi Line Comment 
        */
        #region Region
        #endregion
";
        public static readonly string TaskChildClass = @"
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
        public static readonly string TaskStaticClass = @"
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
    }
";
        public static readonly string TaskMemberClass = @"
    using System;
    using System.Threading.Tasks;

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
";

        public static readonly string TaskExpressionWrapper = @"
using System;
using System.Threading.Tasks;

class Test
{{
    async Task TestMethod()
    {{
{0}
    }}
}}
";

        public static readonly ResultLocation TaskExpressionWrapperLocation =
            TaskExpressionWrapper.CreateWrapperLocation(Guid.NewGuid());

        static ResultLocation CreateWrapperLocation(this string wrapperExpression, object markerObject)
        {
            return CreateWrapperLocations(wrapperExpression, markerObject).First();
        }
        static IEnumerable<ResultLocation> CreateWrapperLocations(this string wrapperExpression, params object[] markerObjects)
        {
            foreach (var markerObject in markerObjects)
            {
                var findMarker = markerObject.ToString();
                var filledWrapper = String.Format(wrapperExpression, markerObjects);
                var initialResult = filledWrapper.FindSourceLocation(findMarker);
                yield return new ResultLocation(initialResult.Line, initialResult.Column, initialResult.Span.Start, 0);
            }
            yield break;
        }
    }
}