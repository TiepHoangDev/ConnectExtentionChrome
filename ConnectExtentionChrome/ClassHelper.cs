using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectExtentionChrome
{
    public static class ClassHelper
    {
        public static void InvokeHelper(this Control control, Action action)
        {
            IAsyncResult asyc = null;
            asyc = control?.BeginInvoke(new MethodInvoker(() =>
             {
                 action?.Invoke();
                 control?.EndInvoke(asyc);
             }));
        }
    }
}
