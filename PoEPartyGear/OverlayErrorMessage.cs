using Newtonsoft.Json.Linq;
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
    public partial class OverlayErrorMessage : Form
    {
        protected override bool ShowWithoutActivation
        {
            get { return false; }
        }

        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
                return createParams;
            }
        }

        public OverlayErrorMessage(string ErrorMessage)
        {
            InitializeComponent();

            label1.Text = $"{ErrorMessage}";

            Location = new Point(Location.X, Location.Y - Application.OpenForms.OfType<OverlayErrorMessage>().Count() * Size.Height);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
