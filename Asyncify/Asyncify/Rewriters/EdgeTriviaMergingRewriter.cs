using System;
using System.Linq;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Asyncify.Rewriters
{
    /// <summary>
    /// Rewriter which merges trivia on the edge of innerExpression. One time use rewriter. Multiple calls to Visit(node) will result in failure.
    /// </summary>
    class EdgeTriviaMergingRewriter : CSharpSyntaxRewriter
    {
        private readonly SyntaxNode _outerExpression;
        private readonly SyntaxToken _firstToken;
        private readonly SyntaxToken _preFirstToken;
        private readonly SyntaxToken _lastToken;
        private readonly SyntaxToken _postLastToken;
        private bool _mergeFirst = false;
        private bool _mergeLast = false;
        private bool _touchedFirst = false;
        private bool _touchedLast = false;
        private bool _checkedOnce = false;
        private SyntaxNode _entryNode;

        public EdgeTriviaMergingRewriter(SyntaxNode innerExpression) 
            : this(innerExpression, innerExpression.Parent)
        {
        
        }

        public EdgeTriviaMergingRewriter(SyntaxNode innerExpression, SyntaxNode outerExpression)
        {
            _outerExpression = outerExpression;
            _firstToken = innerExpression.DescendantTokens().FirstOrDefault();
            _lastToken = innerExpression.DescendantTokens().LastOrDefault();
            if (_firstToken.IsDefault() || _lastToken.IsDefault())
                throw new ArgumentException($"Expression has no tokens to merge trivia into.");
        
            _preFirstToken = outerExpression.GetTokenBeforeOrDefault(_firstToken);
            _postLastToken = outerExpression.GetTokenAfterOrDefault(_lastToken);
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (!_checkedOnce && !_outerExpression.Contains(node))
                throw new ArgumentException(
                    $"{nameof(node)}: '{node.ToStringWithoutTrivia()}' is not contained by expression '{_outerExpression.ToStringWithoutTrivia()}' given to rewriter.");
            else
            {
                _checkedOnce = true;
                _entryNode = node;
            }
            return base.Visit(node);
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            // Process first tokens
            if (token.Kind() == _preFirstToken.Kind() && token.Equals(_preFirstToken))
            {
                _mergeFirst = true;
                _touchedFirst = true;
                if (!token.HasTrailingTrivia)
                    return token;
                return token.WithTrailingTrivia(null);
            }
            else if (_mergeFirst && token.Equals(_firstToken))
            {
                _mergeFirst = false;
                return _preFirstToken.MergeTrailingTriviaInto(token);
            }

            // Process last tokens
            if (token.Kind() == _lastToken.Kind() && token.Equals(_lastToken))
            {
                _mergeLast = true;
                _touchedLast = true;
                return _postLastToken.MergeLeadingTriviaInto(token);
            }
            else if (_mergeLast && token.Equals(_postLastToken))
            {
                _mergeLast = false;
                if (!token.HasLeadingTrivia)
                    return token;
                return token.WithLeadingTrivia(null);
            }

            return token;
        }

        public void EnsureNodesTouched()
        {
            // Verify preconditions of node being within outerExpression passed to ctor
            if (!_touchedFirst && !_touchedLast)
                throw new ArgumentException($"{nameof(_entryNode)}: '{_entryNode?.ToStringWithoutTrivia()}' had no nodes touched by rewriter.");
            else if (!_touchedFirst)
                throw new ArgumentException($"{nameof(_entryNode)}: '{_entryNode?.ToStringWithoutTrivia()}' had only first nodes touched by rewriter.");
            else if (!_touchedLast)
                throw new ArgumentException($"{nameof(_entryNode)}: '{_entryNode?.ToStringWithoutTrivia()}' had only last nodes touched by rewriter.");
        }
    }
}