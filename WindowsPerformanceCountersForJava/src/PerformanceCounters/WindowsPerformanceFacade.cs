//-----------------------------------------------------------------------
// <copyright file="WindowsPerformanceFacade.cs" company="FreemanSoft">
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
namespace FreemanSoft.PerformanceCounters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// An integer based Windows Performance wrapper that provides better java marshling performance.
    /// This has a contains relationship with WindowsPerformanceLiason.
    /// <para></para>
    /// First GetCacheCounterKey() to get a counter key you can pass to the counter manipulation methods.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "I wish it new liason.")]
    public class WindowsPerformanceFacade
    {
        /// <summary>
        /// used to map from an integer key back to the Performance counter names used by WindowsPerformanceLiason
        /// </summary>
        private static ConcurrentDictionary<int, PerformanceCounterKey> keyToNames = new ConcurrentDictionary<int, PerformanceCounterKey>();

        /// <summary>
        /// our "contains-a" object
        /// </summary>
        private WindowsPerformanceLiason liason;

        /// <summary>
        /// Simple constructor that initilizes contains relationship
        /// </summary>
        public WindowsPerformanceFacade()
        {
            this.liason = new WindowsPerformanceLiason();
        }

        /// <summary>
        /// Counter manipulation method that takes pre-cached numeric key.
        /// This is many times faster than incrementBy(1)
        /// <exception cref="ArgumentException">if previously cached can't be found </exception>
        /// </summary>
        /// <param name="counterKey">numeric key that we map to category/instance/counter names </param>
        public void Increment(int counterKey)
        {
            PerformanceCounterKey complexKey = this.GetPerformanceCounterKey(counterKey);
            this.liason.Increment(complexKey.CategoryName, complexKey.InstanceName, complexKey.CounterName);
        }

        /// <summary>
        /// Counter manipulation method that takes pre-cached numeric key.
        /// <exception cref="ArgumentException">if previously cached can't be found </exception>
        /// </summary>
        /// <param name="counterKey">numeric key that we map to category/instance/counter names </param>
        /// <param name="incrementAmount">the amount we wish to increment</param>
        public void IncrementBy(int counterKey, long incrementAmount)
        {
            PerformanceCounterKey complexKey = this.GetPerformanceCounterKey(counterKey);
            System.Diagnostics.Debug.WriteLine("WindowsPerformanceFacade IncrementBy: '" + complexKey.CategoryName + ", " + complexKey.InstanceName + ", " + complexKey.CounterName + ", " + incrementAmount);
            this.liason.IncrementBy(complexKey.CategoryName, complexKey.InstanceName, complexKey.CounterName, incrementAmount);
        }

        /// <summary>
        /// Increments the counter by some specified amount (and its possible base by 1)
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly or there is no associated base</exception>
        /// <param name="counterKey">numeric key that we map to category/instance/counter names </param>
        /// <param name="incrementAmount">increment amount</param>
        /// <param name="incrementBaseAmount">increment base counter amount</param>
        public void IncrementBy(int counterKey, long incrementAmount, long incrementBaseAmount)
        {
            PerformanceCounterKey complexKey = this.GetPerformanceCounterKey(counterKey);
            this.liason.IncrementBy(complexKey.CategoryName, complexKey.InstanceName, complexKey.CounterName, incrementAmount, incrementBaseAmount);
        }

        /// <summary>
        /// Counter manipulation method that takes pre-cached numeric key.
        /// <exception cref="ArgumentException">if previously cached can't be found </exception>
        /// </summary>
        /// <param name="counterKey">numeric key that we map to category/instance/counter names </param>
        public void Decrement(int counterKey)
        {
            PerformanceCounterKey complexKey = this.GetPerformanceCounterKey(counterKey);
            this.liason.Decrement(complexKey.CategoryName, complexKey.InstanceName, complexKey.CounterName);
        }

        /// <summary>
        /// Counter manipulation method that takes pre-cached numeric key.
        /// <exception cref="ArgumentException">if previously cached can't be found </exception>
        /// </summary>
        /// <param name="counterKey">numeric key that we map to category/instance/counter names </param>
        /// <returns>The current value from the NextValue method on the counter.</returns>
        public float NextValue(int counterKey)
        {
            PerformanceCounterKey complexKey = this.GetPerformanceCounterKey(counterKey);
            return this.liason.NextValue(complexKey.CategoryName, complexKey.InstanceName, complexKey.CounterName);
        }

        /// <summary>
        /// caches the performance counters for a given category name.  Only caches one time no matter how often it is called
        /// <para></para>
        /// Used by external code to "warm up" the counters since they take so long to initialize
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if a category or category/instance does not exist</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">optional instance name in category</param>
        public void CacheCounters(string categoryName, string instanceName = null)
        {
            this.liason.CacheCounters(categoryName, instanceName);
        }

        /// <summary>
        /// returns a numeric key for this counter path
        /// <exception cref="ArgumentException">if a categoryName or counterName is not provided</exception>
        /// </summary>
        /// <param name="categoryName">Performance category name</param>
        /// <param name="instanceName">Performance category instance name (optional)</param>
        /// <param name="counterName">Performance counter name</param>
        /// <returns>numer identifier that can be used as an id/key later</returns>
        public int GetPerformanceCounterId(string categoryName, string instanceName, string counterName)
        {
            PerformanceCounterKey key = new PerformanceCounterKey(categoryName, instanceName, counterName);
            if (!keyToNames.ContainsKey(key.KeyCode))
            {
                keyToNames.TryAdd(key.KeyCode, key);
            }
            return key.KeyCode;
        }

        /// <summary>
        /// revers lookup that finds previously cached complex key object associated with this numeric
        /// <exception cref="ArgumentException">if previously cached can't be found </exception>
        /// </summary>
        /// <param name="counterKey">a previously retrieved performance counter id</param>
        /// <returns>The compound PerformanceCounterKey object that contains the category, instance and counter names</returns>
        internal PerformanceCounterKey GetPerformanceCounterKey(int counterKey)
        {
            if (!keyToNames.ContainsKey(counterKey))
            {
                throw new ArgumentException("Performance Counter for numeric key is not registered " + counterKey);
            }
            else
            {
                return keyToNames[counterKey];
            }
        }
    }
}
