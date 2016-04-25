using System;
using System.Linq;
using Asyncify.Test.Extensions;
using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    /// <summary>
    /// CodeUnit for a piece of code that has format references
    /// </summary>
    public class WrapperCodeUnit : SourceCodeUnit
    {
        public ResultLocation[] WrapperLocations { get; }

        public WrapperCodeUnit(string wrapperCode) : base(wrapperCode)
        {
            WrapperLocations = CreateWrapperLocations(wrapperCode);
        }

        public string WrapCode(params string[] expressions)
        {
            return String.Format(Code, expressions.Cast<object>().ToArray());
        }
        public string WrapCode(params SourceCodeUnit[] expressions)
        {
            return String.Format(Code, expressions.Select(s => s.Code).Cast<object>().ToArray());
        }
        public MergedCodeUnit MergeCode(params string[] expressions)
        {
            return new MergedCodeUnit(this, expressions.Select(s => new SourceCodeUnit(s)).ToArray());
        }
        public MergedCodeUnit MergeCode(params SourceCodeUnit[] expressions)
        {
            return new MergedCodeUnit(this, expressions);
        }
        
        static ResultLocation[] CreateWrapperLocations(string wrapperExpression)
        {
            var numberReferences = wrapperExpression.RequiredFormatReferences();
            if (numberReferences == 0)
                throw new ArgumentException($"{nameof(wrapperExpression)} has no format reference, it should not be a WrapperCodeUnit.");

            var guid = Guid.NewGuid();
            var markerObjects = new object[numberReferences];
            for (int i = 0; i < markerObjects.Length; i++)
            {
                markerObjects[i] = guid.ToString();
            }

            var filledWrapper = String.Format(wrapperExpression, markerObjects);
            var results = filledWrapper.FindSourceLocations(guid.ToString());
            return results.Select(result => new ResultLocation(result.Line, result.Column, result.Span.Start, 0)).ToArray();

        }


        public override string ToString()
        {
            return Code;
        }
    }
}