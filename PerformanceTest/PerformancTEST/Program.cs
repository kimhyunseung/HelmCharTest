using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformancTEST
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                using (var con = new SqlConnection("Persist Security Info=False;Data Source=10.251.61.110,26279;Initial Catalog=WZMobileContents;USER ID=BackofficeID;Password=cav#$45_^a"))
                {
                    con.Open();
                    using(var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "dbo.USP_BO_GetNoticeDetail_1";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ServiceCode", "SVC005");
                        cmd.Parameters.AddWithValue("@NoticeNo", 1980);

                        var result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
            sw.Stop();

            Console.WriteLine($"Second : {sw.ElapsedMilliseconds}");
            Console.ReadLine();
        }
    }
}
