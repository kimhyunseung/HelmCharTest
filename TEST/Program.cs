using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            //string apiUrl = "https://alpha-kr-mapi.webzen.com//v2/identification/server/get-identification-policy-list";
            //string content = "{\"service_code\" : \"SVC001\"}";

            //var response = new HttpClient().PostAsync(apiUrl, content).Result.Content.ReadAsStringAsync().Result;
            //Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            //List<Dictionary<string, object>> listDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(dictionary["list"].ToString());

            string sort = string.Concat(
              "2000",
              "4wYSmxTe6wWs6WWWmWrPg8vX0wqRmwaC",
              "MYR",
              "1",
              "test",
              "hmac-sha256",
              "44C31E4B146D46419B8B554001275CD2",
              "https://alpha-kr-mapi.webzen.com/v2/billingonline/return/razer?tid=44C31E4B146D46419B8B554001275CDE",
              "v1");
            string key = "4bvpJcqB6btP6tttJtOmD8auxbNoJbAU";

            string hash;
            //Byte[] code = Encoding.UTF8.GetBytes(key);
            //using (HMACSHA256 hmac = new HMACSHA256(code))
            //{
            //    Byte[] hmBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(sort));
            //    hash = Encoding.UTF8.GetString(hmBytes);
            //}

            Byte[] code = new ASCIIEncoding().GetBytes(key);
            using (HMACSHA256 hmac = new HMACSHA256(code))
            {
                Byte[] hmBytes = hmac.ComputeHash(new ASCIIEncoding().GetBytes(sort));
                hash = BitConverter.ToString(hmBytes).Replace("-", "").ToLower();
                hash = ToHexString(hmBytes);
            }
        }

        private static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder();
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
