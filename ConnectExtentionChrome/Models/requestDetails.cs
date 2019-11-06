using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectExtentionChrome
{
    public class requestDetails
    {
        public string frameId { get; set; }
        public string initiator { get; set; }
        public string method { get; set; }
        public string parentFrameId { get; set; }
        public string requestId { get; set; }
        public string tabId { get; set; }
        public string timeStamp { get; set; }
        public string type { get; set; }
        public string url { get; set; }

        public Uri Uri { get; set; }
        public Image image { get; set; } = Properties.Resources.downloading_20;
        public string pathFile { get; set; }
        public string src { get; set; }
        public string Key { get; private set; }
        public eTypeFile TypeFile { get; set; }
        public eStateDownload stateDownload { get; set; } = eStateDownload.NotStart;
        public HttpStatusCode StatusCodeDownload { get; set; }

        /// <summary>
        /// Computed  url + type => Key, TypeFile, pathFile, src, Uri, from 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public bool Compoted(string root)
        {
            Uri = new Uri(url);
            if (Uri.Query?.Length > 0)
            {
                //url = url.Replace(Uri.Query, "");
                url = $"{Uri.Scheme}://{Uri.Host}{Uri.AbsolutePath}";
            }
            if (url.EndsWith("/"))
            {
                url = url.Remove(url.Length - 1, 1);
            }
            Key = (Path.GetFileName(url) + "").ToLower();

            Debug.Assert(!string.IsNullOrWhiteSpace(Key), $"Key of file name is empty? {type} \t{Uri}");

            bool checkExtention = false;
            switch (type)
            {
                case "script":
                    TypeFile = eTypeFile.js;
                    checkExtention = true;
                    break;
                case "stylesheet":
                    TypeFile = eTypeFile.css;
                    checkExtention = true;
                    break;
                case "image":
                    TypeFile = eTypeFile.image;
                    break;
                case "html":
                case "htm":
                case "main_frame":
                    TypeFile = eTypeFile.html;
                    checkExtention = true;
                    break;
                case "xmlhttprequest":
                case "font":
                case "sub_frame":
                default:
                    TypeFile = eTypeFile.orther;
                    break;
            }

            src = $"/{TypeFile}/{Key}";
            if (checkExtention)
            {
                string extention = $".{TypeFile}";
                if (!Key.EndsWith(extention))
                {
                    src += extention;
                }
            }

            pathFile = $"{root}/{src}";
            return true;
        }

        public bool SetStateDownload(eStateDownload stateDownload)
        {
            this.stateDownload = stateDownload;
            switch (stateDownload)
            {
                case eStateDownload.NotStart:
                    image = Properties.Resources.downloading_20;
                    break;
                case eStateDownload.Running:
                    image = Properties.Resources.task_20;
                    break;
                case eStateDownload.Success:
                    image = StatusCodeDownload == HttpStatusCode.OK ? Properties.Resources.tick_20 : Properties.Resources.warning_20;
                    break;
                case eStateDownload.Error:
                    image = Properties.Resources.error_20;
                    break;
                default:
                    throw new NotSupportedException(stateDownload.ToString());
            }
            return true;
        }

        public async Task DownloadFile()
        {
            await Task.Run(() =>
            {
                try
                {
                    StatusCodeDownload = HttpStatusCode.OK;
                    if (!File.Exists(pathFile))
                    {
                        var client = new RestClient(url);
                        var request = new RestRequest(Method.GET);
                        request.AddCookie("i10c.bdddb", "c2-5fa3alXe8JvcKSWgY8yKR");
                        IRestResponse restResponse = client.Execute(request);
                        using (var fs = new FileStream(pathFile, FileMode.OpenOrCreate))
                        {
                            fs.Write(restResponse.RawBytes, 0, restResponse.RawBytes.Length);
                            fs.Flush(true);
                            fs.Close();
                        }
                        StatusCodeDownload = restResponse.StatusCode;
                    }
                    else
                    {
                        Debug.WriteLine($"skip download {Key}");
                    }
                    SetStateDownload(eStateDownload.Success);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    SetStateDownload(eStateDownload.Error);
                    return;
                }
            });
        }
    }

    public enum eTypeFile
    {
        js,
        css,
        html,
        image,
        orther
    }

    public enum eStateDownload
    {
        NotStart, Running, Success, Error
    }
}
