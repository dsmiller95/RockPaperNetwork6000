using System;
using System.Collections.Generic;
using Unity.Netcode;

public static class NetworkVariableExtensions{

    public static IEnumerable<T> AsEnumerable<T>(this NetworkList<T> networkList)
    where T : unmanaged, IEquatable<T>
    {
        foreach (T item in networkList) yield return item;
    }
}