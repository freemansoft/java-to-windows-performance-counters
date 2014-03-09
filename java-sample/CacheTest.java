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
import system.Console;
import freemansoft.performancecounters.WindowsPerformanceLiason;

/**
 * @author Joe Freeman This program reads the dirty cache Performance Counter in
 *         the cache category 20 times delaying between reads. It assumes the
 *         counters have already been created by running the powershell script
 *         in this directory as Administrator
 */
public class CacheTest {
    public static void main(String[] args) throws IOException, InterruptedException, URISyntaxException {
        // create bridge, with default setup
        // it will lookup jni4net.n.dll next to jni4net.j.jar
        Bridge.setVerbose(true);
        Bridge.init();
        URL urlPerfWrap = Bridge.class.getClassLoader().getResource("FreemanSoft.PerformanceCountersJNI.j4n.dll");
        Bridge.LoadAndRegisterAssemblyFrom(new java.io.File(urlPerfWrap.toURI()));

        WindowsPerformanceLiason liason = new WindowsPerformanceLiason();
        for (int i = 0; i < 20; i++) {
            liason.CacheCounters("cache", null);
            Console.WriteLine("found value for counter dirty cache " + liason.NextValue("cache", "Dirty Pages"));
            Thread.sleep(100);
        }

    }
}