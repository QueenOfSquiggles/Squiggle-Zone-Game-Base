using System;
using System.Collections.Generic;

namespace queen.extension;

public static class DictionaryExtensions
{

    public static void AddSafe<T, Y>(this Dictionary<T, Y> dictionary, T key, Y value)
    {
        if (dictionary.ContainsKey(key)) dictionary[key] = value;
        else dictionary.Add(key, value);
    }

}