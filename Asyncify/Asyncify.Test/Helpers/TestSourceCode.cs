using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asyncify.Test.Extensions;
using Asyncify.Test.Helpers.Code;

namespace TestHelper
{
    public static class TestSourceCode
    {
        public const int TabSize = 4;
        public const int IndentSize = TabSize;
        public const int DefaultIndents = 2;
        
        public static readonly string FullTriviaText = FullTriviaCode.TriviaTextCustom(DefaultIndents, 0, 0, DefaultIndents).ToString();

        public static ProjectUnit TaskWrapperProject = new ProjectUnit(
            new SourceCodeUnit[]
            {
                new TaskStaticCode(), new TaskMemberCode(), new TaskChildCode(), new TaskNamespacedStaticCode(), 
            }); 

        public static readonly string[] SupportingSources =
        {
            TaskStaticClass, TaskMemberClass, TaskChildClass,
            TaskNamespacedStaticClass
        };

        public static string[] GetCompilationSources(string testSource)
        {
            return new[] {testSource}.Concat(SupportingSources).ToArray();
        }
        // TODO: Make a compilation unit into a class with easy substitution access
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