using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using System.Net;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;

namespace PoEPartyGear
{
    public partial class SettingsForm : Form
    {
        IniHelper iniHelper = new IniHelper("settings.ini");

        public SettingsForm()
        {
            InitializeComponent();

            comboBox1.Items.AddRange(
                ((Application.OpenForms["HiddenMain"] as HiddenMain).LeagueData["economyLeagues"] as JArray)
                .Select(x => x["displayName"].ToString())
                .ToArray()
                );
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableHideoutTP", checkBox3.Checked.ToString());
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnablePriceCheck", checkBox2.Checked.ToString());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableViewProfileButton", checkBox1.Checked.ToString());
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            checkBox3.CheckedChanged -= new EventHandler(this.checkBox3_CheckedChanged);
            checkBox2.CheckedChanged -= new EventHandler(this.checkBox2_CheckedChanged);
            checkBox1.CheckedChanged -= new EventHandler(this.checkBox1_CheckedChanged);

            checkBox3.Checked = bool.Parse(iniHelper.Read("EnableHideoutTP") ?? "true");
            checkBox2.Checked = bool.Parse(iniHelper.Read("EnablePriceCheck") ?? "true");
            checkBox1.Checked = bool.Parse(iniHelper.Read("EnableViewProfileButton") ?? "true");
            comboBox1.SelectedItem = iniHelper.Read("LeagueSelectedName");

            checkBox3.CheckedChanged += new EventHandler(this.checkBox3_CheckedChanged);
            checkBox2.CheckedChanged += new EventHandler(this.checkBox2_CheckedChanged);
            checkBox1.CheckedChanged += new EventHandler(this.checkBox1_CheckedChanged);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            iniHelper.Write("LeagueSelectedName", comboBox1.SelectedItem.ToString());
        }
    }
}
