using System.Collections.Generic;

namespace PostPythonChanges
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            string[] files = {
                @"C:\work\core\CurrentWork\runtime\artifacts\obj\coreclr\windows.x64.Debug\inc\clretwallmain.h",
                @"C:\work\core\CurrentWork\runtime\artifacts\obj\coreclr\windows.x64.Debug\inc\clreventpipewriteevents.h",
                @"C:\work\core\CurrentWork\runtime\artifacts\obj\coreclr\windows.x64.Debug\vm\eventing\eventpipe\eventpipe\dotnetruntime.cpp",
            };

            if(args.Length != 0) { files=args; }

            ChangePCWSTR(files);
            CheckForWNull(files);
            
            Console.WriteLine("Hello, World!");
        }

        private static void CheckForWNull(string[] files)
        {
            foreach (string file in files)
            {
                const string search = "W(\"";
                const string search2 = "W(\"NULL\")";
                bool warned = false;
                List<string> list = new List<string>();
                foreach (string line in File.ReadLines(file))
                {
                    if (line.IndexOf(search)>=0)
                    {
                        if (line.IndexOf(search2)>=0)
                        {
                            list.Add(line.Replace(search2, "L\"NULL\""));
                        }
                        else
                        {
                            if (!warned)
                                Console.WriteLine($"File:{file} requires further work");
                            warned = true;
                        }
                    }
                    else
                    {
                        list.Add(line);
                    }
                }
                File.WriteAllLines(file, list.ToArray());
                //                    File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file), string.Join(Path.GetFileNameWithoutExtension(file), "_1", Path.GetExtension(file))), list.ToArray());
            }
        }

        private static void ChangePCWSTR(string[] files)
        {
            const string search = "PCWSTR";
            foreach (string file in files)
            {
                List<string> list = new List<string>();
                foreach(string line in File.ReadLines(file))
                {
                    if(line.IndexOf(search)>=0)
                    {
                        list.Add(line.Replace(search, "const wchar_t*"));
                    }
                    else
                    {
                        list.Add(line);
                    }
                }
                File.WriteAllLines(file, list.ToArray());
            }
        }
    }
}