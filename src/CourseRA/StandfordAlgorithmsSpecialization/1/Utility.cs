using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePerDay.Algorithms_1
{
    public static class Utility
    {
        public static bool ValidateArray(int[] aValues, bool display)
        {
            if (display)
            {
                foreach (int value in aValues)
                {
                    Console.Write("{0} ", value);
                }
                Console.WriteLine();
            }
            for (int i = 0; i < aValues.Length - 1; i++)
            {
                if (aValues[i] > aValues[i + 1])
                    return false;
            }
            return true;
        }


        public static List<int> GetValues(string fileName)
        {
            int value;
            List<int> values = new List<int>();
            foreach (String line in File.ReadLines(fileName))
            {
                if (Int32.TryParse(line, out value))
                {
                    values.Add(value);
                }
            }
            return values;
        }

    }
}
