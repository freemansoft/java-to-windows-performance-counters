// <copyright file="WrappedPerformanceCounterTest.cs" company="FreemanSoft">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using FreemanSoft.PerformanceCounters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test WrappedPerformanceCounter not already covered by WindowsPerformanceLiasonTest.
    /// This is used to test edge conditions and input validation.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Missing word in Dictionary.")]
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WrappedPerformanceCounterTest
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void WrappedPerformanceCounterTest_VerifyEmptyCategoryConstructorCheck()
        {
            new WrappedPerformanceCounter(null, true);
        }

        /// <summary>
        /// Verify we get nothing for a null value
        /// </summary>
        [TestMethod]
        public void WrappedPerformanceCounterTest_VerifyEmptyCounterToBeWrapped()
        {
            Assert.IsNull(WrappedPerformanceCounter.GetBaseTypeForCounter(null));
        }
    }
}
