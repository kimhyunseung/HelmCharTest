using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MakeMOCAAAccount
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc).AddMilliseconds((double)1594417291000).Ticks;
        }
    }
}
