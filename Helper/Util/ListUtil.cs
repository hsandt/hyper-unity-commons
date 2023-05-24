using System;
using System.Collections.Generic;

namespace CommonsHelper
{
    public static class ListUtil
    {
        // @mmx answer on
        // https://stackoverflow.com/questions/594518/is-there-a-lower-bound-function-on-a-sortedlistk-v
        private static int BinarySearch<T>(IList<T> list, T value)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            var comp = Comparer<T>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = (hi + lo) / 2; // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) lo = m + 1;
                else hi = m - 1;
            }

            if (comp.Compare(list[lo], value) < 0) lo++;
            return lo;
        }

        /// Return index of first element greater than or equal to value in list
        public static int GetLowerBound<T>(this IList<T> list, T value)
        {
            return BinarySearch(list, value);
        }
    }
}