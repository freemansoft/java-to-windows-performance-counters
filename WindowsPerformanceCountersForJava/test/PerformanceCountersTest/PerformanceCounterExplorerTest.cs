//-----------------------------------------------------------------------
// <copyright file="PerformanceCounterExplorerTest.cs" company="FreemanSoft">
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
    using FreemanSoft.PerformanceCounters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A test class
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PerformanceCounterExplorerTest
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
        /// verify increment is faster than incrementBy -- test shows 10x difference
        /// </summary>
        [TestMethod]
        [Ignore]
        public void PerformanceCounterExplorerTest_TimeIncrementVsIncrementBy()
        {
            WindowsPerformanceLiason updatableCounters = new WindowsPerformanceLiason();
            Stopwatch incrementWatcher = new Stopwatch();
            incrementWatcher.Start();
            for (int i = 0; i < 100000; i++)
            {
                updatableCounters.Increment(CounterTestUtilities.TestCategoryName, CounterTestUtilities.TestCounterNumberOfItems64Name);
            }
            incrementWatcher.Stop();

            Stopwatch incrementByWatcher = new Stopwatch();
            incrementByWatcher.Start();
            for (int i = 0; i < 100000; i++)
            {
                updatableCounters.IncrementBy(CounterTestUtilities.TestCategoryName, CounterTestUtilities.TestCounterNumberOfItems64Name, 1);
            }
            incrementByWatcher.Stop();
            Assert.AreEqual(incrementWatcher.ElapsedMilliseconds, incrementByWatcher.ElapsedMilliseconds, " expected time is the 'increment', actual is 'incrementBy'");
        }

        /// <summary>
        /// list all categories, not really a test
        /// This test can take a long time.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void PerformanceCounterExplorerTest_ListAllCategories()
        {
            this.ListCategories();
        }

        /// <summary>
        /// utility partially copied from the internet
        /// </summary>
        public void ListCategories()
        {
            PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();
            foreach (PerformanceCounterCategory oneCategory in categories)
            {
                if (oneCategory.CategoryName.Contains("Thread"))
                {
                    //// ignore -- I'm not really interested in thread perf counters
                }
                else
                {
                    this.testContextInstance.WriteLine("{0} [{1}]", oneCategory.CategoryName, oneCategory.CategoryType);
                    System.Diagnostics.Debug.WriteLine("{0} [{1}]", oneCategory.CategoryName, oneCategory.CategoryType);
                    this.ListCounters(oneCategory);
                }
            }
        }

        /// <summary>
        /// utility partially copied from the internet
        /// </summary>
        /// <param name="category"> a category object that we will inspect</param>
        public void ListCounters(PerformanceCounterCategory category)
        {
            string[] instanceNames = category.GetInstanceNames();

            if (instanceNames.Length > 0)
            {
                // MultiInstance categories
                foreach (string instanceName in instanceNames)
                {
                    this.ListInstances(category, instanceName);
                }
            }
            else
            {
                // SingleInstance categories
                this.ListInstances(category, string.Empty);
            }
        }

        /// <summary>
        /// utility partially copied from the internet
        /// </summary>
        /// <param name="category">the category we are looking at</param>
        /// <param name="instanceName">an optional instance name</param>
        private void ListInstances(PerformanceCounterCategory category, string instanceName)
        {
            this.testContextInstance.WriteLine("    {0}", instanceName);
            System.Diagnostics.Debug.WriteLine("    {0}", instanceName);
            try
            {
                PerformanceCounter[] counters = category.GetCounters(instanceName);

                foreach (PerformanceCounter counter in counters)
                {
                    this.testContextInstance.WriteLine("        {0} - {1}", counter.CounterName, counter.CounterType);
                    System.Diagnostics.Debug.WriteLine("        {0} - {1}", counter.CounterName, counter.CounterType);
                }
            }
            catch (System.InvalidOperationException e)
            {
                this.testContextInstance.WriteLine("        --- {0} ", e.Message);
                System.Diagnostics.Debug.WriteLine("        --- {0} ", e.Message);
            }
        }
    }
}
