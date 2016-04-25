using System;
using System.Collections.Generic;
using System.Linq;
using Asyncify.Test.Extensions;
using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class TaskExpressionWrapper : WrapperCodeUnit
    {
        public const string SourceCode = @"
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

        public TaskExpressionWrapper() : base(SourceCode)
        {
            
        }
    }
}