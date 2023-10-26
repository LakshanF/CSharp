

using System.Text.RegularExpressions;

namespace WinFormHelpers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"C:\work\core\LakshanF\CSharp\src\aot\experiments\WinForms\net9\Warnings\WinFormsRepoILWarnings.txt";
            //ParseOnlyILWarnings(fileName);
            //BucketizeILWarnings(fileName);
            BuketizeILWarningMethods(fileName);
            Console.WriteLine("Hello, World!");
        }

        private static void BuketizeILWarningMethods(string fileName)
        {
            string[] regexTargets =
            {
                " generic argument does not satisfy ",
                " value stored in field ",
                " Using member ",
                " Method ",
                " in call to ",
                " Call to ",
                "  in ",
            };

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
                    targetNotMatched.Add(line);
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
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_1.txt"), targetNotMatched);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_Methods.txt"), targetCount.OrderByDescending(x => x.Value).Select(x => string.Join("###", x.Key, x.Value)));
        }

        private static void BucketizeILWarnings(string fileName)
        {
            //string usingMemberTarget = @"(?<= Using member ')[^']*(?=')";
            //  error IL2093: 
            string iLNumberPattern = @"IL(\d{4})";
            Dictionary<string, int> ilNumberCount = new Dictionary<string, int>();
            List<string> ilNumbersNotMatched = new List<string>();
            Match match;
            foreach (string line in File.ReadLines(fileName))
            {
                string iLNumber = null;
                match = Regex.Match(line, iLNumberPattern);
                if (match.Success)
                {
                    iLNumber = match.Value;
                    if (ilNumberCount.ContainsKey(iLNumber))
                    {
                        ilNumberCount[iLNumber]++;
                    }
                    else
                    {
                        ilNumberCount.Add(iLNumber, 1);
                    }
                }
                else
                {
                    ilNumbersNotMatched.Add(line);
                }
            }
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_1.txt"), ilNumbersNotMatched);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_Buckets.txt"), ilNumberCount.OrderByDescending(x=>x.Value).Select(x=>string.Join(",", x.Key, x.Value)));
        }

        private static void ParseOnlyILWarnings(string fileName)
        {
            List<string> ilWarnings = new List<string>();
            List<string> noIlWarnings = new List<string>();
            foreach(string line in File.ReadLines(fileName))
            {
                if (line.Contains(": error IL"))
                {
                    ilWarnings.Add(line);
                }
                else
                {
                    noIlWarnings.Add(line);
                }
            }
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_1.txt"), ilWarnings);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(fileName), "IL_2.txt"), noIlWarnings);
        }
    }
}
