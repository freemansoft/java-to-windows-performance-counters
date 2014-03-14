//-----------------------------------------------------------------------
// <copyright file="WindowsPerformanceLiason.cs" company="FreemanSoft">
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
    /// The primary entry point for java communication. You will MOST LIKELY want to warm up the system
    /// before running events through it.  This is because a first time fetch of a logging counter is VERY
    /// expensive , on the order of a few hundred milliseconds.  The easiest way to do this is to call
    /// CacheCounters() passing in category / instance names.  All the counters are for a category are fetched 
    /// when the category is first retrieved.
    /// <para></para>
    /// <see cref="http://msdn.microsoft.com/en-us/library/system.diagnostics.performancecountertype(v=vs.120).aspx">
    /// Counter types are listed in the microsoft documentation
    /// </see>
    /// <para></para>
    /// Counter formats in the hive or when listing them out
    /// <para></para>
    /// <code>
    /// \\computer\object(parent/instance#index)\counter
    /// \\computer\object(parent/instance)\counter
    /// \\computer\object(instance#index)\counter
    /// \\computer\object(instance)\counter
    /// \\computer\object\counter
    /// \object(parent/instance#index)\counter
    /// \object(parent/instance)\counter
    /// \object(instance#index)\counter
    /// \object(instance)\counter
    /// \object\counter
    /// </code>
    /// </summary>
    public class WindowsPerformanceLiason
    {
        /// <summary>
        /// a list of the categories that we cached from the performance counters API
        /// </summary>
        private static ConcurrentDictionary<string, WrappedPerformanceCategory> categories = new ConcurrentDictionary<string, WrappedPerformanceCategory>();

        /// <summary>
        /// static class constructor that does one time updates
        /// </summary>
        static WindowsPerformanceLiason()
        {
            //// you can see this if you select "Debug" in the Output window
            //// You can get "Debug" to show up in the drop list buy running at least one test in "debug" mode.
            System.Diagnostics.Debug.WriteLine("Stopwatch IsHighResolution returns: " + Stopwatch.IsHighResolution);
        }

        /// <summary>
        /// Returns the current high resolution tick counter. 
        /// Performance counters use this rather than system time because it is more accurate.
        /// <para></para>
        /// Often used to get the start or end ticks for specific counter.
        /// Call this at the start of an operation and at the end and pass the difference to your
        /// counters that need spans. 
        /// <para></para>
        /// Can be used to initialize counter via RawValue method.
        /// Can really only be used with IncrementBy() methods.
        /// </summary>
        /// <returns>current system Stopwatch ticks</returns>
        public long StopwatchTimestamp()
        {
            long ticks;
            ticks = Stopwatch.GetTimestamp();
            return ticks;
        }

        /// <summary>
        /// Increments the counter by 1 (and its possible base by 1).
        /// <para></para>
        /// This method exists because the .Net 3.5 docs say they are significantly faster than incrementBy(1) <see cref="http://msdn.microsoft.com/en-us/library/wzdx16ez(v=vs.90).aspx"/>
        /// Testing shows a 10x difference
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void Increment(string categoryName, string counterName)
        {
            this.Increment(categoryName, null, counterName);
        }

        /// <summary>
        /// Increments the counter by 1 (and its possible base by 1)
        /// <para></para>
        /// This method exists because the .Net 3.5 docs say they are significantly faster than incrementBy(1) <see cref="http://msdn.microsoft.com/en-us/library/wzdx16ez(v=vs.90).aspx"/>
        /// Testing shows a 10x difference
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance of this category, optional can be null</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void Increment(string categoryName, string instanceName, string counterName)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            ourCategory.Increment(counterName);
        }

        /// <summary>
        /// Increments the counter by some specified amount (and its possible base by 1)
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the associated counter</param>
        /// <param name="incrementAmount">increment amount</param>
        public void IncrementBy(string categoryName, string counterName, long incrementAmount)
        {
            this.IncrementBy(categoryName, null, counterName, incrementAmount);
        }

        /// <summary>
        /// Increments the counter by some specified amount (and its possible base by 1)
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance can be null</param>
        /// <param name="counterName">name of the associated counter</param>
        /// <param name="incrementAmount">increment amount</param>
        public void IncrementBy(string categoryName, string instanceName, string counterName, long incrementAmount)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            ourCategory.IncrementBy(counterName, incrementAmount);
        }

        /// <summary>
        /// Increments the counter by some specified amount 
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly or there is no associated base</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the associated counter</param>
        /// <param name="incrementAmount">increment amount</param>
        /// <param name="incrementBaseAmount">increment base counter amount</param>
        public void IncrementBy(string categoryName, string counterName, long incrementAmount, long incrementBaseAmount)
        {
            this.IncrementBy(categoryName, null, counterName, incrementAmount, incrementBaseAmount);
        }

        /// <summary>
        /// Increments the counter by some specified amount (and its possible base by 1)
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly or there is no associated base</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance can be null</param>
        /// <param name="counterName">name of the associated counter</param>
        /// <param name="incrementAmount">increment amount</param>
        /// <param name="incrementBaseAmount">increment base counter amount</param>
        public void IncrementBy(string categoryName, string instanceName, string counterName, long incrementAmount, long incrementBaseAmount)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            ourCategory.IncrementBy(counterName, incrementAmount, incrementBaseAmount);
        }

        /// <summary>
        /// Decrement the counter by 1 (and increment its possible base by 1)
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void Decrement(string categoryName, string counterName)
        {
            this.Decrement(categoryName, null, counterName);
        }

        /// <summary>
        /// Decrement the counter by 1 (and increment its possible base by 1)
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance of this category, optional can be null</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void Decrement(string categoryName, string instanceName, string counterName)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            ourCategory.Decrement(counterName);
        }

        /// <summary>
        /// Returns the next calculated value for this counter
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the counter in the category</param>
        /// <returns>the calculated value for this counter</returns>
        public float NextValue(string categoryName, string counterName)
        {
            return this.NextValue(categoryName, null, counterName);
        }

        /// <summary>
        /// Returns the next value for this counter
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance of this category, optional can be null</param>
        /// <param name="counterName">name of the counter in the category</param>
        /// <returns>the calculated value for this counter</returns>
        public float NextValue(string categoryName, string instanceName, string counterName)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            return ourCategory.NextValue(counterName);
        }

        /// <summary>
        /// Sets the raw value of this counter
        /// </summary>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void SetRawValue(string categoryName, string counterName, long value)
        {
            this.SetRawValue(categoryName, null, counterName, value);
        }

        /// <summary>
        /// Sets the raw value of this counter
        /// </summary>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance of this category, optional can be null</param>
        /// <param name="counterName">name of the counter in the category</param>
        public void SetRawValue(string categoryName, string instanceName, string counterName, long value)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            ourCategory.SetRawValue(counterName, value);
        }

        /// <summary>
        /// Gets the raw value of this counter
        /// </summary>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="counterName">name of the counter in the category</param>
        public long GetRawValue(string categoryName, string counterName)
        {
            return this.GetRawValue(categoryName, null, counterName);
        }

        /// <summary>
        /// Gets the raw value of this counter
        /// </summary>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">name of the instance of this category, optional can be null</param>
        /// <param name="counterName">name of the counter in the category</param>
        public long GetRawValue(string categoryName, string instanceName, string counterName)
        {
            ValidateCategoryAndCounterNameExists(categoryName, counterName);
            WrappedPerformanceCategory ourCategory = this.CacheCountersForCategory(categoryName, instanceName);
            return ourCategory.GetRawValue(counterName);
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
            this.CacheCountersForCategory(categoryName, instanceName);
        }

        /// <summary>
        /// caches the performance counters for a given category name.  Only caches one time no matter how often it is called
        /// <para></para>
        /// This may get called at startup in parallel by many threads in a loaded system where the load starts updating counters immediately.
        /// In that case you may wish to 'warm up' the system by making a single CacheCountersForCategory() for each category you expect to use in a startup method.
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if a category or category/instance does not exist</exception>
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="instanceName">optional instance name in category</param>
        /// <returns>A wrapped category usually for diagnostics or so someone can enumerate the counter names</returns>
        internal WrappedPerformanceCategory CacheCountersForCategory(string categoryName, string instanceName = null)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentException("Missing categoryName");
            }
            string compoundKey = this.CalculateCategoryInstanceKey(categoryName, instanceName);
            if (!categories.ContainsKey(compoundKey))
            {
                WrappedPerformanceCategory ourWrappedCategory = new WrappedPerformanceCategory(categoryName, instanceName);
                if (ourWrappedCategory != null)
                {
                    categories.TryAdd(compoundKey, ourWrappedCategory);
                }
            }
            return categories[compoundKey];
        }

        /// <summary>
        /// helper method that calculates our cache key
        /// </summary>
        /// <param name="categoryName">string name of performance category</param>
        /// <param name="instanceName">string name of instance </param>
        /// <returns>a key made up of category and instance names</returns>
        internal string CalculateCategoryInstanceKey(string categoryName, string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                //// follow the standard naming conventions
                //// return "\\" + categoryName;
                //// optimize for performance -- no string op when just a category
                return categoryName;
            }
            else
            {
                //// follow the standard naming conventions
                //// return "\\" + categoryName + "(" + instanceName + ")";
                //// could optimize more by removing a literal
                return categoryName + "(" + instanceName + ")";
            }
        }

        /// <summary>
        /// validates that category and counter name were provided.
        /// Exists to remove duplicate code and reduce method complexity
        /// <para></para>
        /// <exception cref="ArgumentException">if a category counter or name is not provided</exception>
        /// </summary>
        /// <param name="categoryName">the supplied category name</param>
        /// <param name="counterName">the supplied counter name</param>
        private static void ValidateCategoryAndCounterNameExists(string categoryName, string counterName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentException("Missing categoryName");
            }
            if (string.IsNullOrEmpty(counterName))
            {
                throw new ArgumentException("Missing counterName");
            }
        }
    }
}
