using System;
using System.IO;
using System.Text;

namespace DecryptEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sr = new StreamReader("G:\\Download\\20201116_동일한 UUID 보유 계정 리스트_before.csv"))
            {
                var reader = sr.ReadToEnd();
                var line = reader.Split(Environment.NewLine);
                var output = new StringBuilder();

                int idx = 0;
                foreach(var item in line)
                {
                    if (idx++ == 0)
                    {
                        output.Append(item);
                        output.Append(Environment.NewLine);
                        continue;
                    }

                    var contents = item.Split(',');
                    if(contents.Length >= 5)
                    {
                        contents[4] = string.IsNullOrWhiteSpace(contents[4]) ? "" : Webzen.MobilePlatform.Standard.Security.AESDecrypt(contents[4], "WEBZEN-MOBILE-PLATFORM-V2");
                        output.Append(string.Join(",", contents));
                        output.Append(Environment.NewLine);
                    }
                }

                using(var sw = new StreamWriter("G:\\Download\\20201116_동일한 UUID 보유 계정 리스트_after.csv"))
                {
                    sw.Write(output.ToString());
                }
            }
        }
    }
}
