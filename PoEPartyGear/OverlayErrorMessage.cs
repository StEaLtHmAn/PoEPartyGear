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
