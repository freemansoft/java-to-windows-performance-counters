// <copyright file="WindowsPerformanceFacadeTest.cs" company="FreemanSoft">
//
// Copyright 2014 FreemanSoft Inc
// Licensed under the Apache License, Version 2.0 (the "License");
//
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//-----------------------------------------------------------------------
// </copyright>
//-----------------------------------------------------------------------
namespace FreemanSoft.PerformanceCounters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Threading;
    using FreemanSoft.PerformanceCounters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// one of our unit tests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WindowsPerformanceFacadeTest
    {
        /// <summary>
        /// created by the unit test template
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        #region Additional test attributes
        /// <summary>
        /// create the counters we test with
        /// </summary>
        /// <param name="testContext">standard test context</param>
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            try
            {
                CounterTestUtilities.GenerateStandardTestCounters();
            }
            catch (System.Security.SecurityException e)
            {
                throw new InternalTestFailureException(
                    "Failed to create Category.  "
                    //// + "Run 'net localgroup \"Performance Log Users\" [username] /add' as administrator",
                    + "This fails unless you run Visual Studio as Administrator. ",
                    e);
            }
        }

        /// <summary>
        /// Remove our test counters
        /// </summary>
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            CounterTestUtilities.TeardownStandardTestCounters();
        }

        // .
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        // .
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        // .
        #endregion

        /// <summary>
        /// basic test
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyGetPerformanceCounterMatch()
        {
            WindowsPerformanceFacade liason = new WindowsPerformanceFacade();
            int returnedId = liason.GetPerformanceCounterId("a", "b", "c");
            PerformanceCounterKey foo = liason.GetPerformanceCounterKey(returnedId);
            Assert.AreEqual("a", foo.CategoryName);
            Assert.AreEqual("b", foo.InstanceName);
            Assert.AreEqual("c", foo.CounterName);
        }

        /// <summary>
        /// basic test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceFacadeTest_VerifyGetPerformanceCounterNotCreated()
        {
            WindowsPerformanceFacade liason = new WindowsPerformanceFacade();
            liason.GetPerformanceCounterKey(12);
        }

        /// <summary>
        /// basic test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceFacadeTest_VerifyFailsNoCategoryNameTest()
        {
            WindowsPerformanceFacade liason = new WindowsPerformanceFacade();
            liason.CacheCounters(string.Empty, null);
        }

        /// <summary>
        /// basic test looking for a non-existent category
        /// This test takes 500 milliseconds because that is how long it takes to pull a category from the hive
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void WindowsPerformanceFacadeTest_VerifyFailsNonExistentCategoryNameTest()
        {
            WindowsPerformanceFacade liason = new WindowsPerformanceFacade();
            int counterId = liason.GetPerformanceCounterId("dogfood", null, "catfood");
            liason.Increment(counterId);
        }

        /// <summary>
        /// basic test
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyWeCanSeeWellKnownCategoryTest()
        {
            WindowsPerformanceFacade liason = new WindowsPerformanceFacade();
            int counterId = liason.GetPerformanceCounterId("cache", null, "Dirty Pages");
            float value = liason.NextValue(counterId);
            Assert.AreNotEqual(0.0f, value, 0.1f, "dirty pages should not be 0.0");
        }

        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test basic counters because then we don't have rate window problems.
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyIncrementSimple64()
        {
            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestCounterNumberOfItems64Name);
            float initialValue = underTest.NextValue(counterId);
            underTest.Increment(counterId);
            float finalValue = underTest.NextValue(counterId);
            Assert.AreEqual(initialValue + 1, finalValue);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test a timer counter with base counter
        /// This isn't a very useful call because we probably wouldn't call a timer incrementing by 1
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyIncrementAverageTimer32()
        {
            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestAverageTimer32Name);
            underTest.Increment(counterId);
            underTest.Increment(counterId);
            float finalValue = underTest.NextValue(counterId);
            //// ((N1 - N0) / F) / (B1 - B0)
            //// how fast is our clock
            float freq = Stopwatch.Frequency;
            float numerator = ((float)(2 - 1)) / freq;
            float denominator = 2 - 1;
            Assert.AreEqual(numerator / denominator, finalValue, .002, "Freq: " + freq);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test basic counters because then we don't have rate window problems.
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyIncrementBySimple64()
        {
            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestCounterNumberOfItems64Name);
            float initialValue = underTest.NextValue(counterId);
            underTest.IncrementBy(counterId, 3);
            float finalValue = underTest.NextValue(counterId);
            Assert.AreEqual(initialValue + 3, finalValue);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test a timer counter with base counter
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyIncrementByAverageTimer32()
        {
            //// The spans need to be ever increasing number like if you used a global stopwatch
            long span1 = 1000;
            long span2 = 2000;

            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestAverageTimer32Name);
            //// average (time) of all measurements performed.  lets assume it was clear before we ran the test
            underTest.IncrementBy(counterId, span1);
            underTest.IncrementBy(counterId, span2);
            //// average (time) of all measurements performed. 
            float finalValue = underTest.NextValue(counterId);
            //// ((N1 - N0) / F) / (B1 - B0)
            //// how fast is our clock
            float freq = Stopwatch.Frequency;
            float numerator = ((float)(span2 - span1)) / freq;
            float denominator = 2 - 1;
            Assert.AreEqual(numerator / denominator, finalValue, .002, "Freq: " + freq);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test a timer counter with base counter
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyIncrementByWithBaseAverageTimer32()
        {
            //// The spans need to be ever increasing number like if you used a global stopwatch
            long span1 = 1000;
            long span2 = 2000;

            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestAverageTimer32Name);
            //// average (time) of all measurements performed.  lets assume it was clear before we ran the test
            //// increment the base counters by something other than the default
            underTest.IncrementBy(counterId, span1, 4);
            underTest.IncrementBy(counterId, span2, 4);
            //// average (time) of all measurements performed. 
            float finalValue = underTest.NextValue(counterId);
            //// ((N1 - N0) / F) / (B1 - B0)
            //// how fast is our clock
            float freq = Stopwatch.Frequency;
            float numerator = ((float)(span2 - span1)) / freq;
            float denominator = 8 - 4;
            Assert.AreEqual(numerator / denominator, finalValue, .002, "Freq: " + freq);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test basic counters because then we don't have rate window problems.
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyDecrementSimple64()
        {
            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestCounterNumberOfItems64Name);
            float initalValue = underTest.NextValue(counterId);
            underTest.IncrementBy(counterId, 7);
            underTest.Decrement(counterId);
            float finalValue = underTest.NextValue(counterId);
            Assert.AreEqual(initalValue + 6, finalValue);
        }

        /// <summary>
        /// Verify we can write to all known test counters
        /// Test a timer counter with base counter
        /// This isn't a very useful call because we probably wouldn't call a timer decrementing by anything 
        /// </summary>
        [TestMethod]
        public void WindowsPerformanceFacadeTest_VerifyDecrementAverageTimer32()
        {
            WindowsPerformanceFacade underTest = new WindowsPerformanceFacade();
            int counterId = underTest.GetPerformanceCounterId(CounterTestUtilities.TestCategoryName, null, CounterTestUtilities.TestAverageTimer32Name);
            underTest.Increment(counterId);
            underTest.Increment(counterId);
            underTest.Decrement(counterId);
            float finalValue = underTest.NextValue(counterId);
            //// ((N1 - N0) / F) / (B1 - B0)
            //// how fast is our clock
            float freq = Stopwatch.Frequency;
            //// ((N1 - N0) / F)
            float numerator = (1 - 2) / freq;
            float denominator = 3 - 2;
            Assert.AreEqual(numerator / denominator, finalValue, .002, "Freq: " + freq);
            //// the final value is actually 0 here and our calculatd value is very small, almost -0
        }
    }
}
