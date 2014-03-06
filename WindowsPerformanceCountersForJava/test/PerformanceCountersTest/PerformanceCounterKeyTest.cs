//-----------------------------------------------------------------------
// <copyright file="PerformanceCounterKeyTest.cs" company="FreemanSoft">
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
    /// Test class for PerformanceCounterKey
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PerformanceCounterKeyTest
    {
        /// <summary>
        /// verify category name is required
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PerformanceCounterKeyTest_VerifyNullCheckCategory()
        {
            new PerformanceCounterKey(null, "foo", "bar");
        }

        /// <summary>
        /// verify category name is required
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PerformanceCounterKeyTest_VerifyEmptyCheckCategoryAndNullEqualsEmpty()
        {
            new PerformanceCounterKey(string.Empty, "foo", "bar");
        }

        /// <summary>
        /// verify null and empty instance are allowed and and are equivalent
        /// </summary>
        [TestMethod]
        public void PerformanceCounterKeyTest_VerifyEmptyCheckCategory()
        {
            PerformanceCounterKey c1 = new PerformanceCounterKey("foo", null, "bar");
            PerformanceCounterKey c2 = new PerformanceCounterKey("foo", string.Empty, "bar");
            Assert.AreEqual(c1.KeyCode, c2.KeyCode);
        }

        /// <summary>
        /// verify counter name is required
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PerformanceCounterKeyTest_VerifyNullCheckCounter()
        {
            new PerformanceCounterKey("foo", "bar", null);
        }

        /// <summary>
        /// verify counter name is required
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PerformanceCounterKeyTest_VerifyEmptyCheckCounter()
        {
            new PerformanceCounterKey("foo", "bar", string.Empty);
        }

        /// <summary>
        /// verify null and empty instance are allowed and and are equivalent
        /// </summary>
        [TestMethod]
        public void PerformanceCounterKeyTest_KeyCodeBehavior()
        {
            PerformanceCounterKey c1 = new PerformanceCounterKey("foo", null, "bar");
            PerformanceCounterKey c2 = new PerformanceCounterKey("foo", null, "bar");
            PerformanceCounterKey c3 = new PerformanceCounterKey("foo", null, "bat");
            PerformanceCounterKey c4 = new PerformanceCounterKey("bat", null, "bar");
            Assert.AreEqual(c1.KeyCode, c2.KeyCode);
            Assert.AreNotEqual(c2.KeyCode, c3.KeyCode);
            Assert.AreNotEqual(c3.KeyCode, c4.KeyCode);
            Assert.AreNotEqual(c2.KeyCode, c4.KeyCode);
        }
    }
}