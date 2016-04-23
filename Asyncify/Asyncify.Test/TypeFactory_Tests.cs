using System;
using System.Threading.Tasks;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
using QualifiedNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax;
using TypeArgumentListSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax;
using TypeSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax;

namespace Asyncify.Test
{
    [TestClass]
    public class TypeFactory_Tests
    {
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_type()
        {
            var typeName = "Blah";
            var typeMatches = new[] {typeof (IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Blah"};
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<IdentifierNameSyntax>(type, typeMatches, typeNameMatches);
        }

        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_namespaced_type()
        {
            var typeName = "Ns.Blah";
            var typeMatches = new[] { typeof(QualifiedNameSyntax), typeof (IdentifierNameSyntax), typeof(IdentifierNameSyntax)};
            var typeNameMatches = new[] { "Ns.Blah", "Ns", "Blah"};
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_generic_type()
        {
            var typeName = "Blah<int>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] {"Blah<int>", "<int>",
                "int" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_empty_generic_type()
        {
            var typeName = "Blah<>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(OmittedTypeArgumentSyntax)};
            var typeNameMatches = new[] {"Blah<>", "<>",
                "" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_empty_namespaced_generic_type()
        {
            var typeName = "Ns.Blah<>";
            var typeMatches = new[] {typeof (QualifiedNameSyntax), typeof (IdentifierNameSyntax), typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(OmittedTypeArgumentSyntax)};
            var typeNameMatches = new[] {"Ns.Blah<>", "Ns", "Blah<>", "<>",
                "" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_namespaced_generic_type()
        {
            var typeName = "Ns.Blah<int>";
            var typeMatches = new[] {typeof (QualifiedNameSyntax),typeof (IdentifierNameSyntax), typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] {"Ns.Blah<int>","Ns","Blah<int>", "<int>",
                "int" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_namespaced_generic_namespaced_inner_type()
        {
            var typeName = "Ns.Blah<Ns.Blah>";
            var typeMatches = new[] {typeof (QualifiedNameSyntax),typeof (IdentifierNameSyntax), typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(QualifiedNameSyntax), typeof(IdentifierNameSyntax), typeof(IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Ns.Blah<Ns.Blah>","Ns","Blah<Ns.Blah>", "<Ns.Blah>",
                "Ns.Blah", "Ns", "Blah"};
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_generic_namespaced_inner_type()
        {
            var typeName = "Blah<Ns.Blah>";
            var typeMatches = new[] {typeof (GenericNameSyntax),typeof (TypeArgumentListSyntax),
                typeof (QualifiedNameSyntax), typeof(IdentifierNameSyntax), typeof(IdentifierNameSyntax)};
            var typeNameMatches = new[] {"Blah<Ns.Blah>","<Ns.Blah>",
                "Ns.Blah", "Ns", "Blah"};
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_nested_generic_type()
        {
            var typeName = "Blah<Blah<int>>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] {"Blah<Blah<int>>", "<Blah<int>>",
                "Blah<int>", "<int>",
                "int" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_generic_multiple_type()
        {
            var typeName = "Blah<int,float>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] {"Blah<int,float>", "<int,float>",
                "int",
                "float" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_empty_generic_multiple_type()
        {
            var typeName = "Blah<,>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(OmittedTypeArgumentSyntax),
                typeof(OmittedTypeArgumentSyntax) };
            var typeNameMatches = new[] {"Blah<,>", "<,>",
                "",
                "" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_empty_namespaced_generic_multiple_type()
        {
            var typeName = "Ns.Blah<,>";
            var typeMatches = new[] {typeof (QualifiedNameSyntax), typeof (IdentifierNameSyntax), typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(OmittedTypeArgumentSyntax),
                typeof(OmittedTypeArgumentSyntax) };
            var typeNameMatches = new[] {"Ns.Blah<,>","Ns","Blah<,>", "<,>",
                "",
                "" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_nested_generic_multiple_type()
        {
            var typeName = "Blah<Blah<int,float>>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] { "Blah<Blah<int,float>>", "<Blah<int,float>>",
                "Blah<int,float>", "<int,float>",
                "int",
                "float" };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_nested_multiple_generic_multiple_type()
        {
            var typeName = "Blah<Blah<int, float>, Blah<int>, int>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax), typeof(PredefinedTypeSyntax), typeof(PredefinedTypeSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] { "Blah<Blah<int,float>,Blah<int>,int>", "<Blah<int,float>,Blah<int>,int>",
                "Blah<int,float>","<int,float>","int","float",
                "Blah<int>", "<int>", "int",
                "int",  };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_nested_multiple_namespaced_generic_multiple_type()
        {
            var typeName = "Ns.Blah<Blah<int, float>, Ns.Blah<int>, int>";
            var typeMatches = new[] {typeof (QualifiedNameSyntax),typeof (IdentifierNameSyntax),typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax), typeof(PredefinedTypeSyntax), typeof(PredefinedTypeSyntax),
                typeof(QualifiedNameSyntax),typeof(IdentifierNameSyntax),typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax),
                typeof(PredefinedTypeSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] { "Ns.Blah<Blah<int,float>,Ns.Blah<int>,int>", "Ns", "Blah<Blah<int,float>,Ns.Blah<int>,int>", "<Blah<int,float>,Ns.Blah<int>,int>",
                "Blah<int,float>","<int,float>","int","float",
                "Ns.Blah<int>", "Ns", "Blah<int>", "<int>", "int",
                "int",  };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<QualifiedNameSyntax>(type, typeMatches, typeNameMatches);
        }
        [TestMethod, TestCategory("TypeFactory")]
        public async Task Should_parse_nested_multiple_empty_generic_multiple_type()
        {
            var typeName = "Blah<Blah<,>, Blah<>, int>";
            var typeMatches = new[] {typeof (GenericNameSyntax), typeof(TypeArgumentListSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax), typeof(OmittedTypeArgumentSyntax), typeof(OmittedTypeArgumentSyntax),
                typeof(GenericNameSyntax),typeof(TypeArgumentListSyntax),
                typeof(OmittedTypeArgumentSyntax),
                typeof(PredefinedTypeSyntax)};
            var typeNameMatches = new[] { "Blah<Blah<,>,Blah<>,int>", "<Blah<,>,Blah<>,int>",
                "Blah<,>","<,>","","",
                "Blah<>", "<>", "",
                "int",  };
            var type = await TypeFactory.CreateTypeSyntax(typeName);
            CheckType<GenericNameSyntax>(type, typeMatches, typeNameMatches);
        }

        private static void CheckType<T>(TypeSyntax type, Type[] typeMatches, string[] typeNameMatches)
        {
            Assert.AreEqual(typeof(T), type.GetType());
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