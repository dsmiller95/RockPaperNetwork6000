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
    }
}