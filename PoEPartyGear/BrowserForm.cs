using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoEPartyGear
{
    public partial class BrowserForm : Form
    {
        public BrowserForm(string url = null, string title = null)
        {
            InitializeComponent();

            if (title != null)
                Text = title;
            if (url != null)
                webView21.Source = new Uri(url);
            BringToFront();
        }

        private void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //WindowState = FormWindowState.Maximized;
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            rect.Width = 1280;
            Bounds = rect;
            BringToFront();
        }
    }
}
