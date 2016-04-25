using System;
using System.Collections.Generic;
using Asyncify.Test.Extensions;
using TestHelper;

namespace Asyncify.Test.Helpers.Code
{
    public class MergedCodeUnit : SourceCodeUnit
    {
        public WrapperCodeUnit WrapperCode { get; }
        public SourceCodeUnit[] EmbeddedCode { get; }

        public MergedCodeUnit(WrapperCodeUnit wrapperCode, SourceCodeUnit[] embeddedCode) : base(wrapperCode.WrapCode(embeddedCode))
        {
            WrapperCode = wrapperCode;
            EmbeddedCode = embeddedCode;
            if (wrapperCode.WrapperLocations.Length != embeddedCode.Length)
                throw new ArgumentException($"{nameof(embeddedCode)} Length was {embeddedCode.Length}, expected {nameof(wrapperCode.WrapperLocations)}'s Length which is {wrapperCode.WrapperLocations.Length}");
        }

        public ResultLocation[] FindAbsoluteSourceLocations(int referencePosition, string searchCode)
        {
            var referenceCode = EmbeddedCode[referencePosition];
            var relativeLocations = referenceCode.Code.FindSourceLocations(searchCode);

            int proceedingLinesAccum = 0;
            for (int i = 0; i < referencePosition; i++)
            {
                proceedingLinesAccum += EmbeddedCode[i].Code.FindNewLineEndingLocations().Count;
            }
            var referenceLocation = WrapperCode.WrapperLocations[referencePosition].AddLines(proceedingLinesAccum);

            List<ResultLocation> absoluteLocations = new List<ResultLocation>();
            foreach (var relativeLocation in relativeLocations)
            {
                absoluteLocations.Add(referenceLocation.Add(relativeLocation));
            }
            return absoluteLocations.ToArray();
        }

        public override string ToString()
        {
            return WrapperCode.WrapCode(EmbeddedCode);
        }
    }
}