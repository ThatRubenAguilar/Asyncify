using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Asyncify.Extensions
{
    public class LambdaBodyInfo
    {
        public SyntaxNode RawBodyNode { get; }
        public LambdaExpressionSyntax ContainingLambdaSyntax { get; }
        public bool IsBlock { get; }
        public bool IsExpression => !IsBlock;

        public LambdaBodyInfo(BlockSyntax blockBody, LambdaExpressionSyntax containingLambda)
        {
            if (blockBody == null)
                throw new ArgumentNullException(nameof(blockBody));
            if (containingLambda == null)
                throw new ArgumentNullException(nameof(containingLambda));

            if (!containingLambda.Contains(blockBody))
                throw new ArgumentException($"{nameof(containingLambda)}: '{containingLambda.ToFullString()}' does not contain {nameof(blockBody)}: '{blockBody.ToFullString()}'");
            RawBodyNode = blockBody;
            ContainingLambdaSyntax = containingLambda;
            IsBlock = true;
        }
        public LambdaBodyInfo(ExpressionSyntax expressionBody, LambdaExpressionSyntax containingLambda)
        {
            if (expressionBody == null)
                throw new ArgumentNullException(nameof(expressionBody));
            if (containingLambda == null)
                throw new ArgumentNullException(nameof(containingLambda));

            if (!containingLambda.Contains(expressionBody))
                throw new ArgumentException($"{nameof(containingLambda)}: '{containingLambda.ToFullString()}' does not contain {nameof(expressionBody)}: '{expressionBody.ToFullString()}'");
            RawBodyNode = expressionBody;
            ContainingLambdaSyntax = containingLambda;
            IsBlock = false;
        }
    }
}