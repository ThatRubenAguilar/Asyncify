using System.Collections.Generic;
using Asyncify.Test.Extensions;
using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class SourceCodeUnit
    {
        public string Code { get; }

        public SourceCodeUnit(string code)
        {
            Code = code;
        }

        public ResultLocation FindSourceLocation(string expression)
        {
            return Code.FindSourceLocation(expression);
        }

        public IList<ResultLocation> FindSourceLocations(string expression)
        {
            return Code.FindSourceLocations(expression);
        }


        public override string ToString()
        {
            return Code;
        }
    }
}