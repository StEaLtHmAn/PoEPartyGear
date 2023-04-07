using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace PoEPartyGear
{
    public partial class OverlayPriceCheck : Form
    {
        JObject PriceJsonObj;
        public OverlayPriceCheck(string PriceJson)
        {
            InitializeComponent();

            try
            {
                MouseHook.Start();
                MouseHook.MouseAction += MouseHook_MouseAction;


                PriceJsonObj = JObject.Parse(PriceJson);

                if (!string.IsNullOrEmpty(PriceJsonObj["error_msg"]?.ToString()))
                {
                    label1.Text = $"E: {PriceJsonObj["error_msg"]}";
                }
                else
                {
                    if(Math.Round(double.Parse(PriceJsonObj["min"].ToString()), 0) != Math.Round(double.Parse(PriceJsonObj["max"].ToString()), 0))
                        label1.Text = $"{Math.Round(double.Parse(PriceJsonObj["min"].ToString()), 0)} - {Math.Round(double.Parse(PriceJsonObj["max"].ToString()), 0)} {PriceJsonObj["currency"]}";
                    else
                        label1.Text = $"{Math.Round(double.Parse(PriceJsonObj["min"].ToString()), 2)} - {PriceJsonObj["currency"]}";
                }
                if (PriceJsonObj.ContainsKey("pred_explanation"))
                {
                    foreach (JArray predItem in PriceJsonObj["pred_explanation"] as JArray)
                    {
                        int value = (int)Math.Round(double.Parse(predItem[1].ToString()) * 100);

                        Label label = new Label
                        {
                            Margin = new Padding(3, 1, 3, 1),
                            Size = new Size(228, 13),
                            Text = predItem[0].ToString()
                        };
                        flowLayoutPanel2.Controls.Add(label);

                        if (value == 0)
                        {
                            Label label2 = new Label
                            {
                                Margin = new Padding(3, 1, 3, 1),
                                Size = new Size(228, 14),
                                Text = "-",
                                TextAlign = ContentAlignment.MiddleCenter
                            };
                            flowLayoutPanel2.Controls.Add(label2);
                        }
                        else if (value > 0)
                        {
                            ProgressBar progressBar = new NewProgressBar
                            {
                                Size = new Size(115, 15),
                                Value =value,
                                Margin = new Padding(3, 1, 3, 1),
                                ForeColor = Color.FromArgb(0, 180, 0)
                            };
                            flowLayoutPanel2.Controls.Add(progressBar);
                        }
                        else
                        {
                            ProgressBar progressBar = new NewProgressBar();
                            progressBar.Value = Math.Abs(value);
                            progressBar.Size = new Size(115, 14);
                            progressBar.RightToLeftLayout = true;
                            progressBar.Margin = new Padding(115, 1, 3, 1);
                            progressBar.ForeColor = Color.FromArgb(180, 0, 0);
                            flowLayoutPanel2.Controls.Add(progressBar);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                label1.Text = $"E: {ex.Message}";
            }
        }

        private void MouseHook_MouseAction(object sender, EventArgs e)
        {
            if (MousePosition.X < Left || MousePosition.Y < Top || MousePosition.X >= Right || MousePosition.Y >= Bottom)
                Dispose();
        }

        private void OverlayButton_FormClosing(object sender, FormClosingEventArgs e)
        {
            MouseHook.stop();
        }
    }
}
