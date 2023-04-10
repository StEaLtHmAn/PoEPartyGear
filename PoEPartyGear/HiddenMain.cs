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
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Globals.keyboardHook.KeyPressed += KeyboardHook_KeyPressedAsync;
                //hotkeys
                Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.Control, Keys.D);
                Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.Control, Keys.A);
                Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.None, Keys.F5);
                Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.None, Keys.D1);

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
            else
            {
                Globals.DelayAction(0, new Action(() => { Dispose(); }));
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

        private async Task CtrlD_Pressed()
        {
            try
            {
                if (!bool.Parse(Globals.iniHelper.Read("EnablePriceCheck") ?? "true"))
                    return;
                if (Application.OpenForms.OfType<OverlayPriceCheck>().Count() > 0)
                    Application.OpenForms.OfType<OverlayPriceCheck>().First().Dispose();

                new WshShell().SendKeys("^(c)");
                await Task.Delay(100);
                string clipboardText = Clipboard.GetText();
                int atemptes = 1;
                while (string.IsNullOrEmpty(clipboardText) && atemptes < 5)
                {
                    new WshShell().SendKeys("^(c)");
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
                            poePriceesJSON = client.DownloadString($"https://www.poeprices.info/api?l={Globals.iniHelper.Read("LeagueSelectedName")}&i={Convert.ToBase64String(Encoding.UTF8.GetBytes(clipboardText))}");
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

        bool busyRollingMap = false;
        private async Task CtrlA_Pressed()
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
            async Task CopyItemData()
            {
                new WshShell().SendKeys("^{c}");
                await Task.Delay(100);
                clipboardText = Clipboard.GetText();
                int atemptes = 1;
                while (string.IsNullOrEmpty(clipboardText) && atemptes < 5)
                {
                    new WshShell().SendKeys("^{c}");
                    await Task.Delay(150 + atemptes * 50);
                    clipboardText = Clipboard.GetText();
                    atemptes++;
                }
                Clipboard.Clear();
            }

            try
            {
                if (busyRollingMap)
                    return;
                busyRollingMap = true;

                Point mapLocation = Cursor.Position;

                int RollingAttempt = 0;
                while (busyRollingMap)
                {
                    Cursor.Position = mapLocation;
                    await CopyItemData();

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
                            Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                            form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                            form.ShowDialog();
                            return;
                        }

                        Cursor.Position = new Point(alcLocation.X + Properties.Resources.alc.Width / 2, alcLocation.Y);
                        await Task.Delay(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        await Task.Delay(50);
                        Cursor.Position = mapLocation;
                        await Task.Delay(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        await Task.Delay(50);
                    }
                    else if (clipboardText.Contains("Rarity: Magic"))
                    {
                        Point scourLocation = GetScourLocation();
                        if (scourLocation.IsEmpty)
                        {
                            busyRollingMap = false;
                            OverlayErrorMessage form = new OverlayErrorMessage("Can't find scours.");
                            Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                            form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                            form.ShowDialog();
                            return;
                        }

                        Cursor.Position = new Point(scourLocation.X + Properties.Resources.scour.Width / 2, scourLocation.Y);
                        await Task.Delay(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        await Task.Delay(50);
                        Cursor.Position = mapLocation;
                        await Task.Delay(50);
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        await Task.Delay(50);
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
                                Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                                form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                                form.ShowDialog();
                                return;
                            }

                            Cursor.Position = new Point(scourLocation.X + Properties.Resources.scour.Width / 2, scourLocation.Y);
                            await Task.Delay(50);
                            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            await Task.Delay(50);
                            Cursor.Position = mapLocation;
                            await Task.Delay(50);
                            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            await Task.Delay(50);
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
                Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
                form.Location = new Point(bounds.Width - form.Width, bounds.Height - form.Height);
                form.ShowDialog();
            }
        }

        private async void KeyboardHook_KeyPressedAsync(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Keys.D && e.Modifier == global::ModifierKeys.Control)
            {
                if (Win32.GetForegroundWindow() != handle)
                {
                    Globals.keyboardHook.UnregisterHotKey(global::ModifierKeys.Control, Keys.D);
                    new WshShell().SendKeys("^{d}");
                    Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.Control, Keys.D);
                    return;
                }
                await CtrlD_Pressed();
            }

            if (e.Key == Keys.A && e.Modifier == global::ModifierKeys.Control)
            {
                if (Win32.GetForegroundWindow() != handle)
                {
                    Globals.keyboardHook.UnregisterHotKey(global::ModifierKeys.Control, Keys.A);
                    new WshShell().SendKeys("^{a}");
                    Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.Control, Keys.A);
                    return;
                }
                await CtrlA_Pressed();
            }

            if (e.Key == Keys.F5 && e.Modifier == global::ModifierKeys.None)
            {
                if (Win32.GetForegroundWindow() != handle)
                {
                    Globals.keyboardHook.UnregisterHotKey(global::ModifierKeys.None, Keys.F5);
                    new WshShell().SendKeys("{F5}");
                    Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.None, Keys.F5);
                    return;
                }
                if (!bool.Parse(Globals.iniHelper.Read("EnableHideoutTP") ?? "true"))
                    return;
                new WshShell().SendKeys("~/hideout~");
                return;
            }

            if (e.Key == Keys.D1 && e.Modifier == global::ModifierKeys.None)
            {
                Globals.keyboardHook.UnregisterHotKey(global::ModifierKeys.None, Keys.D1);
                new WshShell().SendKeys("{1}");
                Globals.keyboardHook.RegisterHotKey(global::ModifierKeys.None, Keys.D1);

                if (Win32.GetForegroundWindow() != handle)
                    return;
                if (!bool.Parse(Globals.iniHelper.Read("EnableFlaskHelper") ?? "true"))
                    return;
                await Task.Delay(new Random().Next(75));
                if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey2") ?? "true"))
                {
                    await Task.Delay(new Random().Next(75));
                    new WshShell().SendKeys("2");
                }
                if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey3") ?? "true"))
                {
                    await Task.Delay(new Random().Next(75));
                    new WshShell().SendKeys("3");
                }
                if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey4") ?? "true"))
                {
                    await Task.Delay(new Random().Next(75));
                    new WshShell().SendKeys("4");
                }
                if (bool.Parse(Globals.iniHelper.Read("EnableFlaskHelperKey5") ?? "true"))
                {
                    await Task.Delay(new Random().Next(75));
                    new WshShell().SendKeys("5");
                }
                return;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void getGameWindowHandle()
        {
            handle = 0;
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

                if (Application.OpenForms.OfType<OverlayButton>().Count() == 0 && Application.OpenForms.OfType<BrowserForm>().Count() == 0)
                {
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
            Globals.keyboardHook.Dispose();
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
