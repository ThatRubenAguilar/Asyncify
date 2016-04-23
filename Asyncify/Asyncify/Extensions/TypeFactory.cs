using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Asyncify.Extensions
{
    [DebuggerDisplay("nonUserAttributeRegex={nonUserAttributeRegex}")]
    public static class TypeFactory
    {
        static readonly CSharpParseOptions Options = new CSharpParseOptions(kind: SourceCodeKind.Script);
        /// <summary>
        /// Creates a type syntax from a type string.
        /// </summary>
        public static async Task<TypeSyntax> CreateTypeSyntax(string typeName)
        {
            var parsedTree = CSharpSyntaxTree.ParseText($"typeof({typeName})", Options);
            var treeRoot = await parsedTree.GetRootAsync();
            var typeNameNode = treeRoot.DescendantNodes().OfType<TypeSyntax>().FirstOrDefault();
            return typeNameNode;
        }
    }
}