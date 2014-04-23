// <copyright file="WrappedPerformanceCategoryTest.cs" company="FreemanSoft">
//     Copyright FreemanSoft Inc. This will be moved to an opensource license in the future
// </copyright>
//-----------------------------------------------------------------------
namespace FreemanSoft.PerformanceCounters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using FreemanSoft.PerformanceCounters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test WrappedPerformanceCategory not already covered by WindowsPerformanceLiasonTest.
    /// This is used to test edge conditions and input validation.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Missing word in Dictionary.")]
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WrappedPerformanceCategoryTest
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
        /// Verify the constructor checks for a category name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyEmptyCategoryConstructorCheck()
        {
            new WrappedPerformanceCategory(null);
        }

        /// <summary>
        /// Verify we can get a well know category with a well known instance name.
        /// This test takes 500ms because that is how long it takes to pull a category from the hive
        /// </summary>
        [TestMethod]
        public void WrappedPerformanceCategoryTest_VerifyCanWrapSystemInstances()
        {
            WrappedPerformanceCategory procInfo = new WrappedPerformanceCategory("Processor", "0");
            Assert.IsNotNull(procInfo);
            //// there are a bunch of proc 0 counters. We just want to make sure we got some
            IDictionary<string, WrappedPerformanceCounter> allCounters = procInfo.GetCounters();
            Assert.IsTrue(allCounters.Count > 2);
            Assert.IsTrue(allCounters["% User Time"].CounterIsReadOnly());
            //// this counter has a base but we don't see it since it is a system counter
            Assert.IsFalse(allCounters["% User Time"].CounterHasAssociatedBase());
        }

        /// <summary>
        /// Verify we can get a well know category with a well known instance name.
        /// This test takes 500ms because that is how long it takes to pull a category from the hive
        /// </summary>
        [TestMethod]
        public void WrappedPerformanceCategoryTest_VerifyCanWrapOurInstances()
        {
            WrappedPerformanceCategory testCategory = new WrappedPerformanceCategory(CounterTestUtilities.TestCategoryName, null);
            Assert.IsNotNull(testCategory);
            //// there are a bunch of proc 0 counters. We just want to make sure we got some
            IDictionary<string, WrappedPerformanceCounter> allCounters = testCategory.GetCounters();
            Assert.IsFalse(allCounters[CounterTestUtilities.TestCounterNumberOfItems64Name].CounterIsReadOnly());
            Assert.IsFalse(allCounters[CounterTestUtilities.TestCounterNumberOfItems64Name].CounterHasAssociatedBase());
            Assert.IsTrue(allCounters[CounterTestUtilities.TestAverageTimer32Name].CounterHasAssociatedBase());
        }

        /// <summary>
        /// verify increment checks for empty counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyIncrementEmptyCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.Increment(string.Empty);
        }

        /// <summary>
        /// verify increment checks for invalid counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyIncrementInvalidCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.Increment("some non existent counter name");
        }

        /// <summary>
        /// verify increment checks for empty counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyIncrementByEmptyCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.IncrementBy(string.Empty, 10);
        }

        /// <summary>
        /// verify increment checks for invalid counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyIncrementByInvalidCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.IncrementBy("some non existent counter name", 10);
        }

        /// <summary>
        /// verify decrement checks for empty counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyDecrementEmptyCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.Decrement(string.Empty);
        }

        /// <summary>
        /// verify decrement checks for invalid counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_DecrementVerifyInvalidCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.Decrement("some non existent counter name");
        }

        /// <summary>
        /// verify NextValue checks for empty counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_VerifyNextValueEmptyCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.NextValue(string.Empty);
        }

        /// <summary>
        /// verify NextValue checks for invalid counter names
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrappedPerformanceCategoryTest_NextValueVerifyInvalidCounterNameCheck()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(CounterTestUtilities.TestCategoryName, null);
            ourCategory.NextValue("some non existent counter name");
        }
    }
}
