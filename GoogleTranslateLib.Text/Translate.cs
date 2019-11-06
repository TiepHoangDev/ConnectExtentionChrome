using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace GoogleTranslateLib.Text
{
    public class Translate
    {
        public static async Task<TranslateResult> TranslateText(string input, lang lang_in = lang.en, lang lang_out = lang.vi,
            string TemplateRequest = "https://translate.google.com/translate_a/single?client=webapp&sl={lang_in}&tl={lang_out}&hl=vi&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&pc=1&otf=1&ssel=3&tsel=3&kc=1&tk={tk}&q={input}")
        {
            var r = new TranslateResult
            {
                IsSuccess = false,
                lang_in = lang_in,
                lang_out = lang_out,
                Text_in = input,
                Text_out = input,
                ResponseCode = null
            };
            try
            {
                string tk = computed_tk(input);
                string url = TemplateRequest
                    .Replace("{lang_in}", lang_in.ToString())
                    .Replace("{lang_out}", lang_out.ToString())
                    .Replace("{tk}", tk.ToString())
                    .Replace("{input}", Uri.EscapeUriString(input));
                Debug.WriteLine(url);

                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                foreach (var item in cookies)
                {
                    request.AddCookie(item.Name, item.Value);
                }
                var restResponse = await client.ExecuteTaskAsync(request);

                r.ResponseCode = restResponse.StatusCode;
                r.ResponseText = restResponse.Content;
                if (r.ResponseCode == HttpStatusCode.OK)
                {
                    r.IsSuccess = true;
                    r.Text_out = Regex.Match(r.ResponseText, "\\[\\[\\[\"([^\"]+)").Groups[1].Value;
                }
                else
                {
                    Count_fail++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(input + ": " + ex);
            }
            return r;
        }

        private static List<long> TKK;
        private static readonly string pattern_tkk = @"tkk:'(\d+)\.(\d+)'";
        private static IList<RestResponseCookie> cookies;
        private static int count_fail = 0;
        public static int Count_fail
        {
            get => count_fail;
            set
            {
                count_fail = value;
                if (count_fail > 5)
                {
                    TKK = null;
                    Debug.WriteLine("reset TKK");
                }
            }
        }

        public static string computed_tk(string input)
        {
            if (TKK?.Any() != true)
            {
                TKK = new List<long>() { 435173, 1535985147 };
                try
                {
                    var url = "https://translate.google.com";
                    var client = new RestClient(url);
                    var request = new RestRequest(Method.GET);
                    var restResponse = client.Execute(request);

                    cookies = restResponse.Cookies;
                    if (restResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var html = restResponse.Content;
                        var m = Regex.Match(html, pattern_tkk);
                        if (m.Success)
                        {
                            TKK = new List<long> { long.Parse(m.Groups[1].Value), long.Parse(m.Groups[2].Value) };
                            Debug.WriteLine(string.Join(".", TKK));
                        }
                    }
                    else
                    {
                        Count_fail++;
                        Debug.WriteLine($"GET {url} {restResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            long b = TKK[0];
            var a = b;
            for (int f = 0; f < input.Length; f++)
            {
                a += input[f];
                a = xo((int)a, "+-a^+6");
            }
            a = xo((int)a, "+-3^+b+-f");
            a ^= TKK[1];
            if (0 > a)
            {
                a = (a & 2147483647) + 2147483648;
            }
            a %= (uint)1E6;
            return a + "." + (a ^ b);
        }

        public static long xo(int a, string b)
        {
            for (var c = 0; c < b.Length - 2; c += 3)
            {
                int d = 'a' <= b[c + 2] ? b[c + 2] - 87 : int.Parse("" + b[c + 2]);

                long d2 = 0;
                if ('+' == b[c + 1])
                {
                    d2 = (uint)a >> d;
                }
                else
                {
                    d2 = a << d;
                }

                if ('+' == b[c])
                {
                    a = (int)(a + AND((int)d2, 4294967295));
                }
                else
                {
                    a = (int)(a ^ d2);
                }
            }
            return a;
        }

        private static long AND(long a, long b)
        {
            int k = a < 0 ? -1 : 1;
            return ((a * k) & b) * k;
        }
    }

    public class TranslateResult
    {
        public bool IsSuccess { get; set; }
        public lang lang_in { get; set; }
        public lang lang_out { get; set; }
        public string Text_in { get; set; }
        public string Text_out { get; set; }
        public string ResponseText { get; set; }
        public HttpStatusCode? ResponseCode { get; set; }

        public override string ToString()
        {
            return IsSuccess ? Text_out : Text_in;
        }
    }

}
