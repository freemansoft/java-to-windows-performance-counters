// <copyright file="WindowsPerformanceLiasonBoundsTest.cs" company="FreemanSoft">
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
    public class WindowsPerformanceLiasonBoundsTest
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
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyIncrementEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.Increment(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyIncrementEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.Increment(CounterTestUtilities.TestCategoryName, string.Empty);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyIncrementByEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.IncrementBy(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name, 3);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyIncrementByEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.IncrementBy(CounterTestUtilities.TestCategoryName, string.Empty, 3);
        }

        /// <summary>
        /// Test incrementBy(long,long) with counter with no base
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyIncrementByCounterWithoutBase()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.IncrementBy(CounterTestUtilities.TestCategoryName, CounterTestUtilities.TestCounterNumberOfItems64Name, 3, 3);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyDecrementEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.Decrement(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyDecrementEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.Decrement(CounterTestUtilities.TestCategoryName, string.Empty);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyNextValueEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.NextValue(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name);
        }

        /// <summary>
        /// Just verifying we blow up with an empty counter name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyNextValueEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.NextValue(CounterTestUtilities.TestCategoryName, string.Empty);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifySetRawValueEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.SetRawValue(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name, 0);
        }

        /// <summary>
        /// Just verifying we blow up with an empty counter name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifySetRawValueEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.SetRawValue(CounterTestUtilities.TestCategoryName, string.Empty, 0);
        }

        /// <summary>
        /// Test empty argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyGetRawValueEmptyCategoryCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.GetRawValue(string.Empty, CounterTestUtilities.TestCounterNumberOfItems64Name);
        }

        /// <summary>
        /// Just verifying we blow up with an empty counter name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WindowsPerformanceLiasonBoundsTest_VerifyGetRawValueEmptyCounterCheck()
        {
            WindowsPerformanceLiason underTest = new WindowsPerformanceLiason();
            underTest.GetRawValue(CounterTestUtilities.TestCategoryName, string.Empty);
        }
    }
}
