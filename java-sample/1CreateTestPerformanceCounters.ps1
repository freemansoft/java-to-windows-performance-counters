#this script must be run as administrator
#create two counters
$categoryName = "Freemansoft.TestCategory"
$countCounterName = "TestCounter"
$rateCounterName = "TestRate"
$categoryexists = [System.Diagnostics.PerformanceCounterCategory]::Exists($categoryName)
if ($categoryexists)
{
    [System.Diagnostics.PerformanceCounterCategory]::Delete($categoryName) | Out-Null	
}
$counterData = new-object System.Diagnostics.CounterCreationDataCollection
 
$counter = new-object  System.Diagnostics.CounterCreationData
$counter.CounterType = [System.Diagnostics.PerformanceCounterType]::NumberOfItems64
$counter.CounterName = $countCounterName
$counterData.Add($counter)

$counter = new-object  System.Diagnostics.CounterCreationData
$counter.CounterType = [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32
$counter.CounterName = $rateCounterName
$counterData.Add($counter)
 
[System.Diagnostics.PerformanceCounterCategory]::Create($categoryName, "help text for category", [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance, $counterData)
 
