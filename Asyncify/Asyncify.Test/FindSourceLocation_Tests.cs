using System;
using Asyncify.Test.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Asyncify.Test
{
    [TestClass]
    public class FindSourceLocation_Tests
    {
        [TestMethod, TestCategory("TestHelpers")]
        public void Should_find_location_for_single_line_output()
        {
            var testExpression = @"a b
c d
e f";

            var newlineLength = Environment.NewLine.ToCharArray().Length;
            var find = new[] {"a", "b", "c", "d", "e", "f", };
            var answers = new[]
            {
                new ResultLocation(1, 1, (newlineLength*0) + 0, 1),
                new ResultLocation(1, 3, (newlineLength*0) + 2, 1),
                new ResultLocation(2, 1, (newlineLength*1) + 3, 1),
                new ResultLocation(2, 3, (newlineLength*1) + 5, 1),
                new ResultLocation(3, 1, (newlineLength*2) + 6, 1),
                new ResultLocation(3, 3, (newlineLength*2) + 8, 1),
            };
            for (int i = 0; i < find.Length; i++)
            {
                var result = testExpression.FindSourceLocation(find[i]);
                Assert.AreEqual(answers[i], result);
            }
        }
        [TestMethod, TestCategory("TestHelpers")]
        public void Should_find_location_for_source_index_output()
        {
            var testExpression = @"a b
c d
e f";

            var newlineLength = Environment.NewLine.ToCharArray().Length;
            var answers = new[]
            {
                new ResultLocation(1, 1, (newlineLength*0) + 0, 0),
                new ResultLocation(1, 3, (newlineLength*0) + 2, 0),
                new ResultLocation(2, 1, (newlineLength*1) + 3, 0),
                new ResultLocation(2, 3, (newlineLength*1) + 5, 0),
                new ResultLocation(3, 1, (newlineLength*2) + 6, 0),
                new ResultLocation(3, 3, (newlineLength*2) + 8, 0),
            };
            for (int i = 0; i < answers.Length; i++)
            {
                var result = testExpression.FindSourceLocation(answers[i].Span.Start);
                Assert.AreEqual(answers[i], result);
            }
        }
        [TestMethod, TestCategory("TestHelpers")]
        public void Should_find_location_for_wrapped_line_output()
        {
            var testExpression = @"a b
c d
e f";
            var wrapperExpression = @"
using
{{
{0}
}}";
            var wrapperMarker = Guid.NewGuid().ToString();
            var markerWrapper = String.Format(wrapperExpression, wrapperMarker);
            var wrapperOffset =
                markerWrapper.FindSourceLocation(markerWrapper.FindSourceLocation(wrapperMarker).Span.Start);

            var newlineLength = Environment.NewLine.ToCharArray().Length;
            Assert.AreEqual(new ResultLocation(4, 1, (newlineLength*3)+6, 0), wrapperOffset);

            var filledWrapper = String.Format(wrapperExpression, testExpression);
            var find = new[] {"a", "b", "c", "d", "e", "f", };
            var answers = new[]
            {
                wrapperOffset.Add(new ResultLocation(1, 1, (newlineLength*0) + 0, 1)),
                wrapperOffset.Add(new ResultLocation(1, 3, (newlineLength*0) + 2, 1)),
                wrapperOffset.Add(new ResultLocation(2, 1, (newlineLength*1) + 3, 1)),
                wrapperOffset.Add(new ResultLocation(2, 3, (newlineLength*1) + 5, 1)),
                wrapperOffset.Add(new ResultLocation(3, 1, (newlineLength*2) + 6, 1)),
                wrapperOffset.Add(new ResultLocation(3, 3, (newlineLength*2) + 8, 1)),
            };
            for (int i = 0; i < find.Length; i++)
            {
                var result = filledWrapper.FindSourceLocation(find[i]);
                Assert.AreEqual(answers[i], result);
            }
        }

        [TestMethod, TestCategory("TestHelpers")]
        public void Should_find_location_for_multi_line_output()
        {
            var testExpression = @"a
b
c";

            var newlineLength = Environment.NewLine.ToCharArray().Length;
            var find = new[] {$"a{Environment.NewLine}b",$"b{Environment.NewLine}c",};
            var answers = new[]
            {
                new ResultLocation(1, 1, (newlineLength*0) + 0, (newlineLength*1)+2),
                new ResultLocation(2, 1, (newlineLength*1) + 1, (newlineLength*1)+2),
            };
            for (int i = 0; i < find.Length; i++)
            {
                var result = testExpression.FindSourceLocation(find[i]);
                Assert.AreEqual(answers[i], result);
            }
        }

        [TestMethod, TestCategory("TestHelpers")]
        public void Should_get_correct_line()
        {
            var testExpression = @"a
b
c";
            
            var find = new[] {1, 2, 3};
            var answers = new[]
            {
                "a", "b", "c"
            };
            for (int i = 0; i < find.Length; i++)
            {
                var result = testExpression.GetLine(find[i]);
                Assert.AreEqual(answers[i], result);
            }
        }
        [TestMethod, TestCategory("TestHelpers")]
        public void Should_throw_for_out_of_range_line()
        {
            var testExpression = @"a
b
c";
            
            Throws<ArgumentOutOfRangeException>(() => testExpression.GetLine(4));
            Throws<ArgumentOutOfRangeException>(() => testExpression.GetLine(-1));
            Throws<ArgumentOutOfRangeException>(() => testExpression.GetLine(0));
        }

        void Throws<T>(Action func)
            where T : Exception
        {
            try
            {
                func();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(T), e.GetType());
            }
        }
    }
}