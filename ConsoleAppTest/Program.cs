using GoogleTranslateLib.Text;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ConsoleAppTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            #region TEST DOWNLOAD FILE

            //while (true)
            //{
            //    try
            //    {
            //        string url = "https://www.digikey.com/PartSearchBundles/resultsPageCSS";

            //        //using (WebClient webClient = new WebClient())
            //        //{
            //        //    Console.Clear();

            //        //    string useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";
            //        //    webClient.Headers.Add("user-agent", useragent);
            //        //    Stream st = webClient.OpenRead(url);
            //        //    Console.WriteLine(new StreamReader(st).ReadToEnd());
            //        //    webClient.Dispose();
            //        //}


            //        //var p = Process.Start(new ProcessStartInfo
            //        //{
            //        //    FileName = "curl.exe",
            //        //    Arguments = url,
            //        //    CreateNoWindow = true,
            //        //    RedirectStandardError = true,
            //        //    RedirectStandardOutput = true,
            //        //    UseShellExecute = false,
            //        //});
            //        //Console.WriteLine(p.StartInfo.Arguments);
            //        //p.WaitForExit(5000);
            //        //Console.WriteLine(p.StandardOutput.ReadToEnd());

            //        //Console.WriteLine(p.StandardError.ReadToEnd());

            //        var client = new RestClient(url);
            //        var request = new RestRequest(Method.GET);
            //        request.AddHeader("content-type", "text/css; charset=utf-8");
            //        request.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36");
            //        request.AddHeader("postman-token", "4da4fe8e-4958-74ee-6563-1326304221d7");
            //        request.AddCookie("i10c.bdddb", "c2-5fa3alXe8JvcKSWgY8yKR");
            //        request.AddHeader("cache-control", "no-cache");
            //        request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            //        IRestResponse response = client.Execute(request);
            //        Console.WriteLine(response.Content);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message + "\n" + ex.InnerException?.Message);
            //        Debug.WriteLine(ex.Message + "\n" + ex.InnerException?.Message);
            //    }
            //    Thread.Sleep(3000);
            //}
            #endregion

            #region TEST TRANSLATE

            //string input = File.ReadAllText("1.txt");

            //string nhay = "\"";
            //var pattern_href = $"<a(.*) (href=['{nhay}][^'{nhay}]*['{nhay}])";
            //pattern_href = @">([^<]+\b)<\/";
            //var dic = new Dictionary<string, string>();
            //var output = Regex.Replace(input, pattern_href, new MatchEvaluator(m =>
            //{
            //    var key = m.Groups[1].Value;
            //    key = HttpUtility.HtmlDecode(key);
            //    if (!dic.ContainsKey(key))
            //    {
            //        var tran = Translate.TranslateText(key).GetAwaiter().GetResult();
            //        dic.Add(key, tran.Text_out);

            //        //var r = GoogleTranslateLib.Translate.TranslateText(key).GetAwaiter().GetResult();
            //        //dic.Add(key, r.IsSuccess ? string.Join(" ", r.Result.sentences.Select(q => q.trans)) : key);

            //        var s = $"({tran.ResponseCode}) {key} => {dic[key]} ";
            //        Console.WriteLine(s);
            //        Debug.WriteLine(s);
            //    }
            //    return m.Value.Replace(key, dic[key]);
            //}));


            //File.WriteAllText("2.txt", output, Encoding.UTF8);
            //Process.Start("2.txt");

            #endregion

            #region TEST TK

            //Debug.Assert(Translate.xo(-1799063850, "+-3^+b+-f") == -214896509, "1");
            //Debug.Assert(Translate.xo(435245, "+-a^+6") == 452789581, "2");
            //Debug.Assert(Translate.xo(452789628, "+-a^+6") == 254721705, "3");
            //Debug.Assert(Translate.xo(254721784, "+-a^+6") == -922602365, "4");
            //Debug.Assert(Translate.xo(-922602333, "+-a^+6") == -762080351, "5");

            #endregion

            #region TEST ZO

            //var dic = new Dictionary<string, string>()
            //{
            //    {"Accessories | Audio Products | DigiKey","248994.356166" },
            //    {"English","190908.281176" },
            //    {"USD","12035.429287" },
            //    {"Automation and Control","570622.923418" },
            //    {"Industrial Equipment","551282.969366" },
            //    {"Pneumatics, Hydraulics","86883.520327" },
            //    {"Spiral Wrap, Expandable Sleeving","533337.950461" },
            //};

            //foreach (var item in dic)
            //{
            //    var tk = Translate.computed_tk(item.Key);
            //    Debug.WriteLine($"{tk == item.Value} \t{item} = {tk}");
            //}

            #endregion

            #region SQLite

            #endregion

            Console.WriteLine("DONE");
            Console.ReadKey();
        }
    }

    public class SQLiteHelper
    {
        public static string StringConnect { get => @"Data Source=dbToolPage.db;Version=3;"; } 

        public static void Execute()
        {

        }
    }
}
