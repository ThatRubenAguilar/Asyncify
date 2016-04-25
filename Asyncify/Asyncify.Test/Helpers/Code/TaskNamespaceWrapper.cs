namespace Asyncify.Test.Helpers.Code
{
    public class TaskNamespaceWrapper : WrapperCodeUnit
    {
        public const string SourceCode = @"
using System;
using System.Threading.Tasks;
{0}

class Test
{{
    async Task TestMethod()
    {{
{1}
    }}
}}
";

        public TaskNamespaceWrapper() : base(SourceCode)
        {
            
        }
    }
}