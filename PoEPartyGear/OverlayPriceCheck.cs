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

                        Label label = new Label();
                        label.Margin = new Padding(3, 1, 3, 1);
                        label.Size = new Size(228, 13);
                        label.Text = predItem[0].ToString();
                        flowLayoutPanel1.Controls.Add(label);

                        ProgressBar progressBar = new ProgressBar();
                        progressBar.Size = new Size(115, 15);
                        progressBar.Value = Math.Abs(value);
                        if (value > 0)
                        {
                            progressBar.Margin = new Padding(3, 1, 3, 1);
                        }
                        else
                        {
                            progressBar.RightToLeft = RightToLeft.Yes;
                            progressBar.RightToLeftLayout = true;
                            progressBar.Margin = new Padding(115, 1, 3, 1);
                        }
                        flowLayoutPanel1.Controls.Add(progressBar);
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
