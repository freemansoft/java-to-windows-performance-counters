# Overview

This library lets you update Microsoft Windows Performance Counters from Java based programs.

* The C# library is capable of 10,000,000 performance counter updates per second with 4 threads.
* The Java library (wrapper) is capable of 46,000 performance counter updates per second with 4 threads with the stock JNI wrapper
* The Java library (modified wrapper) is capable of 5,000,000 updates per second. 

## Additional links
* [A quick overview of Windows Performance Counters](http://joe.blog.freemansoft.com/2014/03/windows-performance-counters.html)
* [Recording Java metrics with Windows Performance Counters](http://joe.blog.freemansoft.com/2014/03/recording-java-metrics-with-windows.html)
* [A quick overview of the Java wrapper API](http://joe.blog.freemansoft.com/2014/03/java-api-wrapper-for-windows.html)
	
The GitHub repository contains the following pieces

1. Source for C# library that wrap the performance counters in an "easy to JNI" format. 
2. jni4net JNI generation library available on sourceforge used to create java proxies
3. The pre-built binaries in the packages directory that let you run use this library without having to build the originating dll or the JNI components.  This includes a javadoc jar for the proxies
4. The source for sample java project that shows how to use the library and allows you to run performance tests.  There is a powershell script in this directory that must be run as administrator to create the Test/Demo performance counters.
5. 1build.ps1: A powershell script that builds the C# library, JNI components and runs the demos.  This assumes that you have .net 4.0 installed with its csc and msbuild executables.
	1. 3hacgeneratedcs.ps1: A powershell script that patches generated C# fils
	2. 4hackgeneratedjava.ps1: A powershell script that adds javadoc to the generated Java files
6. A powershell script that creates the performance counters used by the java test program.  Windows requires 
that performance counters be created by an administrator before they can be used.

# API

There are two different API classes that provide the exact same level of functionality. One is a string based API that directly maps to the C# library. The other is a key based API that reduces the need to continually pass keys back and forth.

*	MultiThreadedLiason: String based API where the category/(instance)/counter strings are passed in on every call.
*	MultiThreadedFacade: Completely static interface. This provides 7X thre throughput of MultiThreadedLiason.  Numeric key based API where the caller registers a category/(instance)/counter string combo to get a key that is used in subsequent calls.

Performance Counters use the high resolution system timer.  You must use the StopwachTimestamp() method when doing timer spans.

# Using the Provided Binaries
The github repository holds the latest binaries in the packages directory. You can use them without having to compile C# code or build any proxies.
Steps to use binaries


	1. Create a java project.  You can use the samples in java-sample as guidance
	2. Add the following to your path.  You need the "packages" so that your application can find the dll
		packages/FreemanSoft.PerformanceCounters.j4n.jar
		packages/jni4net.j-0.8.6.0.jar
		packages
	3. Build
	4. Run


# Building from sources
Steps to build the binaries from scratch

	* Create Windows performance counters that the Java code will update.
		*Open an Administrator powershell
			* CD to the java-sample directory
			* Run 1CreateTestPerformanceCounters.ps1 to create the performance counters. The java code doesn't have permission to create performance counters.
	* Build the C# code, generate the JNI wrapper code, build java and run the samples.
		*Open a new powershell command window.
			* Verify the 1build.ps1 paths work for you. They include the paths to Java and to your version of .Net. You should assume they WILL NOT be right.  
				The script assumes you are sitting in the root of this repository.  It does have hard coded paths to csc.exe and msbuild
			* Cd to the root of this project
			* Run the 1build.ps1 powershell script to build the C# library, build the java code and run java tests/samples that generate metrics
	* Watch everything fly by.
	

