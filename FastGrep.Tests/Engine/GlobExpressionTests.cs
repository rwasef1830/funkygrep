﻿using System.Collections.Generic;
using FastGrep.Engine;
using NUnit.Framework;

namespace FastGrep.Tests.Engine
{
    [TestFixture]
    public class GlobExpressionTests
    {
        [Test]
        [TestCaseSource("GetPositiveGlobTestCases")]
        public void IsMatch_Static_Positive(string input, string expression)
        {
            Assert.That(GlobExpression.IsMatch(input, expression));
        }

        [Test]
        [TestCaseSource("GetPositiveGlobTestCases")]
        public void IsMatch_Instance_Positive(string input, string expression)
        {
            Assert.That(new GlobExpression(expression).IsMatch(input));
        }

        [Test]
        [TestCaseSource("GetNegativeGlobTestCases")]
        public void IsMatch_Static_Negative(string input, string expression)
        {
            Assert.That(!GlobExpression.IsMatch(input, expression));
        }

        [Test]
        [TestCaseSource("GetNegativeGlobTestCases")]
        public void IsMatch_Instance_Negative(string input, string expression)
        {
            Assert.That(!new GlobExpression(expression).IsMatch(input));
        }

        // ReSharper disable UnusedMethodReturnValue.Local
        static IEnumerable<TestCaseData> GetPositiveGlobTestCases()
        {
            return new[]
            {
                new TestCaseData("test.txt", "*.txt"),
                new TestCaseData("test1.txt", "test?.txt"),
                new TestCaseData("Xtest.txt", "?test.txt"),
                new TestCaseData("test.exe", "test.*"),
                new TestCaseData("a", "a"),
                new TestCaseData("test1Abc.tot", "test1???.t?t")
            };
        }

        static IEnumerable<TestCaseData> GetNegativeGlobTestCases()
        {
            return new[]
            {
                new TestCaseData("test.txt", "*.txx"),
                new TestCaseData("test1.txt", "test_.txt"),
                new TestCaseData("Xtest.txt", "_test.txt"),
                new TestCaseData("test.exe", "test.?"),
                new TestCaseData("a", "ab"),
                new TestCaseData("test1Abc.tot", "test1??X.t?t")
            };
        }
        // ReSharper restore UnusedMethodReturnValue.Local
    }
}