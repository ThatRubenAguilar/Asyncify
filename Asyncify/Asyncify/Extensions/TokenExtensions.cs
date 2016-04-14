using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class TokenExtensions
    {
        /// <summary>
        /// Merges two token's trivia together into the 'to' token, ex. from &gt; from's trailing &gt; to's leading &gt; to 
        /// </summary>
        /// <param name="from">node which to merge from</param>
        /// <param name="to">node which to merge into</param>
        /// <returns>new to token with from's trailing and to's leading trivia</returns>
        public static SyntaxToken MergeTrailingTriviaInto(this SyntaxToken from, SyntaxToken to)
        {
            if (!from.HasTrailingTrivia)
                return to;

            if (!to.HasLeadingTrivia)
                return to.WithLeadingTrivia(from.TrailingTrivia);

            return to.WithLeadingTrivia(from.TrailingTrivia.Concat(to.LeadingTrivia));
        }
        /// <summary>
        /// Merges two token's trivia together into a list to replace the 'to' token's leading trivia list, ex. from &gt; from's trailing &gt; to's leading &gt; to 
        /// </summary>
        /// <param name="from">node which to merge from</param>
        /// <param name="to">node which to merge into</param>
        /// <returns>list with from's trailing and to's leading trivia</returns>
        public static IEnumerable<SyntaxTrivia> GetMergedTrailingTrivia(this SyntaxToken from, SyntaxToken to)
        {
            if (!from.HasTrailingTrivia)
                return null;

            if (!to.HasLeadingTrivia)
                return from.TrailingTrivia;

            return from.TrailingTrivia.Concat(to.LeadingTrivia);
        }
        /// <summary>
        /// Merges two token's trivia together into the 'to' token, ex to &lt; to's trailing &lt; from's leading &lt; from
        /// </summary>
        /// <param name="from">node which to merge from</param>
        /// <param name="to">node which to merge into</param>
        /// <returns>new to token with from's leading and to's trailing trivia</returns>
        public static SyntaxToken MergeLeadingTriviaInto(this SyntaxToken from, SyntaxToken to)
        {
            if (!from.HasLeadingTrivia)
                return to;

            if (!to.HasTrailingTrivia)
                return to.WithTrailingTrivia(from.LeadingTrivia);

            return to.WithTrailingTrivia(to.TrailingTrivia.Concat(from.LeadingTrivia));
        }
        /// <summary>
        /// Merges two token's trivia together into a list to replace the 'to' token's trailing trivia list, ex to &lt; to's trailing &lt; from's leading &lt; from
        /// </summary>
        /// <param name="from">node which to merge from</param>
        /// <param name="to">node which to merge into</param>
        /// <returns>list with from's leading and to's trailing trivia</returns>
        public static IEnumerable<SyntaxTrivia> GetMergedLeadingTrivia(this SyntaxToken from, SyntaxToken to)
        {
            if (!from.HasLeadingTrivia)
                return null;

            if (!to.HasTrailingTrivia)
                return from.LeadingTrivia;

            return to.TrailingTrivia.Concat(from.LeadingTrivia);
        }

        public static bool IsDefault(this SyntaxToken token)
        {
            return default(SyntaxToken).Equals(token);
        }
    }
}