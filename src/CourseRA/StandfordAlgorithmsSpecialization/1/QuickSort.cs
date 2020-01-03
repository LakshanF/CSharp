using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePerDay.Algorithms_1
{
    class CourseRaQuickSort
    {
        //E:\Work\learn\QuickSort.txt 
        public static int RunAlgorithm(string[] args)
        {
            CourseRaQuickSort algo = new CourseRaQuickSort();
            algo.QuickSortManager(args);
            return -1;
        }

        /// <summary>
        /// Your task is to compute the total number of comparisons used to sort the given input file by QuickSort. As you know, the number of comparisons depends on which elements are chosen as pivots, 
        /// so we'll ask you to explore three different pivoting rules.
        /// 
        /// You should not count comparisons one-by-one.Rather, when there is a recursive call on a subarray of length m, you should simply add m−1 to your running total of comparisons. 
        /// (This is because the pivot element is compared to each of the other m−1 elements in the subarray in this recursive call.)
        /// 
        /// WARNING: The Partition subroutine can be implemented in several different ways, and different implementations can give you differing numbers of comparisons.For this problem, 
        /// you should implement the Partition subroutine exactly as it is described in the video lectures (otherwise you might get the wrong answer). 
        /// 
        /// DIRECTIONS FOR THIS PROBLEM: 
        /// 1) For the first part of the programming assignment, you should always use the first element of the array as the pivot element.
        /// 2) Compute the number of comparisons (as in Problem 1), always using the final element of the given array as the pivot element. 
        /// Again, be sure to implement the Partition subroutine exactly as it is described in the video lectures. Recall from the lectures that, 
        /// just before the main Partition subroutine, you should exchange the pivot element (i.e., the last element) with the first element.
        /// 3) Compute the number of comparisons (as in Problem 1), using the "median-of-three" pivot rule. [The primary motivation behind this rule is to do a little bit of extra work to get much better 
        /// performance on input arrays that are nearly sorted or reverse sorted.] In more detail, you should choose the pivot as follows. Consider the first, middle, and final elements of the given array. 
        /// (If the array has odd length it should be clear what the "middle" element is; for an array with even length 2k, use the kth element as the "middle" element. 
        /// So for the array 4 5 6 7, the "middle" element is the second one ---- 5 and not 6!) Identify which of these three elements is the median (i.e., the one whose value is in between the other two), 
        /// and use this as your pivot. As discussed in the first and second parts of this programming assignment, be sure to implement Partition exactly as described in the video lectures 
        /// (including exchanging the pivot element with the first element just before the main Partition subroutine). 
        /// EXAMPLE: For the input array 8 2 4 5 7 1 you would consider the first(8), middle(4), and last(1) elements; since 4 is the median of the set {1,4,8}, you would use 4 as your pivot element.
        /// SUBTLE POINT: A careful analysis would keep track of the comparisons made in identifying the median of the three candidate elements.You should NOT do this. 
        /// That is, as in the previous two problems, you should simply add m−1 to your running total of comparisons every time you recurse on a subarray with length m
        /// 
        /// Algorithms (from slides)
        /// QuickSort(A, p, r)
        ///  if r-p<=1
        ///     return
        ///  q=Partition(A, p, r)
        ///  QuickSort(A, p, q)
        ///  QuickSort(A, q+1, r)
        /// 
        /// Partition(A, p, 
        /// </summary>
        /// <param name="args"></param>
        static int m_comparisonCount = 0;
        static int m_individualComparisonCount = 0;
        private void QuickSortManager(string[] args)
        {
            //List<int> values = new List<int>(new int[] { 2, 6, 4, 5, 1, 3, 8, 7 });
            List<int> values = Utility.GetValues(args[0]);
            int[] aValues = values.ToArray();

            bool result = Utility.ValidateArray(aValues, false);
            QuickSort(aValues, 0, aValues.Length - 1);
            Debug.Assert(Utility.ValidateArray(aValues, false), "Array not sorted");
            Console.WriteLine("{0} - {1}", m_comparisonCount, m_individualComparisonCount);
        }

        /// <summary>
        /// Class Pseudocode
        ///  - if length==1
        ///     return
        ///  - Pick a pivot
        ///  - Partition A around pivot
        ///  - recursively sort 1st part
        ///  - recursively sort the 2nd part
        /// 
        ///  Note that the above algorithm doesn't include the pivot for subsequent recursive calls (since its sorted at the right place)
        /// 
        /// </summary>
        /// <param name="aValues"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void QuickSort(int[] aValues, int l, int r)
        {
            if ((r - l) < 1)
                return;
            ChoosePivot(aValues, l, r, PivotChoice.Median);
            int q = NewPartition(aValues, l, r);
            QuickSort(aValues, l, q - 1);
            QuickSort(aValues, q + 1, r);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aValues"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private int NewPartition(int[] aValues, int l, int r)
        {
            m_comparisonCount += r - l;
            int pivot = aValues[l], temp, pivotIndex;
            int i = l + 1;
            for (int j = l + 1; j <= r; j++)
            {
                m_individualComparisonCount++;
                if (aValues[j] < pivot)
                {
                    temp = aValues[j];
                    aValues[j] = aValues[i];
                    aValues[i] = temp;
                    i++;
                }
            }

            pivotIndex = i - 1;
            temp = aValues[pivotIndex];
            aValues[pivotIndex] = pivot;
            aValues[l] = temp;
            return pivotIndex;
        }



        /// <summary>
        /// Will pick a pivot and put it in the first element, swapping if needed
        /// </summary>
        /// <param name="aValues"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private void ChoosePivot(int[] aValues, int l, int r, PivotChoice choice)
        {

            switch (choice)
            {
                case PivotChoice.First:
                    {
                        //Take 1 - pivot is the first element
                        break;
                    }
                case PivotChoice.Last:
                    {
                        //Take 2 - pivot is the last element
                        int temp = aValues[r];
                        aValues[r] = aValues[l];
                        aValues[l] = temp;
                        break;
                    }
                case PivotChoice.Median:
                    {
                        //Take 3 - pivot is the median element from first, last and middle
                        if (l == r)
                            return;
                        Debug.Assert(r > l, "Issue with ChoosePivot call");
                        int middleElement = (r - l) / 2;


                        int[] pivotArray = new int[] { aValues[l], aValues[l + middleElement], aValues[r] };

                        int temp, min, median;
                        for (int i = 0; i < pivotArray.Length; i++)
                        {
                            min = i;
                            for (int j = i + 1; j < pivotArray.Length; j++)
                            {
                                if (pivotArray[j] < pivotArray[min])
                                    min = j;
                            }
                            if (min != i)
                            {
                                temp = pivotArray[min];
                                pivotArray[min] = pivotArray[i];
                                pivotArray[i] = temp;
                            }
                        }
                        median = pivotArray[1];

                        if (aValues[l] != median)
                        {
                            if (median == aValues[r])
                            {
                                temp = aValues[l];
                                aValues[l] = aValues[r];
                                aValues[r] = temp;
                            }
                            else
                            {
                                Debug.Assert((median == aValues[l + middleElement]), "Problem with median");
                                temp = aValues[l];
                                aValues[l] = aValues[l + middleElement];
                                aValues[l + middleElement] = temp;
                            }
                        }
                        break;
                    }
            }
        }
    }

    public enum PivotChoice
    {
        First,
        Last,
        Median
    }
}
