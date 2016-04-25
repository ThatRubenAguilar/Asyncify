using System;
using System.Text;
using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class FullTriviaCode : MergedCodeUnit
    {
        public const int DefaultIndents = TestSourceCode.DefaultIndents;
        public const int IndentSize = TestSourceCode.IndentSize;

        public const string WrapperSourceCode = @"// One Line Comment 
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

        public FullTriviaCode(int ifDirectiveIndents = 0, int elseDirectiveIndents = 0, int multilineCommentIndents = 0, int regionIndents = 0) : base(new WrapperCodeUnit(WrapperSourceCode), CreateWhitespaceTrivia(ifDirectiveIndents, elseDirectiveIndents, multilineCommentIndents, regionIndents))
        {
        }

        private static SourceCodeUnit[] CreateWhitespaceTrivia(int ifDirectiveIndents = 0, int elseDirectiveIndents = 0, int multilineCommentIndents = 0, int regionIndents = 0)
        {
            var ifDirectiveCode = new SourceCodeUnit(CreateSpaces(ifDirectiveIndents * IndentSize));
            var elseDirectiveCode = new SourceCodeUnit(CreateSpaces(elseDirectiveIndents * IndentSize));
            var multilineCommentCode = new SourceCodeUnit(CreateSpaces(multilineCommentIndents * IndentSize));
            var regionCode = new SourceCodeUnit(CreateSpaces(regionIndents * IndentSize));
            return new []{ ifDirectiveCode, elseDirectiveCode, multilineCommentCode, regionCode};
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
        /// Creates trivia text which is indented ifDirectiveIndents amount for ifDirective and will be indented to regionIndents amount for regions. This is the format output from Formatter running over the trivia text. Typically ifDirective is aligned to the closest block indent, and region is aligned to the closest variable indent.
        /// </summary>
        /// <returns></returns>
        public static FullTriviaCode TriviaTextFormatted(int regionIndents, int ifDirectiveIndents)
        {
            return TriviaTextCustom(ifDirectiveIndents, 0, 0, regionIndents);
        }
        /// <summary>
        /// Creates trivia text which is indented ifDirectiveIndents amount for ifDirective and will be indented to regionIndents amount for regions. This is the format output from Formatter running over the trivia text. This handles the common case of ifDirectiveIndents = regionIdents-1
        /// </summary>
        /// <returns></returns>
        public static FullTriviaCode TriviaTextFormatted(int regionIndents = DefaultIndents)
        {
            var ifDirectiveIndents = regionIndents - 1;
            if (ifDirectiveIndents < 0)
                ifDirectiveIndents = 0;
            return TriviaTextCustom(ifDirectiveIndents, 0, 0, regionIndents);
        }
        /// <summary>
        /// Creates trivia text with a fully custom format
        /// </summary>
        /// <param name="ifDirectiveIndents"></param>
        /// <param name="elseDirectiveIndents"></param>
        /// <param name="multilineCommentIndents"></param>
        /// <param name="regionIndents"></param>
        /// <returns></returns>
        public static FullTriviaCode TriviaTextCustom(int ifDirectiveIndents = 0, int elseDirectiveIndents = 0, int multilineCommentIndents = 0, int regionIndents = 0)
        {
            return new FullTriviaCode(ifDirectiveIndents, elseDirectiveIndents, multilineCommentIndents, regionIndents);
        }
        
        /// <summary>
        /// Creates trivia text which is uniformly indented formattedIndents amount
        /// </summary>
        /// <param name="formattedIndents"></param>
        /// <returns></returns>
        public static FullTriviaCode TriviaTextUniform(int formattedIndents = DefaultIndents)
        {
            return TriviaTextCustom(formattedIndents, formattedIndents, formattedIndents, formattedIndents);
        }
    }
}