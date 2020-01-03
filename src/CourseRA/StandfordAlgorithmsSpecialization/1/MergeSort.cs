using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePerDay.Algorithms_1
{
    /// <summary>
    /// This file contains all of the 100,000 integers between 1 and 100,000 (inclusive) in some order, with no integer repeated.
    /// Your task is to compute the number of inversions in the file given, where the ith row of the file indicates the ith entry of an array.
    /// Because of the large size of this array, you should implement the fast divide-and-conquer algorithm covered in the video lectures.
    /// 
    /// Approach
    ///  - Brute Force O(n^2)
    ///  - Looks like there is a (nlogn) solution
    /// </summary>
    public class CourseRaAlgorithmSorting
    {
        public static int RunAlgorithm(string[] args)
        {
            CourseRaAlgorithmSorting algo = new CourseRaAlgorithmSorting();
            long value = algo.InvesrsionBruteForce(args);
            //algo.MergeSortManager(args);
            return -1;
        }

        private void MergeSortManager(string[] args)
        {
            //List<int> values = new List<int>(new int[] { 2, 6, 4, 5, 1, 3, 8, 7 });
            List<int> values = Utility.GetValues(args[0]);
            int[] aValues = values.ToArray();

            bool result = Utility.ValidateArray(aValues, false);
            MergeSort(aValues, 0, aValues.Length-1);
            result = Utility.ValidateArray(aValues, false);
        }

        private void MergeSort(int[] aValues, int a, int b)
        {
            if (a >= b)
                return;
            int p = (a + b) / 2;
            MergeSort(aValues, a, p);
            MergeSort(aValues, p + 1, b);
            Merge(aValues, a, p, b);
        }

        /// <summary>
        /// Merges an array with already 2 sorted sequences, a...p and p+1...b, into 1 sorted sequence, a...b
        /// </summary>
        /// <param name="aValues"></param>
        /// <param name="a"></param>
        /// <param name="p"></param>
        /// <param name="b"></param>
        private void Merge(int[] aValues, int a, int p, int b)
        {
            if (a > b)
                return;
            int[] aCopy = new int[b - a + 1];
            int i = a, j = p + 1, c=0;
            while(i <= p && j <= b)
            {
                if (aValues[i] > aValues[j])
                {
                    aCopy[c++] = aValues[j++];
                }
                else
                {
                    aCopy[c++] = aValues[i++];
                }
            }
            while(i <= p)
            {
                aCopy[c++] = aValues[i++];
            }
            while(j <= b)
            {
                aCopy[c++] = aValues[j++];
            }
            for(i = a, c=0; i <= b; i++, c++)
            {
                aValues[i] = aCopy[c];
            }

        }

        long InvesrsionBruteForce(string[] args)
        {
            int value;
            long noOfInversions = 0;
            List<int> values = Utility.GetValues(args[0]);
            for (int i = 0; i < values.Count; i++)
            {
                if (i % 1000 == 0)
                    Console.Write(".");
                value = values[i];
                for (int j = i + 1; j < values.Count; j++)
                {
                    if (value > values[j])
                        noOfInversions++;
                }
            }
            Console.WriteLine();
            Console.WriteLine(noOfInversions);
            return noOfInversions;
        }
    }
}
