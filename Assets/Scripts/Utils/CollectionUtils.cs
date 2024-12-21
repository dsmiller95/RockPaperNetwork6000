using System;

namespace Utils
{
    public static class CollectionUtils
    {
        public static bool ElementsEqual<T>(this T[] a, T[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static int GetElementsHashCode<T>(this T[] array)
        {
            if (array.Length == 0)
            {
                return 1337;
            }

            var hash = new HashCode();
            foreach (T item in array) hash.Add(item);

            return hash.ToHashCode();
        }

        /// <summary>
        /// Fisher-Yates shuffle
        /// </summary>
        /// <param name="array"></param>
        /// <param name="rng"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShuffleInPlace<T>(this T[] array, Random rng)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int swapIndex = rng.Next(i, array.Length);
                (array[i], array[swapIndex]) = (array[swapIndex], array[i]);
            }
        }
    }
}