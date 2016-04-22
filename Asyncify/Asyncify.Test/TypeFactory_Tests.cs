using System;
using System.Linq;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Asyncify.Test
{
    [TestClass]
    public class TypeFactory_Tests
    {
        // TODO: tests for type factory
        [TestMethod, TestCategory("TypeFactory")]
        public void Should_parse_type()
        {
            var typeName = "Blah";
            var typeMatches = new[] {typeof (IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Blah"};
            var type = TypeFactory.CreateTypeSyntax(typeName);
            CheckType(type, typeMatches, typeNameMatches);
        }

        [TestMethod, TestCategory("TypeFactory")]
        public void Should_parse_namespaced_type()
        {
            var typeName = "Ns.Blah";
            var typeMatches = new[] {typeof (IdentifierNameSyntax), typeof(QualifiedNameSyntax), typeof(IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Ns", "Ns.Blah", "Blah"};
            var type = TypeFactory.CreateTypeSyntax(typeName);
            CheckType(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public void Should_parse_generic_type()
        {
            var typeName = "Blah<int>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax), typeof(IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Blah<int>", "<int>", "int"};
            var type = TypeFactory.CreateTypeSyntax(typeName);
            CheckType(type, typeMatches, typeNameMatches);
        }

        private static void CheckType(TypeSyntax type, Type[] typeMatches, string[] typeNameMatches)
        {
            int i = 0;
            foreach (var syntaxNode in type.DescendantNodesAndSelf())
            {
                Assert.AreEqual(typeMatches[i], syntaxNode.GetType());
                Assert.AreEqual(typeNameMatches[i], syntaxNode.ToStringWithoutTrivia());
                i++;
            }
        }
    }
}