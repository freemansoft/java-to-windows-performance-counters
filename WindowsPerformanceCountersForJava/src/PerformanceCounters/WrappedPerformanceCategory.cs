//-----------------------------------------------------------------------
// <copyright file="WrappedPerformanceCategory.cs" company="FreemanSoft">
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
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for the PerformanceCategory class
    /// </summary>
    internal class WrappedPerformanceCategory
    {
        /// <summary>
        /// used for debugging
        /// </summary>
        private string instanceName = null;

        /// <summary>
        /// wrapped counters that are tied to the wrapped category
        /// </summary>
        private ConcurrentDictionary<string, WrappedPerformanceCounter> counters = new ConcurrentDictionary<string, WrappedPerformanceCounter>();

        /// <summary>
        /// cached category wrapped in our special wrapper
        /// </summary>
        private PerformanceCounterCategory wrappedCategory;

        /// <summary>
        /// constructor for the wrapper
        /// </summary>
        /// <param name="categoryName">the category name this is bound too</param>
        /// <param name="instanceName">the optional instanceName name this is bound too in this category</param>
        internal WrappedPerformanceCategory(string categoryName, string instanceName = null)
        {
            this.instanceName = instanceName;
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentException("no category name specified");
            }
            this.wrappedCategory = new PerformanceCounterCategory(categoryName);
            PerformanceCounter[] myCategoryCounters;
            if (string.IsNullOrEmpty(instanceName))
            {
                myCategoryCounters = this.wrappedCategory.GetCounters();
            }
            else
            {
                myCategoryCounters = this.wrappedCategory.GetCounters(instanceName);
            }
            System.Diagnostics.Debug.WriteLine("WrappedPerformanceCategory: " + categoryName + " => " + myCategoryCounters.Length + " counters");
            PerformanceCounter previousCounter = null;
            for (int i = myCategoryCounters.Length - 1; i >= 0; i--)
            {
                PerformanceCounter readOnlyCounter = myCategoryCounters[i];
                PerformanceCounter counterInCategory;
                bool counterIsReadOnly = true;
                try
                {
                    //// this will fail for system counters
                    counterInCategory = new PerformanceCounter(
                        readOnlyCounter.CategoryName,
                        readOnlyCounter.CounterName,
                        readOnlyCounter.InstanceName,
                        false);
                    counterIsReadOnly = false;
                }
                catch (System.InvalidOperationException)
                {
                    counterInCategory = readOnlyCounter;
                    counterIsReadOnly = true;
                }
                //// turn on Debug-->Windows-->Output (debug output) to see this string
                System.Diagnostics.Debug.WriteLine("WrappedPerformanceCategory: " + categoryName + ": " + counterInCategory.CounterName + " -> " + counterInCategory.CounterType);
                PerformanceCounterType? matchingBase =
                    WrappedPerformanceCounter.GetBaseTypeForCounter(counterInCategory);
                //// bind together a counter and its base if the counter time requires a base counter type
                if (matchingBase == null)
                {
                    this.counters.TryAdd(
                        counterInCategory.CounterName,
                        new WrappedPerformanceCounter(counterInCategory, counterIsReadOnly));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        "WrappedPerformanceCategory: " +
                          "Table says " + counterInCategory.CounterType
                        + " is supported by " + matchingBase
                        + " and we're using type " + previousCounter.CounterType);
                    this.counters.TryAdd(
                        counterInCategory.CounterName,
                        new WrappedPerformanceCounter(counterInCategory, previousCounter, counterIsReadOnly));
                }
                previousCounter = counterInCategory;
            }
        }

        /// <summary>
        /// returns wrapped counters for this category
        /// </summary>
        /// <returns>counters keyed by counter name</returns>
        internal IDictionary<string, WrappedPerformanceCounter> GetCounters()
        {
            return this.counters;
        }

        /// <summary>
        /// Increment a counter in this category by 1
        /// <para></para>
        /// This method exists because the .Net 3.5 docs say they are significantly faster than incrementBy(1) <see cref="http://msdn.microsoft.com/en-us/library/wzdx16ez(v=vs.90).aspx"/>
        /// Testing shows a 10x difference
        /// <para></para>
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// </summary>
        /// <param name="counterName">the counter name</param>
        internal void Increment(string counterName)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            ourCounter.Increment();
        }

        /// <summary>
        /// increment a counter by a specific amount (and its possible base by 1)
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// </summary>
        /// <param name="counterName">name of the counter in this category</param>
        /// <param name="incrementAmount">the increment amount</param>
        internal void IncrementBy(string counterName, long incrementAmount)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            ourCounter.IncrementBy(incrementAmount);
        }

        /// <summary>
        /// increment a counter by a specific amount (and its possible base by 1)
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly or there is no associated base</exception>
        /// </summary>
        /// <param name="counterName">name of the counter in this category</param>
        /// <param name="incrementAmount">the increment amount</param>
        /// <param name="incrementBaseAmount">increment base counter amount</param>
        internal void IncrementBy(string counterName, long incrementAmount, long incrementBaseAmount)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            ourCounter.IncrementBy(incrementAmount, incrementBaseAmount);
        }

        /// <summary>
        /// Increment a counter in this category by 1
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// </summary>
        /// <param name="counterName">the counter name</param>
        internal void Decrement(string counterName)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            ourCounter.Decrement();
        }

        /// <summary>
        /// Increment a counter in this category by 1
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// </summary>
        /// <param name="counterName">the counter name</param>
        /// <returns>Returns the calculated value for the current sample</returns>
        internal float NextValue(string counterName)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            return ourCounter.NextValue();
        }

        /// <summary>
        /// returns the raw value property for this counter
        /// </summary>
        /// <param name="counterName">the counter name</param>
        /// <returns>Returns the raw value for the current sample</returns>
        internal long GetRawValue(string counterName)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            return ourCounter.GetRawValue();
        }

        /// <summary>
        /// sets the raw value name for this counter
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// </summary>
        /// <param name="counterName">the counter name</param>
        /// <param name="value">the new raw value</param>
        internal void SetRawValue(string counterName, long value)
        {
            this.ValidateCounterNameExists(counterName);
            WrappedPerformanceCounter ourCounter = this.counters[counterName];
            ourCounter.SetRawValue(value);
        }

        /// <summary>
        /// Internal utility method to remove duplicated code and reduce method complexity.
        /// <exception cref="ArgumentException">if a counter name is not provided or the counter does not exist</exception>
        /// </summary>
        /// <param name="counterName">name of the counter</param>
        private void ValidateCounterNameExists(string counterName)
        {
            if (string.IsNullOrEmpty(counterName))
            {
                throw new ArgumentException("Counter name was not provided");
            }
            else if (!this.counters.ContainsKey(counterName))
            {
                throw new ArgumentException("Counter " + counterName + "does not exist in category ");
            }
        }
    }
}
