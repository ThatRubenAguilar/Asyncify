using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asyncify.Test.Extensions;

namespace TestHelper
{
    public static class TestSourceCode
    {
        public const int DefaultIndentSpaces = 8;
        public const int DefaultCorrectedSpaces = 0;


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
        /// Creates trivia text which is indented existingSpaces amount and will be indented to correctedSpaces amount in case of running formatter.
        /// </summary>
        /// <param name="existingSpaces"></param>
        /// <param name="correctedSpaces"></param>
        /// <returns></returns>
        public static string TriviaTextCorrected(int existingSpaces = DefaultIndentSpaces, int correctedSpaces = DefaultCorrectedSpaces)
        {
            return TriviaTextCustom(existingSpaces, correctedSpaces, correctedSpaces, existingSpaces);
        }

        public static string TriviaTextCustom(int ifDirectiveSpaces=0, int elseDirectiveSpaces=0, int multilineCommentSpaces=0, int regionSpaces=0)
        {
            return String.Format(FullTriviaTextTemplate, CreateSpaces(ifDirectiveSpaces), CreateSpaces(elseDirectiveSpaces), CreateSpaces(multilineCommentSpaces), CreateSpaces(regionSpaces));
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
        /// Creates trivia text which is indented existingSpaces amount
        /// </summary>
        /// <param name="existingSpaces"></param>
        /// <returns></returns>
        public static string TriviaText(int existingSpaces = DefaultIndentSpaces)
        {
            return TriviaTextCustom(existingSpaces, existingSpaces, existingSpaces, existingSpaces);
        }

        // NOTE: Indent code using this to 2 tabs (4 spaces each) or face the wrath of the whitespace formatter
        public static readonly string FullTriviaText = TriviaText();

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