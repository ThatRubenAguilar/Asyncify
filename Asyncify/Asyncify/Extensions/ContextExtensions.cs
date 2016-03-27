using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


// Code originated from https://github.com/Wintellect/Wintellect.Analyzers/blob/master/Source/Wintellect.Analyzers/Wintellect.Analyzers/Extensions/ContextExtensions.cs
namespace Asyncify.Extensions
{
    /// <summary>
    /// Provides extensions for SyntaxNodeAnalysisContext and SymbolAnalysisContext
    /// </summary>
    public static class ContextExtensions
    {
        public static Boolean IsGeneratedOrNonUserCode(this SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsGeneratedOrNonUserCode())
            {
                return true;
            }

            return context.SemanticModel?.SyntaxTree.IsGeneratedOrNonUserCode() ?? false;
        }

        public static Boolean IsGeneratedOrNonUserCode(this SyntaxTree tree) => IsGeneratedCodeFilename(tree.FilePath);

        public static Boolean IsGeneratedOrNonUserCode(this SymbolAnalysisContext context)
        {
            // Check the symbol first as the attributes are more likely than the filename.
            if (context.Symbol.IsGeneratedOrNonUserCode())
            {
                return true;
            }

            // Loop through all places where this Symbol could be declared. This accounts for
            // partial classes and the like.
            for (Int32 i = 0; i < context.Symbol.DeclaringSyntaxReferences.Length; i++)
            {
                SyntaxReference currRef = context.Symbol.DeclaringSyntaxReferences[i];

                // Check the tree itself which hits the filename.
                if (currRef?.SyntaxTree.IsGeneratedOrNonUserCode() == true)
                {
                    return true;
                }
            }

            return false;
        }

        // This code is lifted from: 
        // https://github.com/dotnet/roslyn/blob/master/src/Workspaces/Core/Portable/GeneratedCodeRecognition/GeneratedCodeRecognitionServiceFactory.cs
        private static Boolean IsGeneratedCodeFilename(String fileName)
        {
            if (fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            String extension = Path.GetExtension(fileName);
            if (extension.Length != 0)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);

                if (fileName.EndsWith("AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".g.i", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
