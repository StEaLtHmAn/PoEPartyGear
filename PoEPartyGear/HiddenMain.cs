//using IWshRuntimeLibrary;
using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace PoEPartyGear
{
    public partial class HiddenMain : Form
    {
        public JObject LeagueData = null;
        private readonly MemoryCache _cache = MemoryCache.Default;
        int handle = 0;
        Process process = null;

        public HiddenMain()
        {
            if (!checkForUpdates())
            {
                InitializeComponent();
                Visible = false;

                if (handle == 0)
                    getGameWindowHandle();

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

                LowLevelKeyboardHook.KeyPressed += LowLevelKeyboardHook_KeyPressed;
                LowLevelKeyboardHook.Start();
            }
            else
            {
                Globals.DelayAction(0, new Action(() => { Dispose(); }));
            }
        }

        private void LowLevelKeyboardHook_KeyPressed(object sender, /*LowLevelKeyboardHook.KeyPressed*/EventArgs e)
        {
            try
            {
                //if poe not in front
                if (Win32.GetForegroundWindow() != handle)
                {
                    //return;
                }

                //if (e.Keys.Length == 2 && e.Keys.Contains(Keys.D) && e.Keys.Contains(Keys.LControlKey))
                if ((bool)Invoke((Func<bool>)delegate { return Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.LeftCtrl); }))
                {
                    CtrlD_Pressed();
                }
                //else if (e.Keys.Length == 2 && e.Keys.Contains(Keys.A) && e.Keys.Contains(Keys.LControlKey))
                else if((bool)Invoke((Func<bool>)delegate { return Keyboard.IsKeyDown(Key.A) && Keyboard.IsKeyDown(Key.LeftCtrl); }))
                {
                    CtrlA_Pressed();
                }
                //else if (e.Keys.Length == 1 && e.Keys[0] == Keys.F5)
                else  if ((bool)Invoke((Func<bool>)delegate { return Keyboard.IsKeyDown(Key.F5); }))
                {
                    if (!bool.Parse(Globals.iniHelper.Read("EnableHideoutTP") ?? "true"))
                        return;
                    LowLevelKeyboardHook.pause = true;
                    new WshShell().SendKeys("~/hideout~");
                    LowLevelKeyboardHook.pause = false;
                }
                //else if (e.Keys.Length == 1 && e.Keys[0] == Keys.D1)
                else if ((bool)Invoke((Func<bool>)delegate { return Keyboard.IsKeyDown(Key.D1); }))
                {
                    if (!bool.Parse(Globals.iniHelper.Read("EnableHideoutTP") ?? "true"))
                        return;

                    Thread.Sleep(new Random().Next(75));
                    LowLevelKeyboardHook.pause = true;
                    if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey2") ?? "true"))
                    {
                        Thread.Sleep(new Random().Next(75));
                        new WshShell().SendKeys("2");
                    }
                    if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey3") ?? "true"))
                    {
                        Thread.Sleep(new Random().Next(75));
                        new WshShell().SendKeys("3");
                    }
                    if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey4") ?? "true"))
                    {
                        Thread.Sleep(new Random().Next(75));
                        new WshShell().SendKeys("4");
                    }
                    if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey5") ?? "true"))
                    {
                        Thread.Sleep(new Random().Next(75));
                        new WshShell().SendKeys("5");
                    }
                    LowLevelKeyboardHook.pause = false;
                }
            }
            catch(Exception ex)
            {
                Globals.LogMessage(ex.ToString());
            }
        }

        private bool checkForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                string githubLatestReleaseJsonString = client.DownloadString("https://api.github.com/repos/StEaLtHmAn/PoEPartyGear/releases/latest");
                JObject githubLatestReleaseJson = JObject.Parse(githubLatestReleaseJsonString);

                Version CurrentVersion = Assembly.GetEntryAssembly().GetName().Version;
                string[] githubVersionNumbersSplit = Regex.Replace(githubLatestReleaseJson["tag_name"].ToString().ToLower(), "^[\\D]", string.Empty).Split('.');

                Version GithubVersion;
                if (githubVersionNumbersSplit.Length == 2)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]));
                else if (githubVersionNumbersSplit.Length == 3)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]), int.Parse(githubVersionNumbersSplit[2]));
                else if (githubVersionNumbersSplit.Length == 4)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]), int.Parse(githubVersionNumbersSplit[2]), int.Parse(githubVersionNumbersSplit[3]));
                else
                    GithubVersion = new Version();

                if (GithubVersion > CurrentVersion)
                {
                    foreach (JObject asset in githubLatestReleaseJson["assets"] as JArray)
                    {
                        if (asset["content_type"].ToString() == "application/x-zip-compressed")
                        {
                            MessageBox.Show(githubLatestReleaseJson["name"].ToString() + "\r\n\r\n" + githubLatestReleaseJson["body"].ToString(),
                            "New Updates - Released " + Globals.getRelativeDateTime(DateTime.Parse(githubLatestReleaseJson["published_at"].ToString())), MessageBoxButtons.OK);

                            //download latest zip
                            client.DownloadFile(asset["browser_download_url"].ToString(), asset["name"].ToString());
                            //extract latest updater
                            using (ZipArchive archive = ZipFile.OpenRead(asset["name"].ToString()))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.FullName.Contains("Updater.exe"))
                                        entry.ExtractToFile(entry.FullName, true);
                                }
                            }
                            //run the updater
                            Process.Start("Updater.exe", asset["name"].ToString());
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CtrlD_Pressed()
        {
            string GetClipboardData()
            {
                string clipboardText = null;
                int attempts = 0;
                while (string.IsNullOrEmpty(clipboardText) && attempts < 10)
                {
                    try
                    {
                        Thread.Sleep(10);
                        Invoke(new Action(() =>
                        {
                            clipboardText = Clipboard.GetDataObject().GetData(DataFormats.UnicodeText, false) as string;
                        }));
                    }
                    catch { }
                    attempts++;
                }
                return clipboardText;
            }

            LowLevelKeyboardHook.pause = true;
            try
            {
                if (!bool.Parse(Globals.iniHelper.Read("EnablePriceCheck") ?? "true"))
                    return;
                if (Application.OpenForms.OfType<OverlayPriceCheck>().Count() > 0)
                    Application.OpenForms.OfType<OverlayPriceCheck>().First().Dispose();

                new WshShell().SendKeys("^(c)");
                Thread.Sleep(100);
                string clipboardText = GetClipboardData();
                int attempts = 0;
                while (string.IsNullOrEmpty(clipboardText) && attempts < 5)
                {
                    new WshShell().SendKeys("^(c)");
                    Thread.Sleep(150 + attempts * 50);
                    clipboardText = GetClipboardData();
                    attempts++;
                    Debug.WriteLine("Clipboard get data attempts: " + attempts);
                }
                Invoke(new Action(() =>
                {
                    Clipboard.Clear();
                }));
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
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=UniqueWeapon");
                                        break;
                                    }
                                case "Body Armours":
                                case "Helmets":
                                case "Gloves":
                                case "Boots":
                                case "Shields":
                                case "Quivers":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=UniqueArmour");
                                        break;
                                    }
                                case "Utility Flasks":
                                case "Life Flasks":
                                case "Hybrid Flasks":
                                case "Mana Flasks":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=UniqueFlask");
                                        break;
                                    }
                                case "Belts":
                                case "Amulets":
                                case "Rings":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=UniqueAccessory");
                                        break;
                                    }
                                case "Jewels":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=UniqueJewel");
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

                    JObject PricesJSONobj = null;
                    if (PricesJSONArray.Count() > 1)
                    {
                        int SocketsStartIndex = clipboardText.IndexOf("Sockets: ") + 9;
                        string Sockets = clipboardText.Substring(SocketsStartIndex, clipboardText.IndexOf("\r\n", SocketsStartIndex) - SocketsStartIndex - 2).Trim();
                        int links = Sockets.Count(x => x == '-') + 1;

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

                    Invoke(new Action(() =>
                    {
                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                    { "min", value},
                                    { "max", valueMax},
                                    { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        if (System.Windows.Forms.Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X - form.Width, System.Windows.Forms.Cursor.Position.Y);
                        else
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        form.Show();
                    }));
                }
                else if (clipboardText.Contains("Item Class: Map Fragments") || clipboardText.Contains("Item Class: Stackable Currency") || clipboardText.Contains("Item Class: Delve Stackable Socketable Currency"))
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


                    if (clipboardText.Contains("Place in a Resonator"))
                        itemType = "Fossil";
                    else if (clipboardText.Contains("Oils at Cassia"))
                        itemType = "Oil";

                    string poeNinjaUniqueJSON = _cache["poeNinjaUniqueJSON" + itemType]?.ToString();
                    if (string.IsNullOrEmpty(poeNinjaUniqueJSON))
                    {
                        using (WebClient client = new WebClient())
                        {
                            switch (itemType)
                            {
                                case "Stackable Currency":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/currencyoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=Currency");
                                        break;
                                    }
                                case "Map Fragments":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/currencyoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=Fragment");
                                        break;
                                    }
                                case "Delve Stackable Socketable Currency":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=Resonator");
                                        break;
                                    }
                                case "Fossil":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=Fossil");
                                        break;
                                    }
                                case "Oil":
                                    {
                                        poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=Oil");
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
                    JObject PricesJSONobj;
                    switch (itemType)
                    {
                        default:
                        case "Map Fragments":
                        case "Stackable Currency":
                            {
                                PricesJSONobj = PricesJSONData.Where(x => x["currencyTypeName"].ToString() == itemName).First() as JObject;
                                value = double.Parse(PricesJSONobj["chaosEquivalent"].ToString());
                                break;
                            }
                        case "Fossil":
                        case "Delve Stackable Socketable Currency":
                        case "Oil":
                            {
                                PricesJSONobj = PricesJSONData.Where(x => x["name"].ToString() == itemName).First() as JObject;
                                value = double.Parse(PricesJSONobj["chaosValue"].ToString());
                                break;
                            }
                    }

                    Invoke(new Action(() =>
                    {
                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                    { "min", value},
                                    { "max", value},
                                    { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        if (System.Windows.Forms.Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X - form.Width, System.Windows.Forms.Cursor.Position.Y);
                        else
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        form.Show();
                    }));
                }
                else if (clipboardText.Contains("Rarity: Rare"))
                {
                    string poePriceesJSON = _cache[clipboardText]?.ToString();
                    if (string.IsNullOrEmpty(poePriceesJSON))
                    {
                        using (WebClient client = new WebClient())
                        {
                            poePriceesJSON = client.DownloadString($"https://www.poeprices.info/api?l={Globals.iniHelper.Read("LeagueSelectedName")}&i={Convert.ToBase64String(Encoding.UTF8.GetBytes(clipboardText))}");
                            JObject PricesJSONobj = JObject.Parse(poePriceesJSON);
                            if (string.IsNullOrEmpty(PricesJSONobj["error_msg"].ToString()))
                                _cache.Add(new CacheItem(clipboardText, poePriceesJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10)) });
                        }
                    }
                    Invoke(new Action(() =>
                    {
                        OverlayPriceCheck form = new OverlayPriceCheck(poePriceesJSON);
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        if (System.Windows.Forms.Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X - form.Width, System.Windows.Forms.Cursor.Position.Y);
                        else
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        form.Show();
                    }));
                }
                else if (clipboardText.Contains("Rarity: Gem"))
                {
                    string itemType = clipboardText.Substring(12, clipboardText.IndexOf("\r\n", 12) - 10).Trim();
                    int NameStartIndex = clipboardText.IndexOf("Rarity: Gem\r\n") + 13;
                    string itemName = clipboardText.Substring(NameStartIndex, clipboardText.IndexOf("\r\n", NameStartIndex) + 2 - NameStartIndex).Trim();

                    string poeNinjaUniqueJSON = _cache["poeNinjaJSON" + itemType]?.ToString();
                    if (string.IsNullOrEmpty(poeNinjaUniqueJSON))
                    {
                        using (WebClient client = new WebClient())
                        {
                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=SkillGem");
                            if (!string.IsNullOrEmpty(poeNinjaUniqueJSON))
                                _cache.Add(new CacheItem(poeNinjaUniqueJSON, poeNinjaUniqueJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddDays(5)) });
                        }
                    }

                    JArray PricesJSONData = JObject.Parse(poeNinjaUniqueJSON)["lines"] as JArray;
                    double value = 0;
                    double valueMax = 0;
                    string currency = "chaos";
                    var PricesJSONArray = PricesJSONData.Where(x => x["name"].ToString() == itemName);

                    JObject PricesJSONobj = null;
                    if (PricesJSONArray.Count() > 1)
                    {
                        int LevelStartIndex = clipboardText.IndexOf("Level: ") + 7;
                        string Level = new string(clipboardText.Substring(LevelStartIndex, clipboardText.IndexOf("\r\n", LevelStartIndex) - LevelStartIndex).Where(x => char.IsDigit(x)).ToArray());

                        string Quality = string.Empty;
                        if (clipboardText.Contains("Quality: "))
                        {
                            int QualityStartIndex = clipboardText.IndexOf("Quality: ") + 9;
                            Quality = new string(clipboardText.Substring(QualityStartIndex, clipboardText.IndexOf("\r\n", QualityStartIndex) - QualityStartIndex).Where(x => char.IsDigit(x)).ToArray());
                        }

                        foreach (JObject item in PricesJSONArray)
                        {
                            if (item.ContainsKey("gemLevel") && item["gemLevel"].ToString() == Level.ToString() &&
                                item.ContainsKey("gemQuality") && item["gemQuality"].ToString() == Quality.ToString())
                                PricesJSONobj = item;
                        }
                        if (PricesJSONobj == null)
                        {
                            PricesJSONobj = PricesJSONArray.Last() as JObject;
                        }
                    }

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

                    Invoke(new Action(() =>
                    {
                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                    { "min", value},
                                    { "max", valueMax},
                                    { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        if (System.Windows.Forms.Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X - form.Width, System.Windows.Forms.Cursor.Position.Y);
                        else
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        form.Show();
                    }));
                }
                else if (clipboardText.Contains("Rarity: Divination Card"))
                {
                    string itemType = clipboardText.Substring(12, clipboardText.IndexOf("\r\n", 12) - 10).Trim();
                    int NameStartIndex = clipboardText.IndexOf("Rarity: Divination Card\r\n") + 25;
                    string itemName = clipboardText.Substring(NameStartIndex, clipboardText.IndexOf("\r\n", NameStartIndex) + 2 - NameStartIndex).Trim();

                    string poeNinjaUniqueJSON = _cache["poeNinjaJSON" + itemType]?.ToString();
                    if (string.IsNullOrEmpty(poeNinjaUniqueJSON))
                    {
                        using (WebClient client = new WebClient())
                        {
                            poeNinjaUniqueJSON = client.DownloadString($"https://poe.ninja/api/data/itemoverview?league={Globals.iniHelper.Read("LeagueSelectedName")}&type=DivinationCard");
                            if (!string.IsNullOrEmpty(poeNinjaUniqueJSON))
                                _cache.Add(new CacheItem(poeNinjaUniqueJSON, poeNinjaUniqueJSON), new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddDays(5)) });
                        }
                    }

                    JArray PricesJSONData = JObject.Parse(poeNinjaUniqueJSON)["lines"] as JArray;
                    double value = 0;
                    double valueMax = 0;
                    string currency = "chaos";
                    var PricesJSONArray = PricesJSONData.Where(x => x["name"].ToString() == itemName);

                    JObject PricesJSONobj = PricesJSONArray.Last() as JObject;

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

                    Invoke(new Action(() =>
                    {
                        OverlayPriceCheck form = new OverlayPriceCheck(
                            new JObject {
                                    { "min", value},
                                    { "max", valueMax},
                                    { "currency", currency}
                            }.ToString(Newtonsoft.Json.Formatting.None));
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        if (System.Windows.Forms.Cursor.Position.X + form.Width > bounds.Width)
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X - form.Width, System.Windows.Forms.Cursor.Position.Y);
                        else
                            form.Location = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        form.Show();
                    }));
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        OverlayErrorMessage form = new OverlayErrorMessage($"E: Clipboard text error, {clipboardText}");
                        Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                        form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                        form.Show();
                    }));
                }
                return;
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    OverlayErrorMessage form = new OverlayErrorMessage($"E: {ex}");
                    Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                    form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                    form.Show();
                }));
                Globals.LogMessage(ex.ToString());
            }
            finally
            {
                LowLevelKeyboardHook.pause = false;
            }
        }

        bool busyRollingMap = false;
        private void CtrlA_Pressed()
        {
            Point GetAlcLocation()
            {
                using (Bitmap CurrentView = ImageProcessing.ScreenshotWindow(handle))
                {
                    Point alcLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.alc, 0.75);
                    if (alcLocation.IsEmpty)
                    {
                        alcLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.alc2, 0.75);
                        if (alcLocation.IsEmpty)
                        {
                            alcLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.alc3, 0.75);
                        }
                    }
                    return alcLocation;
                }
            }

            Point GetScourLocation()
            {
                using (Bitmap CurrentView = ImageProcessing.ScreenshotWindow(handle))
                {
                    Point scourLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.scour, 0.75);
                    if (scourLocation.IsEmpty)
                    {
                        scourLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.scour2, 0.75);
                        if (scourLocation.IsEmpty)
                        {
                            scourLocation = ImageProcessing.LocateImageBestMatch(CurrentView, Properties.Resources.scour3, 0.75);
                        }
                    }
                    return scourLocation;
                }
            }


            string clipboardText = string.Empty;
            void CopyItemData()
            {
                new WshShell().SendKeys("^{c}");
                Thread.Sleep(100);
                clipboardText = Clipboard.GetText();
                int atemptes = 1;
                while (string.IsNullOrEmpty(clipboardText) && atemptes < 5)
                {
                    new WshShell().SendKeys("^{c}");
                    Thread.Sleep(150 + atemptes * 50);
                    clipboardText = Clipboard.GetText();
                    atemptes++;
                }
                Clipboard.Clear();
            }
            LowLevelKeyboardHook.pause = true;
            try
            {
                if (busyRollingMap)
                    return;
                busyRollingMap = true;

                Point mapLocation = System.Windows.Forms.Cursor.Position;

                int RollingAttempt = 0;
                while (busyRollingMap)
                {
                    System.Windows.Forms.Cursor.Position = mapLocation;
                    CopyItemData();

                    if (!clipboardText.Contains("Item Class: Maps") || clipboardText.Contains("Unidentified") || clipboardText.Contains("Corrupted"))
                    {
                        busyRollingMap = false;
                        return;
                    }
                    string ExcludedMapMods = Globals.iniHelper.Read("ExcludedMapMods") ?? string.Empty;

                    if (clipboardText.Contains("Rarity: Normal"))
                    {
                        Point alcLocation = GetAlcLocation();
                        if (alcLocation.IsEmpty)
                        {
                            busyRollingMap = false;
                            OverlayErrorMessage form = new OverlayErrorMessage("Can't find alcs.");
                            Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                            form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                            form.Show();
                            return;
                        }

                        System.Windows.Forms.Cursor.Position = new Point(alcLocation.X + Properties.Resources.alc.Width / 2, alcLocation.Y);
                        Thread.Sleep(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        Thread.Sleep(50);
                        System.Windows.Forms.Cursor.Position = mapLocation;
                        Thread.Sleep(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        Thread.Sleep(50);
                    }
                    else if (clipboardText.Contains("Rarity: Magic"))
                    {
                        Point scourLocation = GetScourLocation();
                        if (scourLocation.IsEmpty)
                        {
                            busyRollingMap = false;
                            OverlayErrorMessage form = new OverlayErrorMessage("Can't find scours.");
                            Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                            form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                            form.Show();
                            return;
                        }

                        System.Windows.Forms.Cursor.Position = new Point(scourLocation.X + Properties.Resources.scour.Width / 2, scourLocation.Y);
                        Thread.Sleep(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        Thread.Sleep(50);
                        System.Windows.Forms.Cursor.Position = mapLocation;
                        Thread.Sleep(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        Thread.Sleep(50);
                    }
                    else if (clipboardText.Contains("Rarity: Rare"))
                    {
                        string[] clipboardTextLines = string.Concat(clipboardText.Where(x=>!char.IsDigit(x))).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        if (clipboardTextLines.Any(x => ExcludedMapMods.Contains(x)))
                        {
                            Point scourLocation = GetScourLocation();
                            if (scourLocation.IsEmpty)
                            {
                                busyRollingMap = false;
                                OverlayErrorMessage form = new OverlayErrorMessage("Can't find scours.");
                                Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                                form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                                form.Show();
                                return;
                            }

                            System.Windows.Forms.Cursor.Position = new Point(scourLocation.X + Properties.Resources.scour.Width / 2, scourLocation.Y);
                            Thread.Sleep(50);
                            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            Thread.Sleep(50);
                            System.Windows.Forms.Cursor.Position = mapLocation;
                            Thread.Sleep(50);
                            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            Thread.Sleep(50);
                        }
                        else
                            busyRollingMap = false;
                    }
                    RollingAttempt++;
                    if (RollingAttempt > 10)
                    {
                        busyRollingMap = false;
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                busyRollingMap = false;
                OverlayErrorMessage form = new OverlayErrorMessage($"E: {ex}");
                Rectangle bounds = Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;
                form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                form.Show();
            }
            finally
            {
                LowLevelKeyboardHook.pause = false;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void getGameWindowHandle()
        {
            handle = 0;
            process = null;
            //var allP = Process.GetProcesses();
            if (Process.GetProcesses().Any(x => x.ProcessName == "PathOfExileSteam"))
            {
                process = Process.GetProcessesByName("PathOfExileSteam")[0];
                handle = (int)process.MainWindowHandle;
            }
            if (handle == 0)
            {
                handle = Win32.FindWindow("POEWindowClass", "Path of Exile");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                if (!bool.Parse(Globals.iniHelper.Read("EnableViewProfileButton") ?? "true"))
                    return;

                if (Application.OpenForms.OfType<OverlayButton>().Count() == 0 && Application.OpenForms.OfType<BrowserForm>().Count() == 0 && Application.OpenForms.OfType<SettingsForm>().Count() == 0 && Application.OpenForms.OfType<OverlayPriceCheck>().Count() == 0)
                {
                    if (handle == 0)
                        getGameWindowHandle();
                    if (handle != 0)
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
                                        form.Show();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch {
                getGameWindowHandle();
            }
            timer1.Enabled = true;
        }

        private void HiddenMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            LowLevelKeyboardHook.Stop();
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
            form.Show();
        }
    }
}
