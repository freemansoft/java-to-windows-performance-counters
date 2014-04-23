// <copyright file="WrappedPerformanceCategorySpecificNamesTest.cs" company="FreemanSoft">
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
    public class WrappedPerformanceCategorySpecificNamesTest
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
            //// do not create standard test counters for this test.  
            //// We are testing existing standard counters
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
        /// Working up from the bottom testing "Paging File(_Total)\% Usage"
        /// </summary>
        [TestMethod]
        public void WrappedPerformanceCategorySpecificNamesTest_VerifyPagingFileCounters()
        {
            WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
            WrappedPerformanceCategory ourCategory = liason.CacheCountersForCategory(
                "Paging File", "_Total");
            IDictionary<string, WrappedPerformanceCounter> allCounters = ourCategory.GetCounters();
            WrappedPerformanceCounter ourTargetCounter = allCounters["% Usage"];
            Assert.IsNotNull(ourTargetCounter);
            Assert.IsTrue(ourTargetCounter.CounterIsReadOnly());
            Assert.IsFalse(ourTargetCounter.CounterHasAssociatedBase());
            Assert.IsNotNull(ourCategory.NextValue("% Usage"));
        }
    }
}
