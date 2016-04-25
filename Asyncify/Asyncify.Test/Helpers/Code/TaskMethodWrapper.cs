namespace Asyncify.Test.Helpers.Code
{
    public class TaskMethodWrapper : WrapperCodeUnit
    {
        public const string SourceCode = @"
using System;
using System.Threading.Tasks;

class Test
{{
    {0}
    {{
{1}
    }}
}}
";

        public TaskMethodWrapper() : base(SourceCode)
        {
            
        }
    }
}