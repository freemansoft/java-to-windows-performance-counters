// <copyright file="CounterTestUtilities.cs" company="FreemanSoft">
//     Copyright FreemanSoft Inc. This will be moved to an opensource license in the future
// </copyright>
//-----------------------------------------------------------------------
namespace FreemanSoft.PerformanceCounters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// utilities class
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class CounterTestUtilities
    {
        /// <summary>
        /// test category name
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        public static string TestCategoryName = "Freemansoft.TestCategory";

        /// <summary>
        /// test counter name for a RateOfCountPerSecond64 counter
        /// you increment how many operations took place and it keeps track of the time.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestCounterRateOfCountPerSecond64Name = "ROCPS64";

        /// <summary>
        /// test counter name for a NumberOfItems64 counter
        /// a simple counter that always goes up
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestCounterNumberOfItems64Name = "NOI64";

        /// <summary>
        /// test counter name for a AverageTimer32 counter
        /// calls to this pass in a time
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestAverageTimer32Name = "AT32";

        /// <summary>
        /// test counter name for a AverageBase counter
        /// calls to this pass in the number of operations completed
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestAverageTimerBaseName = "ATB";

        /// <summary>
        /// test counter name for a AverageCount64 counter
        /// number of items processed (I've used this for bytes processed)
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestAverageCount64Name = "AC64";

        /// <summary>
        /// test counter name for a AverageBase counter for AverageCount64
        /// calls to this pass in the number of operations completed (I've used this for number of messages)
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Want single variable for test classes.")]
        internal static string TestAverageCountBaseName = "ACB";

        /// <summary>
        /// Creates a set of test counters we can use across tests
        /// </summary>
        internal static void GenerateStandardTestCounters()
        {
            if (PerformanceCounterCategory.Exists(TestCategoryName))
            {
                TeardownStandardTestCounters();
            }
            CounterCreationDataCollection counterDescriptions = new CounterCreationDataCollection();

            counterDescriptions.Add(CreateCounterForTest(TestCounterNumberOfItems64Name, PerformanceCounterType.NumberOfItems64));
            counterDescriptions.Add(CreateCounterForTest(TestCounterRateOfCountPerSecond64Name, PerformanceCounterType.RateOfCountsPerSecond64));
            counterDescriptions.Add(CreateCounterForTest(TestAverageTimer32Name, PerformanceCounterType.AverageTimer32));
            counterDescriptions.Add(CreateCounterForTest(TestAverageTimerBaseName, PerformanceCounterType.AverageBase));
            counterDescriptions.Add(CreateCounterForTest(TestAverageCount64Name, PerformanceCounterType.AverageCount64));
            counterDescriptions.Add(CreateCounterForTest(TestAverageCountBaseName, PerformanceCounterType.AverageBase));
            //// notice we don't use/touch any wrapped classes so that we can make sure retrieval is first time they are wrapped
            System.Diagnostics.PerformanceCounterCategory.Create(TestCategoryName, "Category help " + TestCategoryName, PerformanceCounterCategoryType.SingleInstance, counterDescriptions);
        }

        /// <summary>
        /// should delete the counters
        /// </summary>
        internal static void TeardownStandardTestCounters()
        {
            PerformanceCounter.CloseSharedResources();
            PerformanceCounterCategory.Delete(TestCategoryName);
        }

        /// <summary>
        /// Creates counter creation data that can be fed to the performance counter library to crate a counter
        /// </summary>
        /// <param name="counterName">the counter name</param>
        /// <param name="counterType">the type of counter</param>
        /// <returns>a counter creation record that can be bundled into a category creation record </returns>
        private static CounterCreationData CreateCounterForTest(string counterName, PerformanceCounterType counterType)
        {
            CounterCreationData newCounterRecord = new System.Diagnostics.CounterCreationData();
            newCounterRecord.CounterName = counterName;
            newCounterRecord.CounterHelp = "help " + counterName;
            newCounterRecord.CounterType = counterType;
            return newCounterRecord;
        }
    }
}
