using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class ExpressionExtensions
    {

        /// <summary>
        /// Converts a blocking task such as Task.Result/GetAwaiter().GetResult() into an awaited statement.
        /// The awaited statement is parenthesized when caller task is generic, the result of the task is used, and the task expression isnt already parenthesized.
        /// </summary>
        /// <param name="blockingTaskExpression">Origin expression that contains the task node and blocking task call as children</param>
        /// <param name="syntaxEditor">Editor to change the document</param>
        /// <param name="keepTaskNode">Calling task node that will not be removed from the blockingTaskExpression</param>
        /// <param name="callerIsGenericTask">Whether the task in keepTaskNode is a generic task type or not</param>
        /// <returns></returns>
        public static SyntaxNode AwaitBlockingTaskSyntax(this ExpressionSyntax blockingTaskExpression, SyntaxEditor syntaxEditor, ExpressionSyntax keepTaskNode, bool callerIsGenericTask)
        {
            var excludeKeepNodeTokens = keepTaskNode.DescendantTokens().ToArray();

            var triviaList = blockingTaskExpression.ExtractTrivia(excludeKeepNodeTokens);

            if (keepTaskNode.HasTrailingTrivia)
                triviaList.InsertRange(0, keepTaskNode.GetTrailingTrivia());

            var alreadyParenthesized = blockingTaskExpression.Parent as ParenthesizedExpressionSyntax;
            // last node should be relative parent unless something comes after
            var resultUsed = !blockingTaskExpression.Parent.ChildNodes().Last().Equals(blockingTaskExpression);

            // Surround with parenthesis only when the result is used and it is a generic task and it isnt already parenthesized
            if (resultUsed && alreadyParenthesized == null && callerIsGenericTask)
            {
                // Move trivia from keep node tokens to outside parens as it more closely resembles what would be expected
                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepTaskNode.WithoutTrivia());
                var newParenExpr =
                    SyntaxFactory.ParenthesizedExpression(newAwaitExpr)
                        .WithLeadingTrivia(keepTaskNode.GetLeadingTrivia())
                        .WithTrailingTrivia(triviaList);

                syntaxEditor.ReplaceNode(blockingTaskExpression, (node, gen) => newParenExpr);
            }
            else
            {
                // Move trivia up with the removal of .GetAwaiter().GetResult()/.Result, we are already in parens or dont need parens so dont mess with it.
                var newAwaitExpr = SyntaxFactory.AwaitExpression(keepTaskNode.WithoutTrivia())
                    .WithLeadingTrivia(keepTaskNode.GetLeadingTrivia())
                    .WithTrailingTrivia(triviaList);

                syntaxEditor.ReplaceNode(blockingTaskExpression, (node, gen) => newAwaitExpr);
            }

            return syntaxEditor.GetChangedRoot();
        }
    }
}