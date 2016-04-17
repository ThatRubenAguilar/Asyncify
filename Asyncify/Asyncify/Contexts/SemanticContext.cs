using System;
using Microsoft.CodeAnalysis;

namespace Asyncify.Contexts
{
    class SemanticContext : ISemanticContext
    {
        public SemanticContext(SemanticModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Model = model;
        }

        public SemanticModel Model { get; set; }
    }

    interface ISemanticContext
    {
        SemanticModel Model { get; }
    }
}