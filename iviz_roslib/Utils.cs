﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iviz.Msgs;
using Newtonsoft.Json;

namespace Iviz.Roslib
{
    /// <summary>
    /// Simple class that overrides the ToString() method to produce a JSON representation. 
    /// </summary>
    public abstract class JsonToString
    {
        public override string ToString()
        {
            return Utils.ToJsonString(this);
        }
    }

    public static class Utils
    {
        public static void ForEach<T>(this IEnumerable<T> ts, Action<T> action)
        {
            if (ts is null) { throw new ArgumentNullException(nameof(ts)); }

            foreach (T t in ts)
            {
                action(t);
            }
        }

        public static void ForEach<T>(this T[] ts, Action<T> action)
        {
            for (int i = 0; i < ts.Length; i++)
            {
                action(ts[i]);
            }
        }

        public static bool HasPrefix(this string check, string prefix)
        {
            if (check is null) { throw new ArgumentNullException(nameof(check)); }

            if (prefix is null) { throw new ArgumentNullException(nameof(prefix)); }

            if (check.Length < prefix.Length)
            {
                return false;
            }

            for (int i = 0; i < prefix.Length; i++)
            {
                if (check[i] != prefix[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasSuffix(this string check, string suffix)
        {
            if (check is null) { throw new ArgumentNullException(nameof(check)); }

            if (suffix is null) { throw new ArgumentNullException(nameof(suffix)); }

            if (check.Length < suffix.Length)
            {
                return false;
            }

            int offset = check.Length - suffix.Length;
            for (int i = 0; i < suffix.Length; i++)
            {
                if (check[offset + i] != suffix[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void PrintBuffer(byte[] bytes, int start, int size)
        {
            for (int i = 0; i < size; i++)
            {
                Logger.Log($"[{i}]: {(int) bytes[start + i]} --> {(char) bytes[start + i]}");
            }
        }

        public static string ToJsonString(this ISerializable o)
        {
            return ToJsonString((object) o);
        }

        public static string ToJsonString(this IService o)
        {
            return ToJsonString((object) o);
        }

        public static string ToJsonString(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> t)
        {
            return new ReadOnlyCollection<T>(t);
        }
        
        public static bool RemovePair<T, U>(this ConcurrentDictionary<T, U> dictionary, T t, U u)
        {
            return ((ICollection<KeyValuePair<T, U>>) dictionary).Remove(new KeyValuePair<T, U>(t, u));
        }
    }
}