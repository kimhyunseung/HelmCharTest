using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientReuse
{
    class Program
    {
        static void Main(string[] args)
        {
            var parllelList = new List<Task>();
            for(var i = 0; i < 10000; i++)
            {
                parllelList.Add(Task.Run(() =>
                {
                    new client().test();
                }));
            }

            Console.WriteLine("END");
            Console.ReadLine();
        }
    }

    public class client
    {
        private static readonly HttpClient httpClient;

        static client() => httpClient = new HttpClient();

        public void test()
        {
            try
            {
                var tmp = httpClient.GetAsync("https://naver.com").Result;
                Console.WriteLine(tmp.IsSuccessStatusCode);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
