using System;
using System.Collections.Generic;

namespace HyperUnityCommons
{
    public static class ListUtil
    {
        /// Helper method for GetLowerBound and GetUpperBound
        /// Return index of first element verifying isListValueVsValueComparisonSignGood(defaultCompare(element, value))
        /// in sorted list, i.e. element >= value for lower bound, element > value for upper bound,
        /// If there is no such element, i.e. isListValueVsValueComparisonSignGood never returns true,
        /// then the list count is returned (this includes the case where list is empty).
        /// It uses binary search, so complexity is O(log(list.Count))
        /// UB unless list is sorted with T's default comparer
        private static int BinarySearch<T>(IList<T> list, T value, Func<int, bool> isListValueVsValueComparisonSignGoodCallback)
        {
            // Implementation based on @mmx answer on
            // https://stackoverflow.com/questions/594518/is-there-a-lower-bound-function-on-a-sortedlistk-v
            // Changes by hsandt:
            // - adapted with a callback to be reused for both GetLowerBoundIndex and GetUpperBoundIndex
            // - added an additional check for list.Count == 0 to follow the "no lower/upper bound" => return list.Count
            // convention completely

            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (list.Count == 0)
            {
                // No lower bound, so return list count i.e. 0
                return 0;
            }

            var comp = Comparer<T>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = (hi + lo) / 2; // this might overflow; be careful.
                if (!isListValueVsValueComparisonSignGoodCallback(comp.Compare(list[m], value))) lo = m + 1;
                else hi = m - 1;
            }

            // If value is greater than lower index `lo` at this point, return the index just above
            // This includes the case where we still haven't found a lower/upper bound, i.e. the searched value
            // is greater than / greater than or equal to all the list values, in which case the increment will lead to
            // lo = list.Count as expected
            if (!isListValueVsValueComparisonSignGoodCallback(comp.Compare(list[lo], value))) lo++;
            return lo;
        }

        private static bool IsListValueVsValueComparisonSignPositiveOrZero(int listValue)
        {
            // This means that list value is greater than or equal to reference value
            return listValue >= 0;
        }

        /// Return index of first element greater than or equal to value in sorted list
        /// If there is no such element, i.e. value is greater than all the elements in the list,
        /// then the list count is returned (this includes the case where list is empty).
        /// It uses binary search, so complexity is O(log(list.Count))
        /// UB unless list is already sorted with T's default comparer
        public static int GetLowerBoundIndex<T>(this IList<T> list, T value)
        {
            return BinarySearch(list, value, IsListValueVsValueComparisonSignPositiveOrZero);
        }

        private static bool IsListValueVsValueComparisonSignPositive(int listValue)
        {
            // This means that list value is greater than reference value
            return listValue > 0;
        }

        /// Return index of first element greater than value in sorted list
        /// If there is no such element, i.e. value is greater than all the elements in the list,
        /// then the list count is returned (this includes the case where list is empty).
        /// It uses binary search, so complexity is O(log(list.Count))
        /// UB unless list is already sorted with T's default comparer
        public static int GetUpperBoundIndex<T>(this IList<T> list, T value)
        {
            return BinarySearch(list, value, IsListValueVsValueComparisonSignPositive);
        }
    }
}
