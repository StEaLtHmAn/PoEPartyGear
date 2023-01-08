using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoEPartyGear
{
    public partial class HiddenMain : Form
    {
        KeyboardHook keyboardHook = new KeyboardHook();
        IniHelper iniHelper = new IniHelper("settings.ini");
        public JObject LeagueData = null;
        private readonly MemoryCache _cache = MemoryCache.Default;

        public HiddenMain()
        {
            InitializeComponent();
            Visible = false;
            //hotkeys
            keyboardHook.KeyPressed += KeyboardHook_KeyPressedAsync;
            keyboardHook.RegisterHotKey(global::ModifierKeys.Control, Keys.D);
            keyboardHook.RegisterHotKey(global::ModifierKeys.None, Keys.F5);

            if (!System.IO.File.Exists("League.data"))
            {
                using (WebClient client = new WebClient())
                {
                    LeagueData = JObject.Parse(client.DownloadString("https://poe.ninja/api/data/getindexstate"));
                    System.IO.File.WriteAllText("League.data", DateTime.Today.ToString() + Environment.NewLine + LeagueData.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }
            else
            {
                DateTime checkDate = DateTime.Parse(System.IO.File.ReadAllText("League.data").Split(Environment.NewLine.ToCharArray())[0]);
                if (DateTime.Today.Subtract(checkDate).TotalDays == 0)
                    LeagueData = JObject.Parse(string.Join(Environment.NewLine, System.IO.File.ReadAllText("League.data")
                                    .Split(Environment.NewLine.ToCharArray())
                                    .Skip(1)
                                    .ToArray()));
                else
                    using (WebClient client = new WebClient())
                    {
                        LeagueData = JObject.Parse(client.DownloadString("https://poe.ninja/api/data/getindexstate"));
                        System.IO.File.WriteAllText("League.data", DateTime.Today.ToString() + Environment.NewLine + LeagueData.ToString(Newtonsoft.Json.Formatting.Indented));
                    }
            }
        }

        private async void KeyboardHook_KeyPressedAsync(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Keys.D && e.Modifier == global::ModifierKeys.Control)
            {
                try
                {
                    if (!bool.Parse(iniHelper.Read("EnablePriceCheck") ?? "true"))
                        return;

                    while (Application.OpenForms.OfType<OverlayPriceCheck>().Count() > 0)
                        Application.OpenForms.OfType<OverlayPriceCheck>().First().Dispose();

                    new WshShell().SendKeys("^{c}");
                    await Task.Delay(100);
                    string clipboardText = Clipboard.GetText();
                    int atemptes = 1;
                    while (string.IsNullOrEmpty(clipboardText) && atemptes < 5)
                    {
                        new WshShell().SendKeys("^{c}");
                        await Task.Delay(150 + atemptes * 50);
                        clipboardText = Clipboard.GetText();
                        atemptes++;
                    }
                    Clipboard.Clear();
                    if (clipboardText.Contains("Rarity: Unique"))
                    {
                        string itemType = clipboardText.Substring(12, clipboardText.IndexOf("\r\n", 12) - 10).Trim();
                        int NameStartIndex = clipboardText.IndexOf("Rarity: Unique\r\n") + 16;
                        string itemName = clipboardText.Substring(NameStartIndex, clipboardText.IndexOf("\r\n", NameStartIndex) + 2 - NameStartIndex).Trim();

                        string poeNinjaUniqueJSON = _cache["poeNinjaUniqueJSON" + itemType]?.ToString();
                        if (string.IsNullOrEmpty(poeNinjaUniqueJSON))
                        {
                            using (WebClient client = new WebClient())
                            {
                                switch (itemType)
                                {
                                    case "One Hand Swords":
                                    case "Rune Daggers":
                                    case "Bows":
                                    case "Warstaves":
                                    case "Sceptres":
                                    case "Two Hand Swords":
                                    case "Wands":
                                    case "Claws":
                                    case "One Hand Axes":
                                    case "Two Hand Axes":
                                    case "One Hand Mace":
                                    case "Two Hand Mace":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={iniHelper.Read("LeagueSelectedName")}&type=UniqueWeapon");
                                            break;
                                        }
                                    case "Body Armours":
                                    case "Helmets":
                                    case "Gloves":
                                    case "Boots":
                                    case "Shields":
                                    case "Quivers":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={iniHelper.Read("LeagueSelectedName")}&type=UniqueArmour");
                                            break;
                                        }
                                    case "Utility Flasks":
                                    case "Life Flasks":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={iniHelper.Read("LeagueSelectedName")}&type=UniqueFlask");
                                            break;
                                        }
                                    case "Belts":
                                    case "Amulets":
                                    case "Rings":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={iniHelper.Read("LeagueSelectedName")}&type=UniqueAccessory");
                                            break;
                                        }
                                    case "Jewels":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={iniHelper.Read("LeagueSelectedName")}&type=UniqueJewel");
                                            break;
                                        }
                                }
                                if (!string.IsNullOrEmpty(poeNinjaUniqueJSON))
                                    _cache.Add(new CacheItem(poeNinjaUniqueJSON, poeNinjaUniqueJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddDays(5)) });
                            }
                        }

                        JArray PricesJSONData = JObject.Parse(poeNinjaUniqueJSON)["lines"] as JArray;
                        double value = 0;
                        double valueMax = 0;
                        string currency = "chaos";
                        var PricesJSONArray = PricesJSONData.Where(x => x["name"].ToString() == itemName);

                        int SocketsStartIndex = clipboardText.IndexOf("Sockets: ") + 9;
                        string Sockets = clipboardText.Substring(SocketsStartIndex, clipboardText.IndexOf("\r\n", SocketsStartIndex) - SocketsStartIndex - 2).Trim();
                        int links = Sockets.Count(x => x == '-') + 1;

                        JObject PricesJSONobj = null;
                        if (PricesJSONArray.Count() > 1)
                        {
                            foreach (JObject item in PricesJSONArray)
                            {
                                if (item.ContainsKey("links") && item["links"].ToString() == links.ToString())
                                    PricesJSONobj = item;
                            }
                            if (PricesJSONobj == null)
                            {
                                PricesJSONobj = PricesJSONArray.First(x => !(x as JObject).ContainsKey("links")) as JObject;
                            }
                        }
                        else if (PricesJSONArray.Count() == 1)
                            PricesJSONobj = PricesJSONArray.First() as JObject;

                        if (double.Parse(PricesJSONobj["divineValue"].ToString()) >= 1)
                        {
                            value = double.Parse(PricesJSONobj["divineValue"].ToString());
                            currency = "divine";
                        }
                        else
                        {
                            value = double.Parse(PricesJSONobj["chaosValue"].ToString());
                        }
                        valueMax = value;

                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                { "min", value},
                                { "max", valueMax},
                                { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                        if (Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(Cursor.Position.X - form.Width, Cursor.Position.Y);
                        else
                            form.Location = new Point(Cursor.Position.X, Cursor.Position.Y);
                        form.ShowDialog();
                    }
                    else if (clipboardText.Contains("Item Class: Map Fragments") || clipboardText.Contains("Item Class: Stackable Currency"))
                    {
                        string itemType = clipboardText.Substring(12, clipboardText.IndexOf("\r\n", 12) - 10).Trim();
                        int NameStartIndex = 0;
                        if (clipboardText.Contains("Rarity: Currency\r\n"))
                        {
                            NameStartIndex = clipboardText.IndexOf("Rarity: Currency\r\n") + 18;
                        }
                        else if (clipboardText.Contains("Rarity: Normal\r\n"))
                        {
                            NameStartIndex = clipboardText.IndexOf("Rarity: Normal\r\n") + 16;
                        }
                        string itemName = clipboardText.Substring(NameStartIndex, clipboardText.IndexOf("\r\n", NameStartIndex) + 2 - NameStartIndex).Trim();

                        string poeNinjaUniqueJSON = _cache["poeNinjaUniqueJSON" + itemType]?.ToString();
                        if (string.IsNullOrEmpty(poeNinjaUniqueJSON))
                        {
                            using (WebClient client = new WebClient())
                            {
                                switch (itemType)
                                {
                                    case "Stackable Currency":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/currencyoverview?league={iniHelper.Read("LeagueSelectedName")}&type=Currency");
                                            break;
                                        }
                                    case "Map Fragments":
                                        {
                                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/currencyoverview?league={iniHelper.Read("LeagueSelectedName")}&type=Fragment");
                                            break;
                                        }
                                }
                                if (!string.IsNullOrEmpty(poeNinjaUniqueJSON))
                                    _cache.Add(new CacheItem(poeNinjaUniqueJSON, poeNinjaUniqueJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddDays(5)) });
                            }
                        }
                        JArray PricesJSONData = JObject.Parse(poeNinjaUniqueJSON)["lines"] as JArray;
                        double value = 0;
                        string currency = "chaos";
                        JObject PricesJSONobj = PricesJSONData.Where(x => x["currencyTypeName"].ToString() == itemName).First() as JObject;
                        value = double.Parse(PricesJSONobj["chaosEquivalent"].ToString());

                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                { "min", value},
                                { "max", value},
                                { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                        if (Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(Cursor.Position.X - form.Width, Cursor.Position.Y);
                        else
                            form.Location = new Point(Cursor.Position.X, Cursor.Position.Y);
                        form.ShowDialog();
                    }
                    else if (clipboardText.Contains("Rarity: Rare"))
                    {
                        string poePriceesJSON = _cache[clipboardText]?.ToString();
                        if (string.IsNullOrEmpty(poePriceesJSON))
                        {
                            using (WebClient client = new WebClient())
                            {
                                poePriceesJSON = client.DownloadString($"https://www.poeprices.info/api?l={iniHelper.Read("LeagueSelectedName")}&i={Convert.ToBase64String(Encoding.UTF8.GetBytes(clipboardText))}");
                                JObject PricesJSONobj = JObject.Parse(poePriceesJSON);
                                if (string.IsNullOrEmpty(PricesJSONobj["error_msg"].ToString()))
                                    _cache.Add(new CacheItem(clipboardText, poePriceesJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10)) });
                            }
                        }
                        OverlayPriceCheck form = new OverlayPriceCheck(poePriceesJSON);
                        Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                        if (Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(Cursor.Position.X - form.Width, Cursor.Position.Y);
                        else
                            form.Location = new Point(Cursor.Position.X, Cursor.Position.Y);
                        form.ShowDialog();
                    }
                    else
                    {
                        OverlayErrorMessage form = new OverlayErrorMessage($"E: Clipboard text error, {clipboardText}");
                        Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                        form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                        form.ShowDialog();
                    }
                    return;
                }
                catch (Exception ex)
                {
                    OverlayErrorMessage form = new OverlayErrorMessage($"E: {ex}");
                    Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                    form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                    form.ShowDialog();
                }
            }

            if (e.Key == Keys.F5 && e.Modifier == global::ModifierKeys.None)
            {
                if (!bool.Parse(iniHelper.Read("EnableHideoutTP") ?? "true"))
                    return;
                new WshShell().SendKeys("~/hideout~");
                return;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        int handle = 0;
        Process process = null;
        public void getGameWindowHandle()
        {
            //var allP = Process.GetProcesses();
            if (Process.GetProcesses().Any(x => x.ProcessName == "PathOfExileSteam"))
            {
                process = Process.GetProcessesByName("PathOfExileSteam")[0];
                handle = (int)process.MainWindowHandle;
            }
            //if (handle == 0)
            //{
            //    handle = Win32.FindWindow("", "Path of Exile");
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                if (!bool.Parse(iniHelper.Read("EnableViewProfileButton") ?? "true"))
                    return;

                if (Application.OpenForms.OfType<OverlayButton>().Count() == 0 && Application.OpenForms.OfType<BrowserForm>().Count() == 0)
                {
                    //get window handle and game process
                    if (handle == 0)
                        getGameWindowHandle();

                    if (handle != 0 && process != null)
                    {
                        using (Bitmap CurrentView = ImageProcessing.ScreenshotWindow(handle))
                        using (Bitmap croppedCurrentView = ImageProcessing.cropAtRect(CurrentView, new Rectangle(0, 0, (int)(CurrentView.Width*0.7), CurrentView.Height)))
                        {
                            Point topLeft = ImageProcessing.LocateImageSingle(croppedCurrentView, Properties.Resources.PlayerMenuStripTopLeftCorner);
                            if (topLeft != Point.Empty)
                            {
                                Point bottomRight = ImageProcessing.LocateImageSingle(croppedCurrentView, Properties.Resources.PlayerMenuStripBottomRightCorner);
                                if (bottomRight != Point.Empty && bottomRight.X - topLeft.X > 0 && bottomRight.Y - topLeft.Y > 0)
                                {
                                    string text = ImageProcessing.ReadTextInRect(handle, new Rectangle(topLeft.X, topLeft.Y, bottomRight.X + 20 - topLeft.X, bottomRight.Y - topLeft.Y), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-", 170);
                                    if (text.Length > 0)
                                    {
                                        //RECT rc;
                                        //Win32.GetWindowRect(handle, out rc); //gets dimensions of the window

                                        OverlayButton form = new OverlayButton(text);
                                        form.Location = new Point(topLeft.X, topLeft.Y - form.Height);
                                        form.ShowDialog();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            timer1.Enabled = true;
        }

        private void HiddenMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            keyboardHook.Dispose();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x00000080;
                return Params;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog();
        }
    }
}
