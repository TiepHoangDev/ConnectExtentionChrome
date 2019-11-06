using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectExtentionChrome
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            #region TEST SQLite

            //Form1.Add("1", "11");
            //Form1.Add("2", "22");
            //Form1.Add("3", "33");
            //Form1.Add("4", "44");

            //Debug.WriteLine(string.Join("\n",  Form1.get_tuDien().Select(q => $"{q.Item1} \t{q.Item2} \t{q.Item3}")));
            #endregion
        }
    }
}
