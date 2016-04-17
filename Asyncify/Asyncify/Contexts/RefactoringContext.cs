namespace Asyncify.Contexts
{
    class RefactoringContext<TDocContext, TSemContext> : IRefactoringContext<TDocContext, TSemContext>
        where TDocContext : DocumentContext
        where TSemContext : SemanticContext
    {

        public RefactoringContext(TDocContext documentContext, TSemContext semanticContext)
        {
            DocumentContext = documentContext;
            SemanticContext = semanticContext;
        }
        public TDocContext DocumentContext { get; set; }
        public TSemContext SemanticContext { get; set; }
    }

    interface IRefactoringContext<TDocContext, TSemContext>
        where TDocContext : DocumentContext
        where TSemContext : SemanticContext
    {
        TDocContext DocumentContext { get; }
        TSemContext SemanticContext { get; }
    }
}