using PoEPartyGear.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace PoEPartyGear
{
    public partial class OverlayButton : Form
    {
        IniHelper iniHelper = new IniHelper("settings.ini");
        string ViewProfileSource = "Official";
        string OCRstring;
        public OverlayButton(string OCRstring)
        {
            InitializeComponent();

            this.OCRstring = OCRstring;
            ViewProfileSource = iniHelper.Read("ViewProfileSource") ?? "Official";

            MouseHook.Start();
            MouseHook.MouseAction += MouseHook_MouseAction;
        }

        private void MouseHook_MouseAction(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<BrowserForm>().Count() == 0)
            if (MousePosition.X < Left || MousePosition.Y < Top || MousePosition.X >= Right || MousePosition.Y >= Bottom)
                Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] textSplit = OCRstring.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string characterName = string.Empty;
            string accountName = string.Empty;
            if (textSplit.Length == 1)
            {
                accountName = textSplit[0];
            }
            else if (textSplit.Length > 1)
            {
                characterName = textSplit[0];
                accountName = textSplit[1];
            }

            string url = string.Empty;
            if (ViewProfileSource == "Official")
                url = $"https://www.pathofexile.com/account/view-profile/{accountName}/characters"; 
            else
                url = $"https://poe-profile.info/profile/{accountName}";
            string title = $"{accountName}";
            if (!string.IsNullOrEmpty(characterName))
            {
                if (ViewProfileSource == "Official")
                    url += $"?&characterName={characterName}";
                else
                    url += $"/{characterName}?realm=pc";
                title += $" - {characterName}";
            }



            BrowserForm form = new BrowserForm($"https://www.pathofexile.com/account/view-profile/{accountName}/characters?&characterName={characterName}", title);
            form.ShowDialog();

            Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        bool highlighted = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!highlighted && MousePosition.X >= Left && MousePosition.Y >= Top && MousePosition.X < Right && MousePosition.Y < Bottom)
            {
                highlighted = true;
                button1.Image = Resources.ViewProfileButtonLight;
            }
            else if (highlighted && MousePosition.X < Left || MousePosition.Y < Top || MousePosition.X >= Right || MousePosition.Y >= Bottom)
            {
                highlighted = false;
                button1.Image = Resources.ViewProfileButton;
            }
        }

        private void OverlayButton_FormClosing(object sender, FormClosingEventArgs e)
        {
            MouseHook.stop();
        }
    }
}
