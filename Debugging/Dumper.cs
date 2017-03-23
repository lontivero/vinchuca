using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vinchuca.REPL;
using Vinchuca.Utils;

namespace Vinchuca.Debugging
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
        internal static void Dump<T>(VirtualConsole console, IEnumerable<T> enumerable, IEnumerable<Column<T>> columns)
        {
            var sep = "+";
            var fmt = "|";
            var tit = new List<object>();
            var i = 0;
            foreach (var column in columns)
            {
                sep += new string('-', Math.Abs(column.Width)) + "+";
                fmt += "{" + i + "," + column.Width + "}|";
                tit.Add(column.Title);
                i++;
            }

            console.WriteLine(sep);
            console.WriteLine(string.Format(fmt, tit.ToArray()));
            console.WriteLine(sep);
            foreach (var item in enumerable)
            {
                var l = new List<object>();
                foreach (var column in columns)
                {
                    l.Add(column.m(item));
                }
                console.WriteLine(string.Format(fmt, l.ToArray()));
            }
            console.WriteLine(sep);
        }
    }
}
