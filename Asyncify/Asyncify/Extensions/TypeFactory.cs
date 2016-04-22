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
        public static TypeSyntax CreateTypeSyntax(string typeName, bool segmentInsideGeneric = false, int startIndex = 0)
        {
            if (startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} was {startIndex} which is longer than {nameof(typeName)}.Length of {typeName.Length}");
            TypeSyntax typeSyntax;
            var lexer = new TypeNameLexer(typeName, startIndex, typeName.Length);
            CreateTypeSyntax(lexer, segmentInsideGeneric, out typeSyntax);
            return typeSyntax;
        }

        /// <summary>
        /// Creates a type syntax from a type string, processing starts at startIndex and continues for length number of characters.
        /// </summary>
        public static TypeSyntax CreateTypeSyntax(string typeName, bool segmentInsideGeneric, int startIndex, int length)
        {
            if (length + startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(startIndex)} was {startIndex} and {nameof(length)} was {length} which combined is longer than {nameof(typeName)}.Length of {typeName.Length}");
            if (startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} was {startIndex} which is longer than {nameof(typeName)}.Length of {typeName.Length}");
            TypeSyntax typeSyntax;
            var lexer = new TypeNameLexer(typeName, startIndex, length);
            CreateTypeSyntax(lexer, segmentInsideGeneric, out typeSyntax);
            return typeSyntax;
        }
        /// <summary>
        /// Creates a generic type argument list from a string, processing starts at startIndex within the string
        /// </summary>
        public static TypeArgumentListSyntax CreateGenericTypeArgumentList(string typeName, int startIndex = 0)
        {
            if (startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} was {startIndex} which is longer than {nameof(typeName)}.Length of {typeName.Length}");
            TypeArgumentListSyntax typeArgumentList;
            var lexer = new TypeNameLexer(typeName, startIndex, typeName.Length);
            CreateGenericTypeArgumentList(lexer, out typeArgumentList);
            return typeArgumentList;
        }
        /// <summary>
        /// Creates a generic type argument list from a string, processing starts at startIndex within the string and continues for length number of characters.
        /// </summary>
        public static TypeArgumentListSyntax CreateGenericTypeArgumentList(string typeName, int startIndex, int length)
        {
            if (length+startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(startIndex)} was {startIndex} and {nameof(length)} was {length} which combined is longer than {nameof(typeName)}.Length of {typeName.Length}");
            if (startIndex > typeName.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} was {startIndex} which is longer than {nameof(typeName)}.Length of {typeName.Length}");

            TypeArgumentListSyntax typeArgumentList;
            var lexer = new TypeNameLexer(typeName, startIndex, length);
            CreateGenericTypeArgumentList(lexer, out typeArgumentList);
            return typeArgumentList;
        }

        private static readonly char[] TypeTokens = { '.', ',', '<', '>' };

        /// <summary>
        /// Creates a type syntax from a type string.
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="insideGeneric">Whether the typeName should be treated as if it is within generic type brackets or not.</param>
        /// <param name="fullTypeSyntax"></param>
        /// <returns></returns>
        static TypeNameLexer CreateTypeSyntax(TypeNameLexer lexer, bool insideGeneric,  out TypeSyntax fullTypeSyntax)
        {
            lexer.NextToken();
            NameSyntax nameTypeSyntax = null;
            if (!lexer.HasMoreTokens)
            {
                nameTypeSyntax = SyntaxFactory.IdentifierName(lexer.ProceedingIdentifier);
            }
            else
            {
                var nextToken = lexer.Token;
                var prevIdentifier = lexer.ProceedingIdentifier;
                switch (nextToken)
                {
                    case ".":
                        nameTypeSyntax = SyntaxFactory.IdentifierName(prevIdentifier);
                        break;
                    case "<":
                        TypeArgumentListSyntax typeParamList;
                        lexer = CreateGenericTypeArgumentList(lexer, out typeParamList);
                        nameTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(prevIdentifier),
                            typeParamList);
                        break;
                    case ">":
                    case ",":
                        if (insideGeneric)
                        {
                            if (string.IsNullOrWhiteSpace(prevIdentifier))
                            {
                                fullTypeSyntax = SyntaxFactory.OmittedTypeArgument();
                                return lexer;
                            }

                            fullTypeSyntax = SyntaxFactory.IdentifierName(prevIdentifier);
                            return lexer; 
                        }

                        lexer.ThrowTokenError();
                        break;
                    default:
                        lexer.ThrowTokenError();
                        break;
                }
                lexer.NextToken();
            }
            while (lexer.HasMoreTokens)
            {
                var nextToken = lexer.Token;
                var prevIdentifier = lexer.ProceedingIdentifier;

                switch (nextToken)
                {
                    case ".":
                        nameTypeSyntax = SyntaxFactory.QualifiedName(nameTypeSyntax, SyntaxFactory.IdentifierName(prevIdentifier));
                        break;
                    case "<":
                        TypeArgumentListSyntax typeParamList;
                        lexer = CreateGenericTypeArgumentList(lexer, out typeParamList);
                        nameTypeSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier(prevIdentifier),
                            typeParamList);
                        break;
                    case ">":
                    case ",":
                        if (insideGeneric)
                        {
                            if (string.IsNullOrWhiteSpace(prevIdentifier))
                            {
                                lexer.ThrowIdentifierError();
                            }

                            fullTypeSyntax = nameTypeSyntax;
                            return lexer;
                        }

                        lexer.ThrowTokenError();
                        break;
                    default:
                        lexer.ThrowTokenError();
                        break;
                }

                lexer.NextToken();
            }
            fullTypeSyntax = nameTypeSyntax;
            return lexer;
        }

        /// <summary>
        /// Creates a generic type argument list from a string
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="typeArgumentListSynax"></param>
        /// <returns></returns>
        static TypeNameLexer CreateGenericTypeArgumentList(TypeNameLexer lexer, out TypeArgumentListSyntax typeArgumentListSynax)
        {
            TypeSyntax currentTypeSyntax;
            lexer = CreateTypeSyntax(lexer, true, out currentTypeSyntax);
            List<TypeSyntax> typeArgumentList = new List<TypeSyntax>();
            typeArgumentList.Add(currentTypeSyntax);
            lexer.NextToken();
            while (lexer.HasMoreTokens)
            {
                var nextToken = lexer.Token;

                switch (nextToken)
                {
                    case ",":
                        lexer = CreateTypeSyntax(lexer, true, out currentTypeSyntax);
                        typeArgumentList.Add(currentTypeSyntax);
                        break;
                    case ">":
                        typeArgumentListSynax = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArgumentList));
                        return lexer;
                    default:
                        lexer.ThrowTokenError();
                        break;
                }
            }
            typeArgumentListSynax = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArgumentList));
            return lexer;
        }
    }
}