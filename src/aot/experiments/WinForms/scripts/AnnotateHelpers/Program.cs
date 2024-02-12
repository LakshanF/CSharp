
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AnnotateHelpers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            switch (args[0])
            {
                case "PrepWork":
                    PrepWork();
                    break;
                case "PostFormatProcessing":
                    PostFormatProcessing();
                    break;
                case "RemoveRUC":
                    FixRUC(args[1]);
                    break;
                case "ArchiveFiles":
                    ArchiveFiles();
                    break;
                default:
                    Console.WriteLine("Invalid command");
                    break;
            }

            Console.WriteLine("Bye, World!");
        }

        private static void FixRUC(string file)
        {
            if (!File.Exists(file))
                throw new ArgumentException($"{file} not found");
            List<string> list = new List<string>();
            bool changedFile = false;
            foreach (string line in File.ReadLines(file))
            {
                if (line.Contains("[RequiresUnreferencedCode()]"))
                {
                    changedFile = true;
                    list.Add(line.Replace("[RequiresUnreferencedCode()]", "[RequiresUnreferencedCode(\"\")]"));
                }
                else
                    list.Add(line);
            }
            if (changedFile)
                File.WriteAllLines(file, list);
        }



        private static void PostFormatProcessing()
        {
            // Clean up the build file
            string file = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\CurrentWarnings.txt";
            string previousFile = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\PreviousWarnings.txt";

            HashSet<string> uniqueLines = new HashSet<string>();
            foreach (string line in File.ReadLines(file))
            {
                if (line.Contains(": error "))
                {
                    uniqueLines.Add(line);
                }
            }
            string replaceStr = @"C:\work\core\CurrentWork\winforms\";
            List<string> newLines = new List<string>();
            foreach (string line in uniqueLines)
            {
                newLines.Add(line.Replace(replaceStr, ""));                    
            }
            File.WriteAllLines(file, newLines);

            // Find the unqiue values from the previous file
            var current = File.ReadLines(file);
            var previous = File.ReadLines(previousFile);
            var unique = current.Except(previous);

            //bucketize the unique values
            Dictionary<string, int> fileMap = BucketizeCSFiles(unique);
            Dictionary<string, int> methodMap = BuketizeILWarningMethods(unique);

            File.WriteAllLines(@"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\unique_file_summary.txt", fileMap.OrderByDescending(x => x.Value).Select(x => string.Join("###", x.Key, x.Value)));
            File.WriteAllLines(@"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\unique.txt", unique);
            File.WriteAllLines(@"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\unique_method_summary.txt", methodMap.OrderByDescending(x => x.Value).Select(x => string.Join("###", x.Key, x.Value)));

            // Generate the script
            GenerateScriptFilesForAnalyzers();
        }

        private static void ArchiveFiles()
        {
            string sourceDirectory = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working";
            string archiveDir = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Archived";
            string date = DateTime.Now.ToString("yyyy-MM-dd");

            string dateDir = Path.Combine(archiveDir, date);
            if (!Directory.Exists(dateDir))
                Directory.CreateDirectory(dateDir);

            string timeDir = Path.Combine(dateDir, DateTime.Now.ToString("HH-mm"));
            if (!Directory.Exists(timeDir))
                Directory.CreateDirectory(timeDir);

            Directory.GetFiles(sourceDirectory)
                .ToList()
                .ForEach(file => File.Copy(file, Path.Combine(timeDir, Path.GetFileName(file))));

            // delete the previous file, and rename current file to previous file
            string fileToProcess = Path.Combine(sourceDirectory, "PreviousWarnings.txt");
            if (File.Exists(fileToProcess))
                File.Delete(fileToProcess);
            fileToProcess = Path.Combine(sourceDirectory, "CurrentWarnings.txt");
            if (File.Exists(fileToProcess))
                File.Move(fileToProcess, Path.Combine(sourceDirectory, "PreviousWarnings.txt"));

            string[] filesToProcess = new string[] { "unique_file_summary.txt", "unique.txt", "unique_method_summary.txt", 
                "ProcessedFiles.txt", "AnnotationNotes.txt", "not_matched_method_summary.txt" };
            foreach (string file in filesToProcess)
            {
                fileToProcess = Path.Combine(sourceDirectory, file);
                if (File.Exists(fileToProcess))
                    File.Delete(fileToProcess);
            }
        }

        private static void GenerateScriptFilesForAnalyzers()
        {
            //Look at the unique_file_summary.txt and unique.txt to find the path to the file
            string filesToProcess = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\unique_file_summary.txt";
            string fileToGetInfo = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\unique.txt";
            List<string> files = File.ReadLines(filesToProcess).Select(s => s.Trim()).Select(s => s.Split("###")[0]).ToList();
            HashSet<string> map = File.ReadLines(fileToGetInfo).Select(x => x.Substring(0, x.IndexOf('('))).ToHashSet();

            //@TODO: easier way to do this
            Dictionary<String, string> dic = new Dictionary<String, string>();
            foreach (string file in files)
            {
                string dic_value = "";
                foreach (string v in map)
                {
                    if (v.Contains(file))
                        dic_value = v;
                }
                Debug.Assert(!string.IsNullOrEmpty(dic_value));
                dic.Add(file, dic_value);
            }

            List<string> outFile = new List<string>();
            foreach (string file in files)
            {
                outFile.Add(@$"call c:\dotnet\dotnet.exe format -v diag --include {dic[file]}");
                outFile.Add(@$"call C:\Users\lakshanf\source\repos\AnnotateHelpers\AnnotateHelpers\bin\Debug\net9.0\AnnotateHelpers.exe  RemoveRUC {dic[file]}");
                outFile.Add(@$"echo {dic[file]} >> C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\ProcessedFiles.txt");
                outFile.Add("");
            }
            File.WriteAllLines(@"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\RunScript.bat", outFile);
        }


        private static Dictionary<string, int> BuketizeILWarningMethods(IEnumerable<string> lines)
        {
            string[] regexTargets =
            {
                " generic argument does not satisfy ",
                " value stored in field ",
                " Using member ",
                " P/invoke method ",
                " Method ",
                " in call to ",
                " Call to ",
                "  in ",
            };

            string hardCodedTypeGetType = @" Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType";

            // @"(?<= Using member ')[^']*(?=')"
            List<string> regexTargetList = regexTargets.Select(x => @"(?<=" + x + "')[^']*(?=')").ToList();
            Dictionary<string, int> targetCount = new Dictionary<string, int>();
            List<string> targetNotMatched = new List<string>();
            Match match;
            foreach (string line in lines)
            {
                string target = "[NOT_MATCHED]";
                foreach (string regexPattern in regexTargetList)
                {
                    match = Regex.Match(line, regexPattern);
                    if (match.Success)
                    {
                        target = match.Value;
                        break;
                    }
                }

                if (target.Equals("[NOT_MATCHED]"))
                {
                    if (line.IndexOf(hardCodedTypeGetType) >= 0)
                    {
                        target = hardCodedTypeGetType;
                    }
                    else
                    {
                        targetNotMatched.Add(line);
                    }
                }
                if (targetCount.ContainsKey(target))
                {
                    targetCount[target]++;
                }
                else
                {
                    targetCount.Add(target, 1);
                }

            }

            string unmathcedFileName = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\Annotation\Working\not_matched_method_summary.txt";
            if(File.Exists(unmathcedFileName))
                File.Delete(unmathcedFileName);
            if(targetNotMatched.Count > 0)
                File.WriteAllLines(unmathcedFileName, targetNotMatched);

            return targetCount;
        }


        private static Dictionary<string, int> BucketizeCSFiles(IEnumerable<string> lines)
        {
            //string fileName = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\net9\WinForms\Warnings\unique_11_12.txt";
            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (string line in lines)
            {
                if (line.Contains(".cs("))
                {
                    string pattern = @"([^\\]+)\(\d+,\d+\):";

                    // Find the first match
                    Match match = Regex.Match(line, pattern);

                    if (match.Success)
                    {
                        string filePath = match.Value;
                        filePath = Path.GetFileName(filePath).Substring(0, Path.GetFileName(filePath).IndexOf('('));
                        if (map.ContainsKey(filePath))
                            map[filePath] += 1;
                        else
                            map[filePath] = 1;

                    }
                    else
                    {
                        Debug.Assert(true);
                    }
                }
            }
            return map;
        }


        private static void PrepWork()
        {
            //ReplaceFixer();

            //BucketizeCSFiles2();
            //BuketizeILWarningMethods();
        }

        private static void ReplaceFixer()
        {
            string replaceFile = @"C:\work\core\CurrentWork\runtime\artifacts\bin\ILLink.CodeFixProvider\Debug\netstandard2.0\ILLink.CodeFixProvider.dll";
            string file = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\net9\MinimalRepro\Annotation\ILLink.CodeFixProvider.txt";
            foreach(string line in File.ReadAllLines(file))
            {
                if(File.Exists(line))
                {
                    string newFile = line.Replace(".dll", ".org");
                    File.Move(line, newFile);
                    File.Copy(replaceFile, line);
                }
            }
        }

        private static void BucketizeCSFiles2()
        {
            string fileName = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\net9\MinimalRepro\Annotation\Working\wrn_2026.txt";
            //string trimType
            List<string> files = new List<string>();
            Dictionary<string, int> map = new Dictionary<string, int>();
            const string NoFile = "[NO_FILE]";
            foreach (string line in File.ReadLines(fileName))
            {
                if (line.Contains(".cs("))
                {
                    string pattern = @"([^\\]+)\(\d+,\d+\):";

                    // Find the first match
                    Match match = Regex.Match(line, pattern);

                    if (match.Success)
                    {
                        string filePath = match.Value;
                        filePath = Path.GetFileName(filePath).Substring(0, Path.GetFileName(filePath).IndexOf('('));
                        if (map.ContainsKey(filePath))
                            map[filePath] += 1;
                        else
                            map[filePath] = 1;

                    }
                    else
                    {
                        Debug.Assert(true);
                        //Console.WriteLine("No file path found in the input string.");
                    }
                }
            }
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), $"wrn_2026_files.txt"), map.OrderByDescending(x => x.Value).Select(x => string.Join("###", x.Key, x.Value)));
        }

        private static void BuketizeILWarningMethods()
        {
            string fileName = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\net9\MinimalRepro\Annotation\Working\wrn_2026.txt";
            string[] regexTargets =
            {
                " generic argument does not satisfy ",
                " value stored in field ",
                " Using member ",
                " P/invoke method ",
                " Method ",
                " in call to ",
                " Call to ",
                "  in ",
            };

            string hardCodedTypeGetType = @" Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType";

            // @"(?<= Using member ')[^']*(?=')"
            List<string> regexTargetList = regexTargets.Select(x => @"(?<=" + x + "')[^']*(?=')").ToList();
            Dictionary<string, int> targetCount = new Dictionary<string, int>();
            List<string> targetNotMatched = new List<string>();
            Match match;
            foreach (string line in File.ReadLines(fileName))
            {
                string target = "[NOT_MATCHED]";
                foreach (string regexPattern in regexTargetList)
                {
                    match = Regex.Match(line, regexPattern);
                    if (match.Success)
                    {
                        target = match.Value;
                        break;
                    }
                }

                if (target.Equals("[NOT_MATCHED]"))
                {
                    if (line.IndexOf(hardCodedTypeGetType) >= 0)
                    {
                        target = hardCodedTypeGetType;
                    }
                    else
                    {
                        targetNotMatched.Add(line);
                    }
                }
                if (targetCount.ContainsKey(target))
                {
                    targetCount[target]++;
                }
                else
                {
                    targetCount.Add(target, 1);
                }

            }
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), $"IL_Methods_2.txt"), targetNotMatched);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), $"IL_Methods.txt"), targetCount.OrderByDescending(x => x.Value).Select(x => string.Join("###", x.Key, x.Value)));
        }


    }
}
