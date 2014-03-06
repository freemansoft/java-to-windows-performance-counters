//-----------------------------------------------------------------------
// <copyright file="PerformanceCounterKey.cs" company="FreemanSoft">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class pass an integer counter identifier from Java instead of passing strings.  
    /// We can use this to "look up" the category, instance and counter when passed an integer key.
    /// </summary>
    internal class PerformanceCounterKey
    {
        /// <summary>
        /// A key constructor
        /// <exception cref="ArgumentException">if a categoryName or counterName is not provided</exception>
        /// </summary>
        /// <param name="categoryName">Performance counter category name</param>
        /// <param name="instanceName">Performance counter category instance name.  This can be null for default instances</param>
        /// <param name="counterName"> Performance counter name inside the category and instance</param>
        internal PerformanceCounterKey(string categoryName, string instanceName, string counterName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentException("Missing required categoryName");
            }
            if (string.IsNullOrEmpty(counterName))
            {
                throw new ArgumentException("Missing required counterName");
            }
            this.CategoryName = categoryName;
            this.InstanceName = string.Empty.Equals(instanceName) ? null : instanceName;
            this.CounterName = counterName;
            this.KeyCode = (this.CategoryName + ":" + this.InstanceName + ":" + this.CounterName).GetHashCode();
        }

        /// <summary>
        /// Gets Performance counter category name
        /// </summary>
        internal string CategoryName { get; private set; }

        /// <summary>
        /// Gets Performance counter instance name in category. Can be null for default instance.
        /// </summary>
        internal string InstanceName { get; private set; }

        /// <summary>
        /// Gets Performance Counter name.
        /// </summary>
        internal string CounterName { get; private set; }

        /// <summary>
        /// Gets Numeric identifier for this counter
        /// </summary>
        internal int KeyCode { get; private set; }
    }
}
