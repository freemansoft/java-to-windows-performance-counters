# Overview

This library lets you update Microsoft Windows Performance Counters from Java based programs.

* The C# library is capable of 10,000,000 performance counter updates per second with 4 threads.
* The Java library (wrapper) is capable of 46,000 performance counter updates per second with 4 threads with the stock JNI wrapper
* The Java library (modified wrapper) is capable of 5,000,000 updates per second. 

	
The GitHub repository contains the following pieces

1. The source for C# library that wraps the performance counters in an "easy to JNI" format. 
2. jni4net JNI generation library available on sourceforge
3. Generated C# dll and Java jar files for the JNI interface
4. The pre-built binaries in the packages directory that let you run use this library without having to build the originating dll or the JNI components
5. The source for sample java project that shows how to use the library and allows you to run performance tests.  There is a powershell script in this directory that must be run as administrator to create the Test/Demo performance counters.
6. A powershell script that builds the JNI components and runs the demos.  This assumes that you have already built the base C# library with visual studio.
7. A powershell script that creates the performance counters used by the java test program

# API

There are two different API classes that provide the exact same functionality. One is a string based API that directly maps to the C# library. The other is a key based API that reduces the need to continually pass keys back and forth.

*	MultiThreadedLiason: String based API where the category/(instance)/counter strings are passed in on every call.
*	MultiThreadedFacade: Completely static interface. This increases performance 5%.  Numeric key based API where the caller registers a category/(instance)/counter string combo to get a key that is used in subsequent calls.

# Using the Provided Binaries
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

	* Build the C code
		*Open the visual studio *as administrator* to be able to create and teardown performance counters in unit tests.
			* Open the solution and build. It should run all the unit/integration tests.
	* Create the performance counters for the Java code
		*Open an Administrator powershell
			* CD to the java-sample directory
			* Run 1CreateTestPerformanceCounters.ps1 to create the performance counters. The java code doesn't have permission to create performance counters.
	* Generate the JNI code, build java and run the samples.
		*Open a new powershell to build the java code and run the tests. You may be able to re-use the admin powershell. I've never done that.
			* Verify the 1build.ps1 paths work for you. They WILL NOT be right.  The script assumes you are sitting in the root of this repository.  It does have hard coded paths to csc.exe
			* cd into the top level directory
			* Run the 1build.ps1 powershell script to build and run java code
	* Watch everything fly by.
	
