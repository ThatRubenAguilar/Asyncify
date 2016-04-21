using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class TypeFactory
    {
        /// <summary>
        /// Creates a type syntax from a type string, processing starts at startIndex.
        /// </summary>
        public static TypeSyntax CreateTypeSyntax(string typeName, int startIndex = 0)
        {
            TypeSyntax typeSyntax;
            CreateTypeSyntax(typeName, startIndex, out typeSyntax);
            return typeSyntax;
        }
        /// <summary>
        /// Creates a generic type argument list from a string, processing starts at startIndex within the string
        /// </summary>
        public static TypeArgumentListSyntax CreateGenericTypeArgumentList(string typeName, int startIndex = 0)
        {
            TypeArgumentListSyntax typeArgumentList;
            CreateGenericTypeArgumentList(typeName, startIndex, out typeArgumentList);
            return typeArgumentList;
        }

        private static readonly char[] TypeTokens = { '.', ',', '<', '>' };
        /// <summary>
        /// Creates a type syntax from a type string, processing starts at startIndex.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="startIndex"></param>
        /// <param name="fullTypeSyntax"></param>
        /// <returns>The number of characters processed in the string</returns>
        static int CreateTypeSyntax(string typeName, int startIndex, out TypeSyntax fullTypeSyntax)
        {
            var nextTokenIndex = typeName.IndexOfAny(TypeTokens, startIndex);
            var prevStartIndex = startIndex;
            NameSyntax nameTypeSyntax = null;
            if (nextTokenIndex == -1)
            {
                nameTypeSyntax = SyntaxFactory.IdentifierName(typeName);
            }
            else
            {
                var nextToken = typeName.Substring(nextTokenIndex, 1);
                var prevIdentifier = typeName.Substring(prevStartIndex, nextTokenIndex - prevStartIndex);
                prevStartIndex = nextTokenIndex + 1;
                switch (nextToken)
                {
                    case ".":
                        nameTypeSyntax = SyntaxFactory.IdentifierName(prevIdentifier);
                        break;
                    case "<":
                        TypeArgumentListSyntax typeParamList;
                        var searchedLength = CreateGenericTypeArgumentList(typeName, prevStartIndex, out typeParamList);
                        prevStartIndex += searchedLength;
                        nameTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(prevIdentifier),
                            typeParamList);
                        break;
                    default:
                        throw new ArgumentException($"Unexpected token '{nextToken}' at index {nextTokenIndex} of type syntax {typeName}");
                }
                nextTokenIndex = typeName.IndexOfAny(TypeTokens, prevStartIndex);
            }
            while (nextTokenIndex != -1)
            {
                var nextToken = typeName.Substring(nextTokenIndex, 1);
                var prevIdentifier = typeName.Substring(prevStartIndex, nextTokenIndex - prevStartIndex);
                prevStartIndex = nextTokenIndex + 1;

                switch (nextToken)
                {
                    case ".":
                        nameTypeSyntax = SyntaxFactory.QualifiedName(nameTypeSyntax, SyntaxFactory.IdentifierName(prevIdentifier));
                        break;
                    case "<":
                        TypeArgumentListSyntax typeParamList;
                        var searchedLength = CreateGenericTypeArgumentList(typeName, prevStartIndex, out typeParamList);
                        prevStartIndex += searchedLength;
                        nameTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(prevIdentifier),
                            typeParamList);
                        break;
                    case ">":
                        fullTypeSyntax = nameTypeSyntax;
                        return prevStartIndex;
                    default:
                        throw new ArgumentException($"Unexpected token '{nextToken}' at index {nextTokenIndex} of type syntax {typeName}");
                }

                nextTokenIndex = typeName.IndexOfAny(TypeTokens, prevStartIndex);
            }
            fullTypeSyntax = nameTypeSyntax;
            return typeName.Length - startIndex;
        }
        /// <summary>
        /// Creates a generic type argument list from a string, processing starts at startIndex within the string
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="startIndex"></param>
        /// <param name="typeArgumentListSynax"></param>
        /// <returns>The number of characters processed in the string</returns>
        static int CreateGenericTypeArgumentList(string typeName, int startIndex, out TypeArgumentListSyntax typeArgumentListSynax)
        {
            // Identifier<> Case
            var nextTokenIndex = typeName.IndexOfAny(TypeTokens, startIndex);
            var nextToken = typeName.Substring(nextTokenIndex, 1);
            if (nextToken == ">")
            {
                var prevIdentifier = typeName.Substring(startIndex, nextTokenIndex - startIndex);
                if (String.IsNullOrWhiteSpace(prevIdentifier))
                {
                    typeArgumentListSynax = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(new TypeSyntax[] { SyntaxFactory.OmittedTypeArgument() }));
                    return nextTokenIndex + 1;
                }
            }

            // Identifier<Identifier...> Case
            TypeSyntax currentTypeSyntax;
            var searchedLength = CreateTypeSyntax(typeName, startIndex, out currentTypeSyntax);
            var prevStartIndex = startIndex + searchedLength;
            nextTokenIndex = typeName.IndexOfAny(TypeTokens, prevStartIndex);
            List<TypeSyntax> typeArgumentList = new List<TypeSyntax>();
            typeArgumentList.Add(currentTypeSyntax);
            while (nextTokenIndex != -1)
            {
                nextToken = typeName.Substring(nextTokenIndex, 1);
                prevStartIndex = nextTokenIndex + 1;

                switch (nextToken)
                {
                    case ",":
                        searchedLength = CreateTypeSyntax(typeName, prevStartIndex, out currentTypeSyntax);
                        prevStartIndex += searchedLength;
                        typeArgumentList.Add(currentTypeSyntax);
                        break;
                    case ">":
                        typeArgumentListSynax = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArgumentList));
                        return prevStartIndex;
                    default:
                        throw new ArgumentException($"Unexpected token '{nextToken}' at index {nextTokenIndex} of type syntax {typeName}");
                }

                nextTokenIndex = typeName.IndexOfAny(TypeTokens, prevStartIndex);
            }
            typeArgumentListSynax = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArgumentList));
            return typeName.Length - startIndex;
        }
    }
}