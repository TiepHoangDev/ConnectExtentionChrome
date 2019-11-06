#define DEBUG_HTML

using GoogleTranslateLib.Text;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ConnectExtentionChrome
{
    public partial class Form1 : Form
    {
        private class Propertys
        {
            public string Folder_root => folders_save?[0];
            public string Folder_css => folders_save?[1];
            public string Folder_image => folders_save?[2];
            public string Folder_js => folders_save?[3];
            public string Folder_html => folders_save?[4];
            public string Folder_orther => folders_save?[5];

            public HttpListener httplistener { get; set; }
            public string[] folders_save { get; set; }
            public ComplexDictionary<string, requestDetails> requests { get; set; } = new ComplexDictionary<string, requestDetails>();
            public volatile List<Task> curentTasks = new List<Task>();
        }

        private volatile Propertys _property = new Propertys();

        public Form1()
        {
            InitializeComponent();
        }

        public void _log(object m)
        {
            richTextBox1.InvokeHelper(() =>
            {
                if (richTextBox1.TextLength > 99999)
                {
                    richTextBox1.Clear();
                }

                richTextBox1.AppendText($"\n{DateTime.Now.ToString("HH:mm:ss:tt dd/MM/yyyy")}>> {m}");
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            });
            Debug.Print(m + "");
        }

        private void button_listen_Click(object sender, EventArgs e)
        {
            Listen();
        }

        private string UrlServer => textBox_url.Text;

        public async void Listen()
        {
            _property.httplistener.Prefixes.Add(UrlServer);
            _property.httplistener.Start();
            _log($"listening in {_property.httplistener.Prefixes.FirstOrDefault()}");
            button_listen.InvokeHelper(() =>
            {
                button_listen.Text = _property.httplistener.IsListening ? "Stop" : "Listen";
            });

            await Task.Run(() =>
           {
               try
               {
                   while (_property.httplistener.IsListening)
                   {
                       try
                       {
                           var c = _property.httplistener.GetContext();
                           var request = c.Request;
                           var response = c.Response;

                           var ms = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
                           var obj = JsonConvert.DeserializeObject<requestDetails>(ms) as requestDetails;

                           var streamOut = response.OutputStream;
                           streamOut.Write(new byte[] { 1 }, 0, 1);
                           streamOut.Flush();
                           streamOut.Close();

                           if (obj != null)
                           {
                               if (obj.method.ToUpper() == "GET")
                               {
                                   obj.Compoted(_property.Folder_root);
                                   _log($"{obj.method} \t{obj.TypeFile} \t{obj.Key} \t{obj.Uri}");

                                   if (_property.requests.ContainsKey(obj.Key))
                                   {
                                       _log($"\t\t\trequests haved {obj.Key} \t{obj.Uri}");
                                   }
                                   else
                                   {
                                       _property.requests.Add(obj.Key, obj);
#if DEBUG_HTML0
                                if (obj.TypeFile != eTypeFile.html)
                                {
                                    continue;
                                }
#endif
                                       ThreadPool.QueueUserWorkItem(_downloadFile, obj);
                                   }
                               }
                           }
                       }
                       catch (Exception ex)
                       {
                           _log(ex.Message);
                       }
                   }
               }
               catch (Exception ex)
               {
                   _log(ex);
               }
               _log($"End listen. _property.httplistener.IsListening = {_property.httplistener.IsListening}");
           });
        }

        private async void CheckEndDownload()
        {
            try
            {
                await Task.Delay(4000);
                if (!complete_download_all && _property.curentTasks.All(q => q.IsCompleted))
                {
                    complete_download_all = true;
                    _log("complete download all");
                    dataGridView1.InvokeHelper(() =>
                    {
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = new BindingList<requestDetails>(_property.requests);
                        dataGridView1.Refresh();
                    });
                    _reloadRoot();
                    await _replaceHtml();
                }
            }
            catch (Exception ex)
            {
                _log(ex);
            }
        }


        public IList<requestDetails> lst_request => _property.requests as IList<requestDetails>;

        private async Task _replaceHtml()
        {
            var htmls = lst_request.Where(q => q.stateDownload == eStateDownload.Success && q.TypeFile == eTypeFile.html).ToList();
            var path = new eTypeFile[] {
                eTypeFile.css,
                eTypeFile.js,
                eTypeFile.image
            };
            var files = lst_request.Where(q => path.Any(a => a == q.TypeFile)).ToList();

            string nhay = "\"";
            string pattern = $"=[{nhay}'][^{nhay}']*{"{0}"}[^{nhay}']*[{nhay}']";
            foreach (var html in htmls)
            {
                var sb = new StringBuilder(File.ReadAllText(html.pathFile));

                //https://regexr.com/
                //replace file
                foreach (var f in files)
                {
                    string m_parten = string.Format(pattern, f.Key.Replace(".", "\\."));
                    var match = Regex.Match(sb.ToString(), m_parten, RegexOptions.IgnoreCase);
                    if (match?.Success == true)
                    {
                        string value = $"={nhay}{f.src}{nhay}";
                        sb = sb.Replace(match.Value, value);
                        //_log($"{f.Key}\n\t{match.Value} \n\t\t\t-> {value} ({m_parten})");
                    }
                }

                //replace a href
                var pattern_href = $"<a(.*) (href=['{nhay}][^'{nhay}]*['{nhay}])";
                var output = Regex.Replace(sb.ToString(), pattern_href, new MatchEvaluator(m =>
                {
                    return m.Value.Replace(m.Groups[2].Value, "href='#'");
                }));
                File.WriteAllText(html.pathFile, output, Encoding.UTF8);

                //translate html

                //translate text
                var pattern_text = @">\s*([^<]+)\s*<\/(?!script)(?!style)";
                var matchs = Regex.Matches(output, pattern_text);
                var task_replace = new List<Task>();
                foreach (Match m in matchs)
                {
                    var key = m.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        var tmp_key = HttpUtility.HtmlDecode(key).Trim();
                        if (!dicTranslate.ContainsKey(tmp_key))
                        {
                            dicTranslate.Add(tmp_key, "");
                            int id = dicTranslate.Count;
                            task_replace.Add(Translate.TranslateText(tmp_key).ContinueWith(tr =>
                            {
                                var tran = tr.Result;
                                dicTranslate[tmp_key] = tran.Text_out;
                                if (tran.IsSuccess)
                                {
                                    dic_newVocabulary.Add(tmp_key, dicTranslate[tmp_key]);
                                }
                                var s = $"{id}. ({tran.ResponseCode}) {tmp_key} => {dicTranslate[tmp_key]} ";
                                _log(s);
                            }));
                        }
                    }
                }
                await Task.WhenAll(task_replace);
                _log("translate text done");

                await Task.Delay(1000);
                output = Regex.Replace(output, pattern_text, new MatchEvaluator(m =>
               {
                   var key = m.Groups[1].Value;
                   if (!string.IsNullOrWhiteSpace(key))
                   {
                       var tmp_key = HttpUtility.HtmlDecode(key).Trim();
                       if (dicTranslate.ContainsKey(tmp_key))
                       {
                           _log($"replace html {key} => {dicTranslate[tmp_key]}");
                           return m.Value.Replace(key, dicTranslate[tmp_key]);
                       }
                   }
                   return m.Value.Replace(key, key);
               }));


                File.WriteAllText(html.pathFile, output, Encoding.UTF8);
                _log($"Done replace {html.Key}!");
                Process.Start(html.pathFile).WaitForExit(100);
            }
            var t = AddRange(dic_newVocabulary);
        }

        private async Task<bool> AddRange(Dictionary<string, string> dic_newVocabulary)
        {
            if (dic_newVocabulary.Any())
            {
                return await Task.Run(() =>
                {
                    bool r = false;
                    try
                    {
                        string query = $"insert into Dictionary (Key,Value) VALUES {string.Join(",", dic_newVocabulary.Select(q => $"('{q.Key.Replace("'", "")}','{q.Value.Replace("'", "")}')"))}";
                        _log(query);
                        using (var con = Connection)
                        {
                            con.Open();
                            new SQLiteCommand(query, con).ExecuteNonQuery();
                            r = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log(ex);
                        r = false;
                    }
                    dic_newVocabulary.Clear();
                    return r;
                });
            }
            return true;
        }

        private static string connectstring => ConfigurationManager.ConnectionStrings["dbToolPage"].ConnectionString;
        private static SQLiteConnection Connection => new SQLiteConnection(connectstring);
        public List<Tuple<int, string, string>> get_tuDien()
        {
            try
            {
                string query = "select * from Dictionary";
                SQLiteDataAdapter da = new SQLiteDataAdapter(query, Connection);
                var dt = new DataTable();
                da.Fill(dt);
                return dt.Rows?.Cast<DataRow>()?.Select(q => new Tuple<int, string, string>(Convert.ToInt32(q.ItemArray[0]), Convert.ToString(q.ItemArray[1]), Convert.ToString(q.ItemArray[1]))).ToList();
            }
            catch (Exception ex)
            {
                _log(ex);
            }
            return new List<Tuple<int, string, string>>();
        }

        public bool Add(string key, string value)
        {
            try
            {
                string query = $"insert into Dictionary (Key,Value) VALUES('{key}','{value}')";
                using (var con = Connection)
                {
                    con.Open();
                    return new SQLiteCommand(query, con).ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _log(ex);
                return false;
            }
        }

        public async void _downloadFile(object o)
        {
            try
            {
                var obj = o as requestDetails;
                var task = Task.Run(async () =>
                {
                    await obj?.DownloadFile();
                    if (_property.requests.Values.Any(q => q.stateDownload != eStateDownload.Success && q.stateDownload != eStateDownload.Error))
                    {
                        CheckEndDownload();
                    }
                });
                _property.curentTasks.Add(task);
                await task;
            }
            catch (Exception ex)
            {
                _log(ex);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            _property.folders_save = _createFolderCssjspageorther(textBox_folderRoot.Text);
            _reloadRoot();

            int index = 0;
            var tmp = new requestDetails();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns[index++].DataPropertyName = nameof(tmp.requestId);
            dataGridView1.Columns[index++].DataPropertyName = nameof(tmp.url);
            dataGridView1.Columns[index++].DataPropertyName = nameof(tmp.type);
            dataGridView1.Columns[index++].DataPropertyName = nameof(tmp.image);

            if (HttpListener.IsSupported)
            {
                _property.httplistener = new HttpListener();
                Listen();
            }
            else
            {
                _log("this OS not support.");
            }

            //
            dicTranslate = new Dictionary<string, string>();
            dic_newVocabulary = new Dictionary<string, string>();
            List<Tuple<int, string, string>> data = get_tuDien();
            foreach (var item in data)
            {
                if (!dicTranslate.ContainsKey(item.Item2))
                {
                    dicTranslate.Add(item.Item2, item.Item3);
                }
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _reloadRoot();
        }

        private void _reloadRoot()
        {
            treeView_root.InvokeHelper(() =>
            {
                treeView_root.Nodes.Clear();
                treeView_root.Nodes.Add(_getNodeFolder(textBox_folderRoot.Text));
                treeView_root.ExpandAll();
            });
        }

        private Dictionary<string, int> img = new Dictionary<string, int>
        {
            { "file" , 0 },
            { "png" , 1 },
            { "css" , 2 },
            { "html" , 3 },
            { "js" , 4 },
            { "folder" , 5 },
        };
        private volatile Dictionary<string, string> dicTranslate;
        private volatile Dictionary<string, string> dic_newVocabulary;
        private volatile bool complete_download_all;

        private TreeNode _getNodeFolder(string text)
        {
            if (Directory.Exists(text))
            {
                var files = Directory.GetFiles(text);
                var folders = Directory.GetDirectories(text);
                return new TreeNode(Path.GetFileName(text), (
                    from f in folders
                    select _getNodeFolder(f)
                    ).Concat(
                    from file in files
                    let ext = Path.GetExtension(file).ToLower()
                    let index = img.ContainsKey(ext) ? img[ext] : img["file"]
                    select new TreeNode(Path.GetFileName(file))
                    {
                        ImageIndex = index,
                        SelectedImageIndex = index,
                        Tag = null,
                    }
                    ).ToArray())
                {
                    ImageIndex = img["folder"],
                    SelectedImageIndex = img["folder"],
                    Tag = text
                };
            }
            return new TreeNode("not exit!!!");
        }

        private void createFolderCssjspageortherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _property.folders_save = _createFolderCssjspageorther(textBox_folderRoot.Text);
            _reloadRoot();
        }

        private string[] _createFolderCssjspageorther(string dir)
        {
            var folders = new string[] {
                dir,
                $"{dir}/css",
                $"{dir}/image",
                $"{dir}/js",
                $"{dir}/html",
                $"{dir}/orther",
            };

            foreach (var item in folders)
            {
                if (!Directory.Exists(item))
                {
                    Directory.CreateDirectory(item);
                }
            }
            _property.folders_save = folders;
            return folders;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await _replaceHtml();
            MessageBox.Show("Done");
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridView1.ColumnCount - 1)
                {
                    var obj = dataGridView1.Rows[e.RowIndex].DataBoundItem as requestDetails;
                    if (obj != null)
                    {
                        Process.Start(obj.url);
                    }
                }
            }
        }

        private void clearAllFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView_root.SelectedNode;
            if (node.Tag is string pathFolder)
            {
                _deleteFolder(pathFolder);
                _reloadRoot();
            }
        }

        private void _deleteFolder(string pathFolder)
        {
            if (Directory.Exists(pathFolder))
            {
                var lst = Directory.GetDirectories(pathFolder);
                foreach (var item in lst)
                {
                    _deleteFolder(item);
                    Directory.Delete(item);
                }
                lst = Directory.GetFiles(pathFolder);
                foreach (var item in lst)
                {
                    File.Delete(item);
                }
            }
        }

        private void treeView_root_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView_root.SelectedNode = e.Node;

            clearAllFileToolStripMenuItem.Visible = e.Node.Tag != null;
            clearAllFileToolStripMenuItem.Text = string.Format(clearAllFileToolStripMenuItem.Tag + "", e.Node.Text);

            openInExprolerToolStripMenuItem.Visible = e.Node.Tag != null;
            openInExprolerToolStripMenuItem.Text = string.Format(openInExprolerToolStripMenuItem.Tag + "", e.Node.Text);
        }

        private void newTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var root = textBox_folderRoot.Text;
            _deleteFolder(root);
            _createFolderCssjspageorther(root);
            _reloadRoot();
            dataGridView1.DataSource = null;
            dataGridView1.Refresh();
            _property.requests.Clear();
            complete_download_all = false;
        }

        private void openInExprolerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView_root.SelectedNode;
            if (node.Tag is string pathFolder)
            {
                Process.Start(pathFolder);
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
