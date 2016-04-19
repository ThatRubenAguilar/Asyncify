using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asyncify.Test.Extensions;

namespace TestHelper
{
    public static class TestSourceCode
    {
        public const int TabSize = 4;
        public const int IndentSize = TabSize;
        public const int DefaultIndents = 2;


        static readonly string FullTriviaTextTemplate = @"// One Line Comment 
#if Directive
{0}/* Comment If */
#else
{1}/* Comment Else */
#endif
{2}/*
{2}Multi Line Comment 
{2}*/
{3}#region Region
{3}#endregion
";

        /// <summary>
        /// Creates trivia text which is indented ifDirectiveIndents amount for ifDirective and will be indented to regionIndents amount for regions. This is the format output from Formatter running over the trivia text. Typically ifDirective is aligned to the closest block indent, and region is aligned to the closest variable indent.
        /// </summary>
        /// <returns></returns>
        public static string TriviaTextFormatted(int regionIndents, int ifDirectiveIndents)
        {
            return TriviaTextCustom(ifDirectiveIndents, 0, 0, regionIndents);
        }
        /// <summary>
        /// Creates trivia text which is indented ifDirectiveIndents amount for ifDirective and will be indented to regionIndents amount for regions. This is the format output from Formatter running over the trivia text. This handles the common case of ifDirectiveIndents = regionIdents-1
        /// </summary>
        /// <returns></returns>
        public static string TriviaTextFormatted(int regionIndents = DefaultIndents)
        {
            var ifDirectiveIndents = regionIndents - 1;
            if (ifDirectiveIndents < 0)
                ifDirectiveIndents = 0;
            return TriviaTextCustom(ifDirectiveIndents , 0, 0, regionIndents);
        }
        /// <summary>
        /// Creates trivia text with a fully custom format
        /// </summary>
        /// <param name="ifDirectiveIndents"></param>
        /// <param name="elseDirectiveIndents"></param>
        /// <param name="multilineCommentIndents"></param>
        /// <param name="regionIndents"></param>
        /// <returns></returns>
        public static string TriviaTextCustom(int ifDirectiveIndents=0, int elseDirectiveIndents=0, int multilineCommentIndents=0, int regionIndents=0)
        {
            return String.Format(FullTriviaTextTemplate, CreateSpaces(ifDirectiveIndents*IndentSize), CreateSpaces(elseDirectiveIndents * IndentSize), CreateSpaces(multilineCommentIndents * IndentSize), CreateSpaces(regionIndents * IndentSize));
        }

        private static string CreateSpaces(int numSpaces)
        {
            var spacesBuilder = new StringBuilder();
            for (int i = 0; i < numSpaces; i++)
            {
                spacesBuilder.Append(" ");
            }
            return spacesBuilder.ToString();
        }

        /// <summary>
        /// Creates trivia text which is uniformly indented formattedIndents amount
        /// </summary>
        /// <param name="formattedIndents"></param>
        /// <returns></returns>
        public static string TriviaTextUniform(int formattedIndents = DefaultIndents)
        {
            return TriviaTextCustom(formattedIndents, formattedIndents, formattedIndents, formattedIndents);
        }


        public static readonly string FullTriviaText = TriviaTextCustom(DefaultIndents, 0, 0, DefaultIndents);

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

        public static TaskChild<Different.AsyncMemberMethods> GetDifferentMemberMethods()
        {
            return new TaskChild<Different.AsyncMemberMethods>();
        }
    }
";
        public static readonly string TaskNamespacedStaticClass = @"
    using System;
    using System.Threading.Tasks;

namespace Different
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

        public static readonly string[] SupportingSources =
        {
            TaskStaticClass, TaskMemberClass, TaskChildClass,
            TaskNamespacedStaticClass
        };

        public static string[] GetCompilationSources(string testSource)
        {
            return new[] {testSource}.Concat(SupportingSources).ToArray();
        }

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