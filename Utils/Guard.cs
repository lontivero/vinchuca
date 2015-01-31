using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DreamBot.Utils
{
    static class Guard
    {
        [DebuggerStepThrough]
        public static void NotNull(object o, string paramName)
        {
            if(o == null) throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        public static void NotEmpty(string str, string paramName)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        public static void IsBeetwen(int val, int min, int max, string paramName)
        {
            if(val <min || val > max) throw new ArgumentOutOfRangeException(paramName);
        }

        public static void IsGreaterOrEqualTo(int val, int min, string paramName)
        {
            if (val < min) throw new ArgumentOutOfRangeException(paramName);
        }

        public static void ContainsKey<T,TQ>(IDictionary<T, TQ> dict, T key, string message)
        {
            if(!dict.ContainsKey(key)) throw new ArgumentException(message);
        }
    }
}
