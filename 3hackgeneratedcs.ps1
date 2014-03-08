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
echo "Hacking generated JNI C# files"
$pathtofiles = "generated\clr"
$oldregex = "global::net.sf.jni4net.jni.JNIEnv @__env = global::net.sf.jni4net.jni.JNIEnv.Wrap"
$new = "// quick hack to get the thread cache correctly set up because the Wrap doesn't do that
			global::net.sf.jni4net.jni.JNIEnv @__ignored = global::net.sf.jni4net.jni.JNIEnv.ThreadEnv;
			global::net.sf.jni4net.jni.JNIEnv @__env = global::net.sf.jni4net.jni.JNIEnv.Wrap"
 Get-Childitem $pathtofiles -recurse -name -Include *.cs | ForEach {
  echo ":::: $pathtofiles\$_"
  $fullpath = "$pathtofiles\$_"
  (Get-Content $fullpath | ForEach {$_ -replace "global::net.sf.jni4net.jni.JNIEnv \@__env = global::net.sf.jni4net.jni.JNIEnv.Wrap", "$new"}) | Set-Content $fullpath 
}
