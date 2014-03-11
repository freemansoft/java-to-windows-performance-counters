# Copyright 2014 FreemanSoft Inc
# Licensed under the Apache License, Version 2.0 (the "License");
#
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
# The code generator wipes out the javadoc

echo "Hacking generated Java files to add javadoc"
$pathtofiles = "generated\jvm\freemansoft\performancecounters"
$sourceForFacade = "WindowsPerformanceFacade.java"

$newFacadeClassDoc ='/**
 * Java proxy for the C# WindowsPerformanceFacade class.
 * This is a static wrapper for the Windows Performance Counters 
 * that uses an integer key for the category/counter names to speed up performance
 * First get a key for a category/instance/counter combination.
 * Then use that integer key to manipulate the counters with the rest of the API
 */
@net.sf.jni4net.attributes.ClrType'
$newFacadeIncrement ='/**
     * Modify the specified counter by 1
     * Increment any paired base counter by 1
     * @param counterKey a counter identifier retrieved from GetPerformanceCounterId
     */
    @net.sf.jni4net.attributes.ClrMethod("(I)V")'
$newFacadeIncrementBy='/**
     * Increment the specified counter by the specified amount.
     * Increment any paired base counter by 1
     * @param counterKey a counter identifier retrieved from GetPerformanceCounterId
     * @param incrementAmount the amount to increment a counter
     */
    @net.sf.jni4net.attributes.ClrMethod("(IJ)V")'
$newFacadeIncrementByWithBase='/**
     * Increment the specified counter by the specified amount.
     * Increment any paired base counter by the specified amount
     * @param counterKey a counter identifier retrieved from GetPerformanceCounterId
     * @param incrementAmount the amount to increment a counter
     * @param incrementBaseAmount the amount to increment the base counter
     */
    @net.sf.jni4net.attributes.ClrMethod("(IJJ)V")'
$newFacadeDecrement='/**
     * Decrement the specified counter by 1
     * Increment any paired base counter by 1
     * @param counterKey a counter identifier retrieved from GetPerformanceCounterId
     */
    @net.sf.jni4net.attributes.ClrMethod("(I)V")'
$newFacadeNextValue='/**
     * Retrieve the calcuated value of this counter
     * @param counterKey a counter identifier retrieved from GetPerformanceCounterId
     */
    @net.sf.jni4net.attributes.ClrMethod("(I)F")'
$newFacadeCacheCounters='/**
     * Creates proxies for the requested counter. This only needs to be done once per counter.
     * This is a very slow, almost 400ms, operation.  Do it in setup if you do not want a first time hit.
     * @param categoryName the name of the category
     * @param instanceName the optional name of the instance. You can pass null or empty string for default instance
     * @param counterName the name of the counter
     */
    @net.sf.jni4net.attributes.ClrMethod("(LSystem/String;LSystem/String;)V")'
$newFacadeGetPerformanceCounterId='/**
     * Create a unique integer key to represent the category, instance and counter name combination.
     * @param categoryName the name of the category
     * @param instanceName the optional name of the instance. You can pass null or empty string for default instance
     * @param counterName the name of the counter
     * @return the unique id for this counter
     */
    @net.sf.jni4net.attributes.ClrMethod("(LSystem/String;LSystem/String;LSystem/String;)I")'

(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace "@net.sf.jni4net.attributes.ClrType", "$newFacadeClassDoc" }) | 
	Set-Content $pathtofiles\$sourceForFacade
	
(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(I\)V"\)',"$newFacadeIncrement" }) | 
	Set-Content $pathtofiles\$sourceForFacade
(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(IJ\)V"\)',"$newFacadeIncrementBy" }) | 
	Set-Content $pathtofiles\$sourceForFacade
(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(IJJ\)V"\)',"$newFacadeIncrementByWithBase" }) | 
	Set-Content $pathtofiles\$sourceForFacade
#(Get-Content $pathtofiles\$sourceForFacade | 
#	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(I\)V"\)',"$newFacadeDecrement" }) | 
#	Set-Content $pathtofiles\$sourceForFacade
(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(I\)F"\)',"$newFacadeNextValue" }) | 
	Set-Content $pathtofiles\$sourceForFacade

(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(LSystem/String;LSystem/String;\)V"\)',"$newFacadeCacheCounters" }) | 
	Set-Content $pathtofiles\$sourceForFacade
(Get-Content $pathtofiles\$sourceForFacade | 
	ForEach { $_ -replace '@net.sf.jni4net.attributes.ClrMethod\("\(LSystem/String;LSystem/String;LSystem/String;\)I"\)',"$newFacadeGetPerformanceCounterId" }) | 
	Set-Content $pathtofiles\$sourceForFacade

	