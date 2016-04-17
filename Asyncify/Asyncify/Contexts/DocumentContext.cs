using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Asyncify.Contexts
{
    class DocumentContext : IDocumentContext
    {
        public DocumentContext(SyntaxNode root, Document document, CancellationToken token)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (document == null) throw new ArgumentNullException(nameof(document));

            Root = root;
            Document = document;
            Token = token;
        }

        public SyntaxNode Root { get; set; }
        public Document Document { get; set; }
        public CancellationToken Token { get; }
        public Workspace Workspace => Document.Project?.Solution?.Workspace;

        public Task<SemanticModel> GetSemanticModel()
        {
            return Document.GetSemanticModelAsync(Token);
        }

        public SyntaxEditor CreateSyntaxEditor()
        {
            return new SyntaxEditor(Root, Workspace);
        }
    }

    interface IDocumentContext
    {
        SyntaxNode Root { get; }
        Document Document { get; }
        Workspace Workspace { get; }
        CancellationToken Token { get; }
    }
}