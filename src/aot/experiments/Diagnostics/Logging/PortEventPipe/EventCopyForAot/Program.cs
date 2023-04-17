using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace EventCopyForAot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string eventName = "GCStart_V1";
            Console.WriteLine("Hello, World!");
            TakeBackups();
            CopyClrEtwAllMain(eventName);
            CopyClrEventPipeEvents(eventName);
            CopyDisabledDotnetRuntimeProvider(eventName);
            CopyDotnetRuntimeProvider(eventName);
            CopyEtwEvents(eventName);
            Console.WriteLine("Bye, World!");
        }

        private static void CopyEtwEvents(string eventName)
        {
            string destfileName = @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\EtwEvents.h";

            List<string> newlines = new List<string>();
            foreach (string line in File.ReadLines(destfileName))
            {
                //#define FireEtwGCAllocationTick_V2
                if (line.IndexOf($"#define FireEtw{eventName}")>=0)
                {
                    newlines.Add(line.Replace($"FireEtw{eventName}", $"FireEtXplat{eventName}"));
                }
                else
                    newlines.Add(line);
            }
            File.WriteAllLines(destfileName, newlines);
        }

        private static void TakeBackups()
        {
            string[] files = {
                @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\eventpipe\dotnetruntime.cpp",
                @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\clreventpipewriteevents.h",
                @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\clretwallmain.h",
                @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\eventpipe\disableddotnetruntime.cpp",
                @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\EtwEvents.h"
        };
            string dir = @"C:\work\core\LakshanF\CSharp\src\temp\src_bk\src2";

            foreach (string file in files)
            {
                File.Copy(file, Path.Combine(dir, Path.GetFileName(file)), true);
            }

        }

        private static void CopyDotnetRuntimeProvider(string eventName)
        {
            string splitLine = @"EventPipeEvent *EventPipeEventGCStart_V2 = nullptr;";
            string destfileName = @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\eventpipe\dotnetruntime.cpp";
            string srcFileNAme = @"C:\work\core\main\runtime\artifacts\obj\coreclr\windows.x64.Debug\vm\eventing\eventpipe\eventpipe\dotnetruntime.cpp";

            var orgList = File.ReadLines(destfileName).ToList();
            int index = orgList.IndexOf(splitLine);

            bool lineFound = false;
            List<string> newlines = new List<string>();
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"EventPipeEvent *EventPipeEvent{eventName}")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    newlines.Add(line);
                    break;
                }
            }
            orgList.InsertRange(index, newlines);

            splitLine = @"BOOL EventPipeEventEnabledGCStart_V2(void)";
            index = orgList.IndexOf(splitLine);
            lineFound = false;
            newlines = new List<string>();
            int cb = 0;
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"BOOL EventPipeEventEnabled{eventName}")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    if (line.IndexOf("    LPCGUID ")>=0)
                        newlines.Add(line.Replace("    LPCGUID ", "    const GUID * "));
                    else
                        newlines.Add(line);
                    if (line.StartsWith("}"))
                        cb++;
                    if (cb==2)
                    {
                        newlines.Add("");
                        break;
                    }
                }
            }
            orgList.InsertRange(index, newlines);

            splitLine = @"    EventPipeEventGCStart_V2 = EventPipeAdapter::AddEvent(EventPipeProviderDotNETRuntime,1,1,2,EP_EVENT_LEVEL_INFORMATIONAL,false);";
            index = orgList.IndexOf(splitLine);
            lineFound = false;
            newlines = new List<string>();
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"EventPipeEvent{eventName} = EventPipeAdapter::AddEvent")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    newlines.Add(line);
                    break;
                }
            }
            orgList.InsertRange(index, newlines);

            File.WriteAllLines(destfileName, orgList);
        }

        private static void CopyDisabledDotnetRuntimeProvider(string eventName)
        {
            string splitLine = @"ULONG EventPipeWriteEventGCStart_V2(";
            string destfileName = @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\eventpipe\disableddotnetruntime.cpp";
            string srcFileNAme = @"C:\work\core\main\runtime\artifacts\obj\coreclr\windows.x64.Debug\inc\clreventpipewriteevents.h";

            var orgList = File.ReadLines(destfileName).ToList();
            int index = orgList.IndexOf(splitLine);

            bool lineFound = false;
            List<string> newlines = new List<string>();
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"EventPipeWriteEvent{eventName}")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    if (!line.StartsWith(");"))
                    {
                        if (line.IndexOf("    LPCGUID ")>=0)
                        {
                            string repl1 = line.Replace("    LPCGUID ", "    const GUID * ");
                            if (repl1.IndexOf(" = nullptr") >= 0)
                                repl1 = repl1.Replace(" = nullptr", "");
                            newlines.Add(repl1);
                        }
                        else if (line.IndexOf("PCWSTR")>=0)
                            newlines.Add(line.Replace("PCWSTR", "wchar_t*"));
                        else
                            newlines.Add(line);
                    }
                    else
                    {
                        newlines.Add(")");
                        newlines.Add("{");
                        newlines.Add("    return 0;");
                        newlines.Add("}");
                        newlines.Add("");

                        break;
                    }
                }
            }
            orgList.InsertRange(index, newlines);
            File.WriteAllLines(destfileName, orgList);
        }

        private static void CopyClrEventPipeEvents(string eventName)
        {
            string splitLine = @"BOOL EventPipeEventEnabledGCStart_V2(void);";
            string destfileName = @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\clreventpipewriteevents.h";
            string srcFileNAme = @"C:\work\core\main\runtime\artifacts\obj\coreclr\windows.x64.Debug\inc\clreventpipewriteevents.h";

            var orgList = File.ReadLines(destfileName).ToList();
            int index = orgList.IndexOf(splitLine);

            bool lineFound = false;
            List<string> newlines = new List<string>();
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"EventPipeEventEnabled{eventName}")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    if (line.IndexOf("    LPCGUID ")>=0)
                        newlines.Add(line.Replace("    LPCGUID ", "    const GUID * "));
                    else if (line.IndexOf("PCWSTR")>=0)
                        newlines.Add(line.Replace("PCWSTR", "wchar_t*"));
                    else
                        newlines.Add(line);
                    if (line.StartsWith(");"))
                    {
                        break;
                    }
                }
            }
            orgList.InsertRange(index, newlines);
            File.WriteAllLines(destfileName, orgList);
        }

        /// <summary>
        /// Hack to write clrEtwAllMain.h
        ///  - First get upto gc
        ///  - insert the new content
        ///  - Save the file
        /// </summary>
        /// <param name="eventName"></param>
        private static void CopyClrEtwAllMain(string eventName)
        {
            string splitLine = @"inline BOOL EventEnabledGCStart_V2(void) {return EventPipeEventEnabledGCStart_V2();}";
            string destfileName = @"C:\work\core\CurrentWork4\runtime\src\coreclr\nativeaot\Runtime\clretwallmain.h";
            string srcFileNAme = @"C:\work\core\main\runtime\artifacts\obj\coreclr\windows.x64.Debug\inc\clretwallmain.h";
            
            var orgList = File.ReadLines(destfileName).ToList();
            int index = orgList.IndexOf(splitLine);
            bool lineFound = false;
            List<string> newlines = new List<string>();
            foreach (string line in File.ReadLines(srcFileNAme))
            {
                if (line.IndexOf($"EventEnabled{eventName}")>=0)
                {
                    lineFound = true;
                }
                if (lineFound)
                {
                    if (line.IndexOf($"inline BOOL EventEnabled{eventName}")>=0)
                    {
                        // inline BOOL EventEnabledGCEnd_V1(void) {return EventPipeEventEnabledGCEnd_V1() || EventXplatEnabledGCEnd_V1();}
                        string repl1 = $"inline BOOL EventEnabled{eventName}(void) {{return EventPipeEventEnabled{eventName}();}}";
                        newlines.Add(repl1);
                    }
                    else if (line.IndexOf("PCWSTR")>=0)
                        newlines.Add(line.Replace("PCWSTR", "wchar_t*"));
                    //                        throw new ArgumentException($"Problems:{line}");
                    else if (line.IndexOf("    LPCGUID ")>=0)
                        newlines.Add(line.Replace("    LPCGUID ", "    const GUID * "));
                    else
                        newlines.Add(line);
                    if(line.StartsWith("    ULONG status = "))
                        newlines.Add("#ifndef TARGET_UNIX");
                    if (line.StartsWith("    status &= "))
                        newlines.Add("#endif");
                    if (line.StartsWith("}"))
                    {
                        newlines.Add("");
                        break;
                    }
                }
            }
            orgList.InsertRange(index, newlines);
            File.WriteAllLines(destfileName, orgList);
        }
    }
}