using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace REPL.Debugging
{
    internal class Column<T>
    {
        public int Width;
        public string Title;
        public Func<T, object> m;
    }

    internal static class Dumper
    {
        [Conditional("DEBUG")]
        internal static void Dump<T>(IEnumerable<T> enumerable, IEnumerable<Column<T>> columns)
        {
            var fmt = "";
            var tit = new List<object>();
            var i = 0;
            foreach (var column in columns)
            {
                fmt += "{" + i + "," + column.Width + "}";
                tit.Add(column.Title);
                i++;
            }

            Console.WriteLine(fmt, tit.ToArray());
            foreach (var item in enumerable)
            {
                var l = new List<object>();
                foreach (var column in columns)
                {
                    l.Add(column.m(item));
                }
                Console.WriteLine(fmt, l.ToArray());
            }
        }
    }
}
