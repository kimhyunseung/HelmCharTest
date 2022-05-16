using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbusingDetect
{
    class Program
    {
        private static readonly string SERVICE_CODE = "SVC042";
        private static readonly int SEARCH_DAY = 1;
        private static readonly string CSV_PATH = "G:\\download";

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            //몇일전까지 받을지, 하루 단위로 CSV파일 만들어짐
            for(var i = 1; i <= SEARCH_DAY; i++)
            {
                DateTime searchStartDatetime = DateTime.Today.AddDays(-i),
                        searchEndDatetime = DateTime.Today.AddDays(-(i-1));

                IEnumerable<LoginLogEntity> loginLogs = new List<LoginLogEntity>();
                IEnumerable<InitializeEntity> initializeLogs = new List<InitializeEntity>();
                IEnumerable<GameLogEntity> unconsumeLogs = new List<GameLogEntity>();

                sw.Start();

                #region Azure에서 데이터 가져오기

                Parallel.Invoke(() =>
                {
                    loginLogs = GetLoginLogList(searchStartDatetime, searchStartDatetime, searchEndDatetime, SERVICE_CODE);
                },
                () =>
                {
                    initializeLogs = GetInitLogList(searchStartDatetime, searchStartDatetime, searchEndDatetime, SERVICE_CODE);
                },
                () =>
                {
                    unconsumeLogs = GetUnconsumeLogList(searchStartDatetime, searchStartDatetime, searchEndDatetime, SERVICE_CODE);
                });

                sw.Stop();

                Console.WriteLine($"Complete get data from azure source, Time : {sw.ElapsedMilliseconds}");

                sw.Restart();

                #endregion

                #region 가져온 데이터 필요한 내용으로 바꾸기

                //초기화 호출 횟수
                var callCnt = (from item in initializeLogs
                               group item by item.UUID into g
                               select (
                                 UUID: g.Key,
                                 Count: g.Count())
                              ).ToList();

                //언컨슘 호출 횟수
                var unconsumeCallCnt = (from item in unconsumeLogs
                                        group item by item.UUID into g
                                        select (
                                          UUID: g.Key,
                                          Count: g.Count())
                                      ).ToList();

                //초기화시 UUID와 IP에 의한 호출 횟수
                var ipInitCallCnt = (from list in
                                        (from item in initializeLogs
                                         group item by new { UUID = item.UUID, ClientIP = item.ClientIP } into g
                                         select (
                                            UUID: g.Key.UUID,
                                            ClientIP: g.Key.ClientIP,
                                            Count: g.Count())
                                        )
                                     group list by list.UUID into grp
                                     select (
                                         UUID: grp.Key,
                                         Count: grp.Count()
                                     )).ToList();

                //UUID 별 게임 계정 번호 개수
                var uuidAccountCnt = (from item in (from item in loginLogs
                                                    group item by new { UUID = item.UUID, GameAccountNo = item.GameAccountNo } into g
                                                    select (
                                                      UUID: g.Key.UUID,
                                                      GameAccountNo: g.Key.GameAccountNo)
                                                    )
                                      group item by new { UUID = item.UUID } into g
                                      select (
                                          UUID: g.Key.UUID,
                                          AccountCount: g.Count()
                                      )
                                    ).ToList();

                //UUID별 게임 계정 번호로 로그인 성공 갯수 리스트
                var uuidAccountLoginCnt = (from item in loginLogs.Where(x => x.ReturnCode.Equals("1"))
                                   group item by new { UUID = item.UUID, GameAccountNo = item.GameAccountNo } into g
                                   select (
                                     UUID: g.Key.UUID,
                                     GameAccountNo: g.Key.GameAccountNo,
                                     Count : g.Count())
                                ).ToList();

                //UUID별 ADID 개수
                var adidUUIDCnt = (from list in
                                       (from item in unconsumeLogs
                                        where !string.IsNullOrWhiteSpace(item.UUID)
                                        group item by new { UUID = item.UUID, ADID = item.ADID } into g
                                        select (
                                             UUID: g.Key.UUID,
                                             ADID: g.Key.ADID,
                                             Count: g.Count())
                                        )
                                   group list by list.UUID into grp
                                   select (
                                        UUID: grp.Key,
                                        Count: grp.Count()
                                   ));

                #endregion

                //UUID당 카운트 종합
                var countSummaryByUUID = (from item in callCnt

                                          join ip in ipInitCallCnt on item.UUID equals ip.UUID into ipCnt
                                          from joinIP in ipCnt.DefaultIfEmpty()

                                          join unconsume in unconsumeCallCnt on item.UUID equals unconsume.UUID into unconsumeCnt
                                          from joinUnconsume in unconsumeCnt.DefaultIfEmpty()

                                          join account in uuidAccountCnt on item.UUID equals account.UUID into accountCnt
                                          from joinAccount in accountCnt.DefaultIfEmpty()

                                          join adid in adidUUIDCnt on item.UUID equals adid.UUID into adidCnt
                                          from joinAdid in adidCnt.DefaultIfEmpty()

                                          orderby item.UUID

                                          select new SummaryInfoObj
                                          {
                                              UUID = item.UUID,
                                              InitCallCount = item.Count,
                                              GameAccountNoCount = joinAccount.AccountCount,
                                              UnConsumeCount = joinUnconsume.Count,
                                              IPCount = joinIP.Count,
                                              ADIDCount = joinAdid.Count
                                          }).ToList();

                //UUID별 계정정보 및 ADID 개수
                var countAccountByUUID = (from item in callCnt

                                          join account in uuidAccountCnt on item.UUID equals account.UUID into accountCnt
                                          from joinAccount in accountCnt.DefaultIfEmpty()

                                          join login in uuidAccountLoginCnt on item.UUID equals login.UUID into loginCnt
                                          from joinLogin in loginCnt.DefaultIfEmpty()

                                          join adid in adidUUIDCnt on item.UUID equals adid.UUID into adidCnt
                                          from joinAdid in adidCnt.DefaultIfEmpty()

                                          where !string.IsNullOrWhiteSpace(joinLogin.GameAccountNo)

                                          orderby item.UUID

                                          select new SummaryInfoObj
                                          {
                                              UUID = item.UUID,
                                              UUIDCount = item.Count,
                                              GameAccountNo = joinLogin.GameAccountNo,
                                              GameAccountNoCount = joinAccount.AccountCount,
                                              ADIDCount = joinAdid.Count
                                          }).ToList();

                sw.Stop();
                Console.WriteLine($"Complete linq all task, Total elapsed millisecond : {sw.ElapsedMilliseconds}");
                sw.Restart();

                #region CSV파일로 만들기

                Parallel.Invoke(() =>
                {
                    SummaryCountWriteCSV(SERVICE_CODE, searchStartDatetime, searchEndDatetime, countSummaryByUUID);
                },
                () => {
                    SummaryAccountWriteCSV(SERVICE_CODE, searchStartDatetime, searchEndDatetime, countAccountByUUID);
                });

                sw.Stop();
                Console.WriteLine($"Complete all task, Total elapsed millisecond : {sw.ElapsedMilliseconds}");

                #endregion
                //Console.ReadLine();
            }
        }

        public static IEnumerable<LoginLogEntity> GetLoginLogList(DateTime tableDate, DateTime searchStartDatetime, DateTime searchEndDatetime, string serviceCode)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=http;AccountName=wmpkrstoragelive;AccountKey=DkCpnHMSgahKDEpSUaOTZTn/XtC5knVsY8LYjINwu+LhXugKeVEwUZTLr2hbi0PL612QnfMitb+42VfN3Qdhcw==;");
            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable tableReference = cloudTableClient.GetTableReference($"LoginLog{tableDate.ToString("yyyyMM")}");
            tableReference.CreateIfNotExistsAsync(null, null).Wait();
            Stopwatch sw = new Stopwatch();

            sw.Start();

            string filter1 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, searchStartDatetime);
            string filter2 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, searchEndDatetime);
            string filter3 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serviceCode);

            string filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
            filter = TableQuery.CombineFilters(filter3, TableOperators.And, filter);

            TableQuery<LoginLogEntity> query = new TableQuery<LoginLogEntity>().Where(filter);
            TableContinuationToken token = new TableContinuationToken();
            TableQuerySegment<LoginLogEntity> result = null;
            var items = new List<LoginLogEntity>();
            int i = 0;

            do
            {
                result = tableReference.ExecuteQuerySegmentedAsync(query, token).Result;
                token = result.ContinuationToken;

                items.AddRange(result.ToList());

                if (i != 0 && i == items.Count)
                    break;

                i = items.Count;
            } while (token != null);
            sw.Stop();
            
            Console.WriteLine($"GetLoginLogList elapsed millisecond : {sw.ElapsedMilliseconds}, Count : {items.Count}");

            sw.Reset();
            sw.Start();
            items = items.Where(x => !string.IsNullOrWhiteSpace(x.GameAccountNo) && !x.GameAccountNo.Equals("0")).ToList();
            sw.Stop();
            Console.WriteLine($"GetLoginLogList refactoring data elapsed millisecond : {sw.ElapsedMilliseconds}, Count : {items.Count}");

            return items;
        }

        public static IEnumerable<InitializeEntity> GetInitLogList(DateTime tableDate, DateTime searchStartDatetime, DateTime searchEndDatetime, string serviceCode)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=http;AccountName=wmpkrstoragelive;AccountKey=DkCpnHMSgahKDEpSUaOTZTn/XtC5knVsY8LYjINwu+LhXugKeVEwUZTLr2hbi0PL612QnfMitb+42VfN3Qdhcw==;");
            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable tableReference = cloudTableClient.GetTableReference($"Initialize{serviceCode}{tableDate.ToString("yyyyMM")}");
            tableReference.CreateIfNotExistsAsync(null, null).Wait();
            Stopwatch sw = new Stopwatch();

            sw.Start();

            string filter1 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, searchStartDatetime);
            string filter2 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, searchEndDatetime);
            string filter3 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serviceCode);

            string filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
            filter = TableQuery.CombineFilters(filter3, TableOperators.And, filter);

            TableQuery<InitializeEntity> query = new TableQuery<InitializeEntity>().Where(filter);
            TableContinuationToken token = new TableContinuationToken();
            TableQuerySegment<InitializeEntity> result = null;
            var items = new List<InitializeEntity>();
            int i = 0;

            do
            {
                result = tableReference.ExecuteQuerySegmentedAsync(query, token).Result;
                token = result.ContinuationToken;

                items.AddRange(result.ToList());

                if (i != 0 && i == items.Count)
                    break;

                i = items.Count;
            } while (token != null);
            sw.Stop();

            Console.WriteLine($"GetInitLogList elapsed millisecond : {sw.ElapsedMilliseconds}, Count : {items.Count}");

            return items;
        }

        public static IEnumerable<GameLogEntity> GetUnconsumeLogList(DateTime tableDate, DateTime searchStartDatetime, DateTime searchEndDatetime, string serviceCode)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=http;AccountName=wmpkrstoragelive;AccountKey=DkCpnHMSgahKDEpSUaOTZTn/XtC5knVsY8LYjINwu+LhXugKeVEwUZTLr2hbi0PL612QnfMitb+42VfN3Qdhcw==;");
            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable tableReference = cloudTableClient.GetTableReference($"UnconsumeLog{tableDate.ToString("yyyyMM")}");
            tableReference.CreateIfNotExistsAsync(null, null).Wait();
            Stopwatch sw = new Stopwatch();

            sw.Start();

            string filter1 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, searchStartDatetime);
            string filter2 = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, searchEndDatetime);
            string filter3 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serviceCode);

            string filter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
            filter = TableQuery.CombineFilters(filter3, TableOperators.And, filter);

            TableQuery<GameLogEntity> query = new TableQuery<GameLogEntity>().Where(filter);
            TableContinuationToken token = new TableContinuationToken();
            TableQuerySegment<GameLogEntity> result = null;
            var items = new List<GameLogEntity>();
            int i = 0;

            do
            {
                result = tableReference.ExecuteQuerySegmentedAsync(query, token).Result;
                token = result.ContinuationToken;

                items.AddRange(result.ToList());

                if (i != 0 && i == items.Count)
                    break;

                i = items.Count;
            } while (token != null);
            sw.Stop();

            Console.WriteLine($"GetUnconsumeLogList elapsed millisecond : {sw.ElapsedMilliseconds}, Count : {items.Count}");

            sw.Reset();
            sw.Start();
            items = items.Where(x => !x.Content.Contains("ios GameAccountNo[0]")).ToList();
            Parallel.ForEach(items.Where(x => string.IsNullOrWhiteSpace(x.GameAccountNo) || x.GameAccountNo.Equals("0")), (item) =>
            {
                item.GameAccountNo = item.Content.Substring(34, 8);
            });
            sw.Stop();
            Console.WriteLine($"GetUnconsumeLogList refactoring data elapsed millisecond : {sw.ElapsedMilliseconds}, Count : {items.Count}");

            return items;
        }

        public static void SummaryCountWriteCSV(string serviceCode, DateTime searchStartDate, DateTime searchEndDate, IEnumerable<SummaryInfoObj> model)
        {
            string fileName = $"ams_SummaryCount_{serviceCode}_{searchStartDate.ToString("yyyyMMdd")}_{searchEndDate.ToString("yyyyMMdd")}_{DateTime.Now.Ticks}.csv";
            if (!Directory.Exists(CSV_PATH))
                Directory.CreateDirectory(CSV_PATH);

            using (FileStream fs = new FileStream($"{CSV_PATH}\\{fileName}", FileMode.CreateNew, FileAccess.Write))
            {
                using(StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    string header = $"UUID,Count,AccountNoCount,UnConsumeCount,IPCount,ADIDCount";
                    sw.WriteLine(header);

                    foreach(var item in model)
                        sw.WriteLine($"{item.UUID},{item.InitCallCount},{item.GameAccountNoCount},{item.UnConsumeCount},{item.IPCount},{item.ADIDCount}");
                }
            }
        }

        public static void SummaryAccountWriteCSV(string serviceCode, DateTime searchStartDate, DateTime searchEndDate, IEnumerable<SummaryInfoObj> model)
        {
            string fileName = $"ams_SummaryAccount_{serviceCode}_{searchStartDate.ToString("yyyyMMdd")}_{searchEndDate.ToString("yyyyMMdd")}_{DateTime.Now.Ticks}.csv";
            if (!Directory.Exists(CSV_PATH))
                Directory.CreateDirectory(CSV_PATH);

            using (FileStream fs = new FileStream($"{CSV_PATH}\\{fileName}", FileMode.CreateNew, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    string header = $"UUID,AccountNo,AccountNoCount,ADIDCount";
                    sw.WriteLine(header);

                    foreach (var item in model)
                        sw.WriteLine($"{item.UUID},{item.GameAccountNo},{item.GameAccountNoCount},{item.ADIDCount}");
                }
            }
        }

        public class LoginLogEntity : TableEntity
        {
            public string LocalTime { get; set; }
            public string StoreType { get; set; }
            public string PartnerCode { get; set; }
            public string PartnerUserID { get; set; }
            public string PartnerUserToken { get; set; }
            public string UUID { get; set; }
            public string PnsType { get; set; }
            public string Country { get; set; }
            public string Language { get; set; }
            public string ClientIP { get; set; }
            public string AccessToken { get; set; }
            public string ReturnCode { get; set; }
            public string UserNo { get; set; }
            public string PinID { get; set; }
            public string GameAccountNo { get; set; }
            public string UserType { get; set; }
            public string AuthBlock { get; set; }
            public string BillingBlock { get; set; }
            public string Email { get; set; }
        }

        public class BillingChargeLogEntity : TableEntity
        {
            public string LocalTime { get; set; }
            public string ServiceCode { get; set; }
            public string OrderID { get; set; }
            public int ReturnCode { get; set; }
            public bool IsVerify { get; set; }
            public string ReceiptReturnCode { get; set; }
            public int GameAccountNo { get; set; }
            public string TransactionID { get; set; }
            public string StoreType { get; set; }
            public string Country { get; set; }
            public string Language { get; set; }
            public string CurrencyCode { get; set; }
            public string StoreProductID { get; set; }
            public string ReceiptData { get; set; }
            public string Signature { get; set; }
            public string ApprovalID { get; set; }
        }

        public class InitializeEntity : TableEntity
        {
            public string LocalTime { get; set; }
            public string ServiceCode { get; set; }
            public string UUID { get; set; }
            public string ClientIP { get; set; }
        }

        public class GameLogEntity : TableEntity
        {
            public string SessionID { get; set; }
            public string GameAccountNo { get; set; }
            public string UUID { get; set; }
            public string ADID { get; set; }
            public string LocalTime { get; set; }
            public string PrimaryCode { get; set; }
            public string SecondaryCode { get; set; }
            public string Content { get; set; }
            public string ClientIP { get; set; }
        }

        public class SummaryInfoObj
        {
            public string UUID { get; set; }
            public int InitCallCount { get; set; }
            public string GameAccountNo { get; set; }
            public int GameAccountNoCount { get; set; }
            public int UUIDCount { get; set; }
            public int UnConsumeCount { get; set; }
            public int IPCount { get; set; }
            public int ADIDCount { get; set; }
        }
    }
}