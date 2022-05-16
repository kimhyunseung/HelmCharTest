using AESGCMTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var encText = "W1LkSs5LU3y1iTcrd7T3/Q==";
            var key = "PASSWORD_VALUE";
            //var result = Webzen.MobilePlatform.Security.AESDecrypt(encText, key);
            var result = aesbase64wrapper.decodeanddecrypt(enctext);
        }
    }
}
