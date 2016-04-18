using System.Threading;
using Microsoft.CodeAnalysis;

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
    }
}