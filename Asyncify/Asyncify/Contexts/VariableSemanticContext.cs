using System;
using System.Threading.Tasks;
using Asyncify.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Asyncify.Contexts
{
    class VariableSemanticContext : SemanticContext, IVariableSemanticContext
    {

        public VariableSemanticContext(TypeSyntax variableTypeDeclaration, IdentifierNameSyntax variableNameDeclaration, SemanticModel model) : base(model)
        {
            if (variableTypeDeclaration == null) throw new ArgumentNullException(nameof(variableTypeDeclaration));
            if (variableNameDeclaration == null) throw new ArgumentNullException(nameof(variableNameDeclaration));
            VariableTypeDeclaration = variableTypeDeclaration;
            VariableNameDeclaration = variableNameDeclaration;
        }
        /// <summary>
        /// Declaration of the variable type, e.g. var or Task&lt;int&gt;
        /// </summary>
        public TypeSyntax VariableTypeDeclaration { get; set; }
        /// <summary>
        /// Declaration of the variable name
        /// </summary>
        public IdentifierNameSyntax VariableNameDeclaration { get; set; }

        /// <summary>
        /// Creates a VariableSemanticContext for a new local variable.
        /// </summary>
        /// <param name="expressionLocation">Location of the expression where the local variable will be inserted</param>
        /// <param name="variableType">Type symbol to use for type declaration</param>
        /// <param name="variableName">Variable name that serves as a prefix if there are conflicts</param>
        /// <param name="semanticModel">Semantic model of the expressionLocation tree</param>
        /// <returns></returns>
        public static async Task<VariableSemanticContext> CreateForLocalVariable(ExpressionSyntax expressionLocation, ITypeSymbol variableType, string variableName, SemanticModel semanticModel)
        {
            var defaultVarName = expressionLocation.GenerateDefaultUnusedLocalVariableName(variableName, semanticModel);
            
            var minimalTypeString = variableType.ToMinimalDisplayString(semanticModel,
                expressionLocation.SpanStart);
            var varTypeDeclaration = await TypeFactory.CreateTypeSyntax(minimalTypeString);
            var varNameDeclaration = SyntaxFactory.IdentifierName(defaultVarName);
            return new VariableSemanticContext(varTypeDeclaration, varNameDeclaration, semanticModel);
        }
    }

    interface IVariableSemanticContext : ISemanticContext
    {
        TypeSyntax VariableTypeDeclaration { get; }
        IdentifierNameSyntax VariableNameDeclaration { get; }
    }
}