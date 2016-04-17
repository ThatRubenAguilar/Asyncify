using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Asyncify.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

// Code partially originated from https://github.com/Wintellect/Wintellect.Analyzers/blob/master/Source/Wintellect.Analyzers/Wintellect.Analyzers/Extensions/NodeExtensions.cs

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class NodeExtensions
    {
        /// <summary>
        /// Used to look up non user code attributes in HasIgnorableAttributes.
        /// </summary>
        private static readonly Regex nonUserAttributeRegex = new Regex(@".*(GeneratedCode|DebuggerNonUserCode)(Attribute)?",
                                                                        RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Returns the first parent of a node that is one of the specified types.
        /// </summary>
        /// <param name="node">
        /// The node to check.
        /// </param>
        /// <param name="types">
        /// The array of types to check.
        /// </param>
        /// <returns>
        /// If one of the parents in the <paramref name="types"/> array matches, that type, otherwise null.
        /// </returns>
        /// <remarks>
        /// Full credit to the awesome Giggio for the inspiration 
        /// https://github.com/code-cracker/code-cracker/blob/master/src/Common/CodeCracker.Common/Extensions/AnalyzerExtensions.cs
        /// </remarks>
        public static SyntaxNode FirstAncestorOfType(this SyntaxNode node, params Type[] types)
        {
            SyntaxNode currentNode = node;
            while (currentNode != null)
            {
                SyntaxNode parent = currentNode.Parent;
                if (parent != null)
                {
                    for (Int32 i = 0; i < types.Length; i++)
                    {
                        if (parent.GetType() == types[i])
                        {
                            return parent;
                        }
                    }
                }

                currentNode = parent;
            }

            return null;
        }

        /// <summary>
        /// Returns the current or first parent of a node that is one of the specified types.
        /// </summary>
        /// <param name="node">
        /// The node to check.
        /// </param>
        /// <param name="types">
        /// The array of types to check.
        /// </param>
        /// <returns>
        /// If the self or one of the parents in the <paramref name="types"/> array matches, that type, otherwise null.
        /// </returns>
        /// <remarks>
        /// Full credit to the awesome Giggio for the inspiration
        /// https://github.com/code-cracker/code-cracker/blob/master/src/Common/CodeCracker.Common/Extensions/AnalyzerExtensions.cs
        /// </remarks>
        public static SyntaxNode FirstAncestorOrSelfOfType(this SyntaxNode node, params Type[] types)
        {
            SyntaxNode currentNode = node;
            while (currentNode != null)
            {
                for (Int32 i = 0; i < types.Length; i++)
                {
                    if (currentNode.GetType() == types[i])
                    {
                        return currentNode;
                    }
                }

                currentNode = currentNode.Parent;
            }

            return null;
        }

        /// <summary>
        /// Returns true if this node is part of a looping construct.
        /// </summary>
        /// <param name="node">
        /// The node to check.
        /// </param>
        /// <returns>
        /// True if part of a looping construct, false otherwise.
        /// </returns>
        public static Boolean IsNodeInALoop(this SyntaxNode node) => null != node.FirstAncestorOfType(typeof(ForEachStatementSyntax),
                                                                                                      typeof(ForStatementSyntax),
                                                                                                      typeof(WhileStatementSyntax),
                                                                                                      typeof(DoStatementSyntax));

        /// <summary>
        /// Adds the string specified using statement to the CompilationUnitSyntax if that using is not already present.
        /// </summary>
        /// <remarks>
        /// The using statement, if inserted, will be followed by a CR/LF.
        /// </remarks>
        /// <param name="unit">
        /// The type being extended.
        /// </param>
        /// <param name="usingString">
        /// The string statement such as "System.Diagnostics" or "Microsoft.CodeAnalysis.CSharp.Syntax".
        /// </param>
        /// <returns>
        /// The CompilationUnitSyntax in <paramref name="unit"/>.
        /// </returns>
        public static CompilationUnitSyntax AddUsingIfNotPresent(this CompilationUnitSyntax unit, String usingString)
        {
            var t = unit.ChildNodes().OfType<UsingDirectiveSyntax>().Where(u => u.Name.ToString().Equals(usingString));
            if (!t.Any())
            {
                UsingDirectiveSyntax usingDirective = SyntaxFactoryHelper.QualifiedUsing(usingString);

                // I'll never understand why WithAdditionalAnnotations(Formatter.Annotation) isn't the default. Picking
                // up the default formatting should be the default and would make developing rules much easier.
                unit = unit.AddUsings(usingDirective).WithAdditionalAnnotations(Formatter.Annotation);
            }
            return unit;
        }

        /// <summary>
        /// Returns true if the PropertyDeclarationSyntax has a getter method.
        /// </summary>
        /// <param name="property">
        /// The property to check.
        /// </param>
        /// <returns>
        /// True if the property has a getter, false otherwise.
        /// </returns>
        public static Boolean HasGetter(this PropertyDeclarationSyntax property)
        {
            if (property.AccessorList == null)
            {
                return false;
            }
            return property.AccessorList.Accessors.Where(t => t.IsKind(SyntaxKind.GetAccessorDeclaration)).Any();
        }

        /// <summary>
        /// From a FieldDeclarationSyntax, returns the field name.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static String FieldName(this FieldDeclarationSyntax field)
        {
            var vars = field.DescendantNodes().Where(i => i.IsKind(SyntaxKind.VariableDeclarator));
            VariableDeclaratorSyntax varName = (VariableDeclaratorSyntax)vars.First();

            return varName.Identifier.ToString();
        }

        /// <summary>
        /// Returns true if this is generated or non user code.
        /// </summary>
        /// <param name="node">
        /// The SyntaxNode to check.
        /// </param>
        /// <returns>
        /// True if this node or its defining types are non user code.
        /// </returns>
        public static Boolean IsGeneratedOrNonUserCode(this SyntaxNode node)
        {
            // Look at the type, which could be nested.
            TypeDeclarationSyntax currType = (TypeDeclarationSyntax)(node.FirstAncestorOrSelfOfType(typeof(ClassDeclarationSyntax),
                                                                                                    typeof(StructDeclarationSyntax)));
            while (currType != null)
            {
                if (currType.AttributeLists.HasIgnorableAttributes())
                {
                    return true;
                }
                currType = (TypeDeclarationSyntax)(currType.FirstAncestorOfType(typeof(ClassDeclarationSyntax),
                                                                                typeof(StructDeclarationSyntax)));
            }

            // That's as far as we can go with nodes. There's no assembly with them.
            return false;
        }

        /// <summary>
        /// Takes an SyntaxList of Attributes and checks if any are non user code attributes.
        /// </summary>
        /// <param name="attributeList">
        /// The list to check.
        /// </param>
        /// <returns>
        /// True if the list contains a non user code attribute.
        /// </returns>
        public static Boolean HasIgnorableAttributes(this SyntaxList<AttributeListSyntax> attributeList)
        {

            for (Int32 i = 0; i < attributeList.Count; i++)
            {
                AttributeListSyntax currAttrList = attributeList[i];
                for (Int32 k = 0; k < currAttrList.Attributes.Count; k++)
                {
                    AttributeSyntax attr = currAttrList.Attributes[k];
                    if (nonUserAttributeRegex.IsMatch(attr.Name.ToString()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Extracts trivia from tokens within the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="excludeTokens">tokens to exclude from trivia extraction</param>
        /// <returns></returns>
        public static List<SyntaxTrivia> ExtractTrivia(this SyntaxNode node, IEnumerable<SyntaxToken> excludeTokens)
        {
            var excluseHashSet = new HashSet<SyntaxToken>(excludeTokens);
            var triviaList = new List<SyntaxTrivia>();
            foreach (var token in node.DescendantTokens())
            {
                if (excluseHashSet.Contains(token))
                    continue;
                if (token.HasLeadingTrivia || token.HasTrailingTrivia)
                    triviaList.AddRange(token.GetAllTrivia());
            }

            return triviaList;
        }

        /// <summary>
        /// Extracts trivia from tokens within the node
        /// </summary>
        /// <returns></returns>
        public static List<SyntaxTrivia> ExtractTrivia(this SyntaxNode node)
        {
            var triviaList = new List<SyntaxTrivia>();
            foreach (var token in node.DescendantTokens())
            {
                if (token.HasLeadingTrivia || token.HasTrailingTrivia)
                    triviaList.AddRange(token.GetAllTrivia());
            }

            return triviaList;
        }

        public static string ToStringWithoutTrivia(this SyntaxNode node)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var token in node.DescendantTokens())
            {
                sb.Append(token);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the token that is before stopToken or the default.
        /// </summary>
        /// <param name="node">node to search in</param>
        /// <param name="stopToken">token to stop at</param>
        /// <param name="considerTrivia">Whether to consider trivia as tokens</param>
        /// <returns></returns>
        public static SyntaxToken GetTokenBeforeOrDefault(this SyntaxNode node, SyntaxToken stopToken, bool considerTrivia = false)
        {
            SyntaxToken followToken = default(SyntaxToken);

            foreach (var token in node.DescendantTokens(descendIntoTrivia:considerTrivia))
            {
                if (token.Equals(stopToken))
                    break;
                followToken = token;
            }

            return followToken;
        }
        /// <summary>
        /// Gets the token that is after stopToken or the default.
        /// </summary>
        /// <param name="node">node to search in</param>
        /// <param name="stopToken">token to stop at</param>
        /// <param name="considerTrivia">Whether to consider trivia as tokens</param>
        /// <returns></returns>
        public static SyntaxToken GetTokenAfterOrDefault(this SyntaxNode node, SyntaxToken stopToken, bool considerTrivia = false)
        {
            SyntaxToken followToken = default(SyntaxToken);

            foreach (var token in node.DescendantTokens(descendIntoTrivia:considerTrivia))
            {
                if (followToken.Equals(stopToken))
                    return token;
                followToken = token;
            }

            return default(SyntaxToken);
        }

        /// <summary>
        /// Gets the token that is before stopNode's first token or the default.
        /// </summary>
        /// <param name="node">node to search in</param>
        /// <param name="stopNode">node whose first token to stop at</param>
        /// <returns></returns>
        public static SyntaxToken GetTokenBeforeOrDefault(this SyntaxNode node, SyntaxNode stopNode)
        {
            SyntaxToken followToken = default(SyntaxToken);
            var stopToken = stopNode.GetFirstToken(true, true, true, true);

            foreach (var token in node.DescendantTokens(descendIntoTrivia:true))
            {
                if (token.Equals(stopToken))
                    break;
                followToken = token;
            }

            return followToken;
        }
        /// <summary>
        /// Gets the token that is after stopNode's last token or the default.
        /// </summary>
        /// <param name="node">node to search in</param>
        /// <param name="stopNode">node whose last token to stop at</param>
        /// <returns></returns>
        public static SyntaxToken GetTokenAfterOrDefault(this SyntaxNode node, SyntaxNode stopNode)
        {
            SyntaxToken followToken = default(SyntaxToken);
            var stopToken = stopNode.GetLastToken(true, true, true, true);

            foreach (var token in node.DescendantTokens(descendIntoTrivia:true))
            {
                if (followToken.Equals(stopToken))
                    return token;
                followToken = token;
            }

            return default(SyntaxToken);
        }

        /// <summary>
        /// Merges the trivia on the edge of innerNode contained within outerNode fully into innerNode
        /// </summary>
        /// <param name="innerNode">node to merge trivia into</param>
        /// <param name="outerNode">node which contains innedNode</param>
        /// <returns>outerNode with trivia fully merged into innerNode</returns>
        public static SyntaxNode MergeEdgeTriviaIn(this SyntaxNode innerNode, SyntaxNode outerNode)
        {
            // TODO: Improve this with merge in/out option
            var rewriter = new EdgeTriviaMergingRewriter(innerNode, outerNode);
            var mergedTriviaNode = rewriter.Visit(outerNode);
            rewriter.EnsureNodesTouched();
            return mergedTriviaNode;
        }
        /// <summary>
        /// Merges the trivia on the edge of innerNode contained within innerNode.Parent fully into innerNode
        /// </summary>
        /// <param name="innerNode">node to merge trivia into</param>
        /// <returns>innerNode.Parent with trivia fully merged into innerNode</returns>
        public static SyntaxNode MergeEdgeTriviaIn(this SyntaxNode innerNode)
        {
            return innerNode.MergeEdgeTriviaIn(innerNode.Parent);
        }

        public static int GetChildIndex(this SyntaxNode node, SyntaxNode searchNode)
        {
            return node.ChildNodes().TakeWhile(n => !n.Equals(searchNode) && !n.Contains(searchNode)).Count();
        }
        public static int GetDescendantIndex(this SyntaxNode node, SyntaxNode searchNode)
        {
            return node.DescendantNodes().TakeWhile(n => !n.Equals(searchNode)).Count();
        }

        public static T GetChildNodeAtIndex<T>(this SyntaxNode node, int index) where T : SyntaxNode
        {
            return node.ChildNodes().ElementAt(index) as T;
        }  
        public static T GetDescendantNodeAtIndex<T>(this SyntaxNode node, int index) where T : SyntaxNode
        {
            return node.DescendantNodes().ElementAt(index) as T;
        }

        public static LambdaExpressionSyntax FindContainingLambda(this SyntaxNode node)
        {
            return node.AncestorsAndSelf().OfType<LambdaExpressionSyntax>().FirstOrDefault();
        }
    }
}
