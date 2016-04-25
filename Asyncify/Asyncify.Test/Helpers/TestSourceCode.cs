using Asyncify.Test.Helpers.Code;

namespace TestHelper
{
    public static class TestSourceCode
    {
        public const int TabSize = 4;
        public const int IndentSize = TabSize;
        public const int DefaultIndents = 2;
        
        public static readonly string FullTriviaText = FullTriviaCode.TriviaTextCustom(DefaultIndents, 0, 0, DefaultIndents).ToString();
        
        
    }
}