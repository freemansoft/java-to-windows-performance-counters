/*
Copyright 2014 FreemanSoft Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

import java.io.IOException;
import java.net.URISyntaxException;
import java.net.URL;

import net.sf.jni4net.Bridge;
import freemansoft.performancecounters.WindowsPerformanceFacade;
import system.InvalidOperationException; // C# .Net wrapper exception can be thrown by library

/**
 * @author Joe Freeman This program generates a sawtooth pattern. It assumes the
 *         counters have already been created by running the Powershell script
 *         in this directory as Administrator
 */
public class SawtoothSample {

    private static String CATEGORY_NAME = "Freemansoft.JavaTestCategory";
    // a rate per second counter
    private static String COUNTER_NAME = "TestRate";

    // these are per second rates calculated on sliding windows
    private static int PHASE_TIME_MSEC = 2000;
    private static int REPETITIONS = 20;
    private static int COUNTS_PER_SECOND = 200;
    private static int MSEC_WAIT_PER_COUNT = PHASE_TIME_MSEC / COUNTS_PER_SECOND;

    public static void main(String[] args) throws IOException, InterruptedException, URISyntaxException
             {
        // create bridge, with default setup. It will lookup jni4net.n.dll next
        // to jni4net.j.jar
        Bridge.setVerbose(true);
        Bridge.init();
        URL urlPerfWrap = Bridge.class.getClassLoader().getResource("FreemanSoft.PerformanceCountersJNI.j4n.dll");
        Bridge.LoadAndRegisterAssemblyFrom(new java.io.File(urlPerfWrap.toURI()));
        int counterId; // integer representing category/counter combination
        try {
            WindowsPerformanceFacade.CacheCounters(CATEGORY_NAME, null);
            counterId = WindowsPerformanceFacade.GetPerformanceCounterId(CATEGORY_NAME, "", COUNTER_NAME);
        } catch (system.InvalidOperationException e) {
            System.out
                    .println("You must first create the performance counter running the CreateTestPerformanceCounters.ps1 powershell script in this source directory AS ADMINISTRATOR.");
            throw e;
        }

        for (int reps = 0; reps < REPETITIONS; reps++) {
            for (int sampleCount = 0; sampleCount < COUNTS_PER_SECOND; sampleCount++) {
                WindowsPerformanceFacade.Increment(counterId);
                Thread.sleep(MSEC_WAIT_PER_COUNT);
            }
            // do the silent part of the cycle to bring down averages
            Thread.sleep(PHASE_TIME_MSEC);
        }
    }
}
