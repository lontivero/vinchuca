using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DreamBot.System;

namespace DreamBot.DGA
{
    internal class Dga
    {
        private static readonly MD5 Md5 = MD5.Create();

        private static readonly string[] Hld = new[] {
            "info", "com", "net",
            "biz", "xyz", "tk",
            "org", "cc.cc"
        };

        public void test()
        {
            foreach (var generateDomain in GenerateDomains2(DateTimeProvider.UtcNow))
            {
                Console.WriteLine(generateDomain);
            }        
        }

        private string[] verbos = new[] {"go", "drink", "let", "cook", "see", "be", "fly", "do", "be"};
        private string[] sustantivos = new[] {"home", "friend", "woman", "hotel", "country", "others"}; 
        internal IEnumerable<string> GenerateDomains2(DateTime date)
        {
            var x = new DateTime(2015, 1, 1);

            var days = (int)(date - x).TotalDays;
            var name = "";
            while(name.Length < 24)
            {
                name += Word(verbos, days);
                name += Word(sustantivos, days);
            }
            yield return name;
        }

        private string Word(string[] words, int days)
        {
            var len = words.Length;
            return words[days%len];
        }

        internal IEnumerable<string> GenerateDomains(DateTime date)
        {
            var s = new byte[5];
            ushort i = 0;
            while(i < ushort.MaxValue)
            {
                var name = new StringBuilder(64);
                var ixs = BitConverter.GetBytes(i);

                s[0] = (byte)((date.Year + 48) % 0xff);
                s[1] = (byte)date.Month;
                s[2] = (byte)(7 * (date.Day/7));
                s[3] = ixs[0];
                s[4] = ixs[1];
                var hash = Md5.ComputeHash(s);

                foreach (var b in hash)
                {
                    var c1 = (char)((b & 0x1F) + 'a');
                    var c2 = (char)((b / 8) + 'a');

                    if (c1 == c2) continue;
                    if (c1 <= 'z') name.Append(c1);
                    if (c2 <= 'z') name.Append(c2);
                }

                name.Append('.');
                name.Append(Hld[i%Hld.Length]);

                yield return name.ToString();
                i++;
            }
        }
    }
}
