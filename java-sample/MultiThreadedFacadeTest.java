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
import java.util.ArrayList;
import java.util.List;

import net.sf.jni4net.Bridge;
import freemansoft.performancecounters.WindowsPerformanceFacade;

/**
 * @author Joe Freeman This program posts as many "rate per second" counter
 *         updates as possible. It assumes the counters have already been
 *         created by running the powershell script in this directory as
 *         Administrator
 *         <p>
 *         Here are the number of calls that can be made to a rate/second
 *         monitor on a quad core bootcamp macbook
 * 
 *         <pre>
 *         default jni4net               hacked jni4net
 *         14,000/sec 1 threads          2.0 million
 *         26,000/sec 2 threads			 3.5 million
 *         40,000/sec 3 threads 46% cpu  5.3 million 30%
 *         46,000/sec 4 threads 55% cpu  6.5 million 50%
 *         47,500/sec 6 threads 85% cpu  6.3 million 75%
 *         48,000/sec 8 threads 99% cpu
 * </pre>
 * 
 *         The C code is about 5 times faster
 */
public class MultiThreadedFacadeTest {

    private static String CATEGORY_NAME = "Freemansoft.JavaTestCategory";
    // private static String COUNTER_NAME = "TestCounter";
    private static String COUNTER_NAME = "TestRate";
    private static int NUMBER_OF_THREADS = 2;
    private static int TEST_RUN_TIME_IN_MSEC = 10000;

    public static void main(String[] args) throws IOException, InterruptedException, URISyntaxException {
        // create bridge, with default setup
        // it will lookup jni4net.n.dll next to jni4net.j.jar
        Bridge.setVerbose(true);
        Bridge.init();
        URL urlPerfWrap = Bridge.class.getClassLoader().getResource("FreemanSoft.PerformanceCountersJNI.j4n.dll");
        Bridge.LoadAndRegisterAssemblyFrom(new java.io.File(urlPerfWrap.toURI()));

        MultiThreadedFacadeTest testInstance = new MultiThreadedFacadeTest();
        testInstance.start();
    }

    public void start() throws InterruptedException {
        int counterId;
        long startTime;
        startTime = System.currentTimeMillis();
        try {
            WindowsPerformanceFacade.CacheCounters(CATEGORY_NAME, null);
            counterId = WindowsPerformanceFacade.GetPerformanceCounterId(CATEGORY_NAME, "", COUNTER_NAME);
        } catch (system.InvalidOperationException e) {
            System.out
                    .println("You must first create the performance counter running the CreateTestPerformanceCounters.ps1 powershell script in this source directory AS ADMINISTRATOR.");
            throw e;
        }
        System.out.println("Current counter value is " + WindowsPerformanceFacade.NextValue(counterId));
        System.out.println("Caching took " + (System.currentTimeMillis() - startTime));
        List<Thread> workerThreads = new ArrayList<Thread>();
        // // keep these so we can interrogate them later
        List<MyRunnable> runners = new ArrayList<MyRunnable>();
        for (int i = 0; i < NUMBER_OF_THREADS; i++) {
            // // these category and counter names must match the names in
            // CreateTestPerformanceCounters.ps1
            MyRunnable executor = new MyRunnable(counterId);
            Thread worker = new Thread(executor);
            worker.setName("MyRunnable " + i);
            runners.add(executor);
            workerThreads.add(worker);
        }
        // System.out.println(runners.size()+" runners and "+workerThreads.size()+" threads.");
        long startThreadInitiate = +System.currentTimeMillis();
        for (Thread worker : workerThreads) {
            worker.start();
        }
        long startThreadFinish = System.currentTimeMillis();
        long sleepTimeInitiate = startThreadFinish;
        Thread.sleep(TEST_RUN_TIME_IN_MSEC);
        long sleepTimeFinish = System.currentTimeMillis();
        long signalThreadInitiate = sleepTimeFinish;
        for (Thread worker : workerThreads) {
            worker.interrupt();
        }
        long signalThreadFinish = System.currentTimeMillis();
        long threadStopInitiate = signalThreadFinish;
        for (Thread worker : workerThreads) {
            worker.join();
        }
        long threadStopFinish = System.currentTimeMillis();
        System.out.println("Started in " + (startThreadFinish - startThreadInitiate));
        System.out.println("Stopping after " + (sleepTimeFinish - sleepTimeInitiate));
        System.out.println("All threads signaled in " + (signalThreadFinish - signalThreadInitiate));
        System.out.println("All threads stopped in " + (threadStopFinish - threadStopInitiate));
        int totalCount = 0;
        for (MyRunnable runner : runners) {
            totalCount += runner.executionCount;
        }
        System.out.println(runners.size() + " threads incremented " + totalCount + " times.");
    }

    /**
     * A task executor that stuffs as many increments as possible
     */

    class MyRunnable implements Runnable {
        private final int counterId;
        public int executionCount = 0;

        MyRunnable(int counterId) {
            this.counterId = counterId;
        }

        @Override
        public void run() {
            System.out.println("'" + Thread.currentThread().getName() + "' ready for CounterId " + this.counterId);
            long startTime = System.currentTimeMillis();
            try {
                this.executionCount = 0;
                while (!Thread.currentThread().isInterrupted()) {
                    // I commenting out the facade call but got order the same
                    // performance
                    WindowsPerformanceFacade.Increment(counterId);
                    this.executionCount++;
                    // System.out.print(".");
                }
            } catch (system.InvalidOperationException e) {
                System.out
                        .println("You must first create the performance counter running the CreateTestPerformanceCounters.ps1 powershell script in this source directory AS ADMINISTRATOR.");
                // throw e;
            }
            System.out.println("'" + Thread.currentThread().getName() + "' ran for "
                    + (System.currentTimeMillis() - startTime));
        }
    }
}
