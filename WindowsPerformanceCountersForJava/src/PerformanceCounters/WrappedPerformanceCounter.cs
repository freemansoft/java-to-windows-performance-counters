//-----------------------------------------------------------------------
// <copyright file="WrappedPerformanceCounter.cs" company="FreemanSoft">
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
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for the PerformanceCounter class
    /// </summary>
    internal class WrappedPerformanceCounter
    {
        /// <summary>
        /// Maps all of the counter types that need base counters to their base counter type.
        /// This isn't a concurrent dictionary because it is populated only once at class load time
        /// </summary>
        private static Dictionary<PerformanceCounterType, PerformanceCounterType> counterTypeToBaseType;

        /// <summary>
        /// the library counter this wraps
        /// </summary>
        private PerformanceCounter wrappedCounter;

        /// <summary>
        /// the library counter that supports wrappedCounter
        /// </summary>
        private PerformanceCounter associatedBase;

        /// <summary>
        /// Whether or not we can modify this counter
        /// </summary>
        private bool isReadOnly = false;

        /// <summary>
        /// used to initialize some lookup tables
        /// </summary>
        static WrappedPerformanceCounter()
        {
            counterTypeToBaseType = new Dictionary<PerformanceCounterType, PerformanceCounterType>
            {
                ////{ PerformanceCounterType.AverageBase, PerformanceCounterType.AverageBase },
                { PerformanceCounterType.AverageCount64, PerformanceCounterType.AverageBase },
                { PerformanceCounterType.AverageTimer32, PerformanceCounterType.AverageBase },
                ////{ PerformanceCounterType.CounterDelta32, PerformanceCounterType.CounterDelta32 },
                ////{ PerformanceCounterType.CounterDelta64, PerformanceCounterType.CounterDelta64 },
                ////{ PerformanceCounterType.CounterMultiBase, PerformanceCounterType.CounterMultiBase },
                { PerformanceCounterType.CounterMultiTimer, PerformanceCounterType.CounterMultiBase },
                { PerformanceCounterType.CounterMultiTimer100Ns, PerformanceCounterType.CounterMultiBase },
                { PerformanceCounterType.CounterMultiTimer100NsInverse, PerformanceCounterType.CounterMultiBase },
                { PerformanceCounterType.CounterMultiTimerInverse, PerformanceCounterType.CounterMultiBase },
                ////{ PerformanceCounterType.CounterTimer, PerformanceCounterType.CounterTimer },
                ////{ PerformanceCounterType.CounterTimerInverse, PerformanceCounterType.CounterTimerInverse },
                ////{ PerformanceCounterType.CountPerTimeInterval32, PerformanceCounterType.CountPerTimeInterval32 },
                ////{ PerformanceCounterType.CountPerTimeInterval64, PerformanceCounterType.CountPerTimeInterval64 },
                ////{ PerformanceCounterType.ElapsedTime, PerformanceCounterType.ElapsedTime },
                ////{ PerformanceCounterType.NumberOfItems32, PerformanceCounterType.NumberOfItems32 },
                ////{ PerformanceCounterType.NumberOfItems64, PerformanceCounterType.NumberOfItems64 },
                ////{ PerformanceCounterType.NumberOfItemsHEX32, PerformanceCounterType.NumberOfItemsHEX32 },
                ////{ PerformanceCounterType.NumberOfItemsHEX64, PerformanceCounterType.NumberOfItemsHEX64 },
                ////{ PerformanceCounterType.RateOfCountsPerSecond32, PerformanceCounterType.RateOfCountsPerSecond32 },
                ////{ PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterType.RateOfCountsPerSecond64 },
                ////{ PerformanceCounterType.RawBase, PerformanceCounterType.RawBase },
                { PerformanceCounterType.RawFraction, PerformanceCounterType.RawBase },
                ////{ PerformanceCounterType.SampleBase, PerformanceCounterType.SampleBase },
                ////{ PerformanceCounterType.SampleCounter,PerformanceCounterType.SampleCounter },
                { PerformanceCounterType.SampleFraction, PerformanceCounterType.SampleBase },
                ////{ PerformanceCounterType.Timer100Ns, PerformanceCounterType.Timer100Ns },
                ////{ PerformanceCounterType.Timer100NsInverse, PerformanceCounterType.Timer100NsInverse },
                //// we should probably add other types to this list later
                //// other known WMI counter types http://msdn.microsoft.com/en-us/library/aa394569(v=vs.85).aspx
                //// Base Counter Types
                //// Basic Algorithm Counter Types
                //// Counter Algorithm Counter Types
                //// Noncomputational Counter Types
                //// Precision Timer Algorithm Counter Types
                //// Queue-length Algorithm Counter Types
                //// Statistical Counter Types
                //// Timer Algorithm Counter Types
            };
        }

        /// <summary>
        /// constructor for this class
        /// </summary>
        /// <param name="counterToBeWrapped">the contained counter</param>
        /// <param name="isReadOnly">advisory on whether or not the wrapped counter can be modified. Doesn't seem to be a way to ask the counter</param>
        internal WrappedPerformanceCounter(PerformanceCounter counterToBeWrapped, bool isReadOnly) :
            this(counterToBeWrapped, null, isReadOnly)
        {
            //// nothing else to be done
        }

        /// <summary>
        /// constructor for this class
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if no counter provided to be wrapped</exception>
        /// </summary>
        /// <param name="counterToBeWrapped">the contained counter</param>
        /// <param name="associatedBaseToBeWrapped">optional associated base counter</param>
        /// <param name="isReadOnly">advisory on whether or not the wrapped counter can be modified. Doesn't seem to be a way to ask the counter</param>
        internal WrappedPerformanceCounter(PerformanceCounter counterToBeWrapped, PerformanceCounter associatedBaseToBeWrapped, bool isReadOnly)
        {
            if (counterToBeWrapped == null)
            {
                throw new ArgumentNullException("No counter provided to wrap");
            }
            this.wrappedCounter = counterToBeWrapped;
            this.associatedBase = associatedBaseToBeWrapped; //// this is an optional parameter
            this.isReadOnly = isReadOnly;
            System.Diagnostics.Debug.WriteLine("WrappedPerformanceCounter: '" + counterToBeWrapped.CounterName
                + "' supported by: '" + (associatedBaseToBeWrapped == null ? "null" : associatedBaseToBeWrapped.CounterName) + "'");
        }

        /// <summary>
        /// utility method that tells us the matching base type for a counter if it needs one
        /// </summary>
        /// <param name="queryCounter">an instantiated counter</param>
        /// <returns>base counter type if one exists or null if none needed</returns>
        internal static PerformanceCounterType? GetBaseTypeForCounter(PerformanceCounter queryCounter)
        {
            if (queryCounter != null)
            {
                PerformanceCounterType queryCounterType = queryCounter.CounterType;
                if (WrappedPerformanceCounter.counterTypeToBaseType.ContainsKey(queryCounterType))
                {
                    return WrappedPerformanceCounter.counterTypeToBaseType[queryCounterType];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// increments this counter by the specified amount and its optional base by 1
        /// This method doesn't really make sense when using a timer counter with a base counter since they both are always incremented by the same amount.
        /// <para></para>
        /// This method exists because the .Net 3.5 docs say they are significantly faster than incrementBy(1) <see cref="http://msdn.microsoft.com/en-us/library/wzdx16ez(v=vs.90).aspx"/>
        /// Testing shows a 10x difference
        /// <para></para>
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// </summary>
        internal void Increment()
        {
            if (this.associatedBase != null)
            {
                this.associatedBase.Increment();
            }
            else
            {
                this.wrappedCounter.Increment();
            }
        }

        /// <summary>
        /// increments this counter by the specified amount and its optional base by 1
        /// This method can be used with a counter that has a base counter as long as you want the base incremented by 1.
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// </summary>
        /// <param name="incrementAmount">the amount to increment</param>
        internal void IncrementBy(long incrementAmount)
        {
            if (this.associatedBase != null)
            {
                this.IncrementBy(incrementAmount, 1);
            }
            else
            {
                this.wrappedCounter.IncrementBy(incrementAmount);
            }
        }

        /// <summary>
        /// increments this counter by the incrementAmount and its mandatory base by incrementAmountBase
        /// This method can be used with a counter that has a base counter as long as you want the base incremented by 1.
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly or there is no associated base</exception>
        /// </summary>
        /// <param name="incrementAmount">the amount to increment</param>
        /// <param name="incrementAmountBase">the amount to increment the base counter</param>
        internal void IncrementBy(long incrementAmount, long incrementAmountBase)
        {
            if (this.associatedBase == null)
            {
                throw new InvalidOperationException(this.wrappedCounter.CounterName
                    + ": Unable to increment counter and base because no base exists");
            }
            this.wrappedCounter.IncrementBy(incrementAmount);
            this.associatedBase.IncrementBy(incrementAmountBase);
        }

        /// <summary>
        /// Decrement this counter by 1 and increments and its optional base by 1
        /// This method doesn't really make sense when using a timer counter with a base counter since they both are always incremented by the same amount.
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// </summary>
        internal void Decrement()
        {
            this.wrappedCounter.Decrement();
            if (this.associatedBase != null)
            {
                this.associatedBase.Increment();
            }
        }

        /// <summary>
        /// returns the calculated value for the counter
        /// </summary>
        /// <returns>Returns the calculated value for the current sample</returns>
        internal float NextValue()
        {
            return this.wrappedCounter.NextValue();
        }

        /// <summary>
        /// Returns the raw value property of the counter
        /// </summary>
        /// <returns>the raw value property</returns>
        internal long GetRawValue()
        {
            return this.wrappedCounter.RawValue;
        }

        /// <summary>
        /// Sets the raw value property of the counter
        /// <exception cref="System.InvalidOperationException">if this counter is ReadOnly</exception>
        /// </summary>
        /// <param name="value">the new raw value</param>
        internal void SetRawValue(long value)
        {
            this.wrappedCounter.RawValue = value;
        }

        /// <summary>
        /// used mostly for testing
        /// </summary>
        /// <returns>true if this counter is read only</returns>
        internal bool CounterIsReadOnly()
        {
            //// Made internal so unit tests can see it. Shiv will shiv me when he sees this.
            return this.isReadOnly;
        }

        /// <summary>
        /// used for testing
        /// </summary>
        /// <returns>true if this counter has an real base counter associated with it</returns>
        internal bool CounterHasAssociatedBase()
        {
            return this.associatedBase != null;
        }
    }
}
