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
            textBox1.TextChanged -= new EventHandler(this.textBox1_TextChanged);
            comboBox1.SelectedIndexChanged -= new EventHandler(this.comboBox1_SelectedIndexChanged);
            comboBox2.SelectedIndexChanged -= new EventHandler(this.comboBox2_SelectedIndexChanged);
            checkBox4.CheckedChanged -= new EventHandler(this.checkBox4_CheckedChanged);
            checkBox5.CheckedChanged -= new EventHandler(this.checkBox5_CheckedChanged);
            checkBox6.CheckedChanged -= new EventHandler(this.checkBox6_CheckedChanged);
            checkBox7.CheckedChanged -= new EventHandler(this.checkBox7_CheckedChanged);
            checkBox8.CheckedChanged -= new EventHandler(this.checkBox8_CheckedChanged);

            comboBox2.SelectedItem = iniHelper.Read("LeagueSelectedName") ?? comboBox2.Items[0];
            checkBox3.Checked = bool.Parse(iniHelper.Read("EnableHideoutTP") ?? "true");
            checkBox2.Checked = bool.Parse(iniHelper.Read("EnablePriceCheck") ?? "true");
            checkBox1.Checked = bool.Parse(iniHelper.Read("EnableViewProfileButton") ?? "true");
            comboBox1.SelectedItem = iniHelper.Read("LeagueSelectedName") ?? comboBox1.Items[0];
            textBox1.Text = string.Join("\r\n", (iniHelper.Read("ExcludedMapMods") ?? string.Empty).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            checkBox4.Checked = bool.Parse(iniHelper.Read("EnableFlaskHelper") ?? "true");
            checkBox5.Checked = bool.Parse(iniHelper.Read("EnableFlaskHelperKey2") ?? "true");
            checkBox6.Checked = bool.Parse(iniHelper.Read("EnableFlaskHelperKey3") ?? "true");
            checkBox7.Checked = bool.Parse(iniHelper.Read("EnableFlaskHelperKey4") ?? "true");
            checkBox8.Checked = bool.Parse(iniHelper.Read("EnableFlaskHelperKey5") ?? "true");

            checkBox3.CheckedChanged += new EventHandler(this.checkBox3_CheckedChanged);
            checkBox2.CheckedChanged += new EventHandler(this.checkBox2_CheckedChanged);
            checkBox1.CheckedChanged += new EventHandler(this.checkBox1_CheckedChanged);
            textBox1.TextChanged += new EventHandler(this.textBox1_TextChanged);
            comboBox1.SelectedIndexChanged += new EventHandler(this.comboBox1_SelectedIndexChanged);
            comboBox2.SelectedIndexChanged += new EventHandler(this.comboBox2_SelectedIndexChanged);
            checkBox4.CheckedChanged += new EventHandler(this.checkBox4_CheckedChanged);
            checkBox5.CheckedChanged += new EventHandler(this.checkBox5_CheckedChanged);
            checkBox6.CheckedChanged += new EventHandler(this.checkBox6_CheckedChanged);
            checkBox7.CheckedChanged += new EventHandler(this.checkBox7_CheckedChanged);
            checkBox8.CheckedChanged += new EventHandler(this.checkBox8_CheckedChanged);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            iniHelper.Write("LeagueSelectedName", comboBox1.SelectedItem.ToString());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            iniHelper.Write("ExcludedMapMods", string.Join(";", string.Concat(textBox1.Text.Where(x => !char.IsDigit(x))).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)));
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            if (e.State == DrawItemState.Selected)
            {
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                e.DrawBackground();
            }

            StringFormat _stringFlags = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(tabControl1.TabPages[e.Index].Text, tabControl1.Font, Brushes.Black, tabControl1.GetTabRect(e.Index), new StringFormat(_stringFlags));
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            iniHelper.Write("ViewProfileSource", comboBox2.SelectedItem.ToString());
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableFlaskHelper", checkBox4.Checked.ToString());
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableFlaskHelperKey2", checkBox5.Checked.ToString());
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableFlaskHelperKey3", checkBox6.Checked.ToString());
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableFlaskHelperKey4", checkBox7.Checked.ToString());
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            iniHelper.Write("EnableFlaskHelperKey5", checkBox8.Checked.ToString());
        }
    }
}
