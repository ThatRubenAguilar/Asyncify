using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Asyncify.Extensions
{
    public static class SemanticExtensions
    {
        /// <summary>
        /// Gets a symbol or null if it cannot be cast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="node"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static T GetSymbol<T>(this SemanticModel model, SyntaxNode node, CancellationToken token)
            where T : class, ISymbol
        {
            var symbolInfo = model.GetSymbolInfo(node, token);
            return symbolInfo.Symbol as T;
        }
        /// <summary>
        /// Gets a declared symbol or null if it cannot be cast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="node"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static T GetDeclaredSymbol<T>(this SemanticModel model, SyntaxNode node, CancellationToken token)
            where T : class, ISymbol
        {
            return model.GetDeclaredSymbol(node, token) as T;
        }
    }
}