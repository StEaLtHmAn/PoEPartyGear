﻿    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Threading;

namespace PoEPartyGear
{
    public static class Globals
    {
        //public static KeyboardHook keyboardHook = new KeyboardHook();
        public static IniHelper iniHelper = new IniHelper("settings.ini");
        public static Dictionary<string, string> CategoryCache = new Dictionary<string, string>();
        public static string access_token = null;
        public static string clientId = null;

        public static void LogMessage(string message)
        {
            string filename = $"{Assembly.GetExecutingAssembly().GetName().Name}.log";
            if (File.Exists(filename) && new FileInfo(filename).Length > 1048576)
            {
                File.Delete(filename);
            }
            File.AppendAllText(filename, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }

        public static void DelayAction(int millisecond, Action action)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += delegate
            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        public static string getRelativeDateTime(DateTime date)
        {
            TimeSpan ts = DateTime.Now - date;
            if (ts.TotalMinutes < 1)//seconds ago
                return "just now";
            if (ts.TotalHours < 1)//min ago
                return (int)ts.TotalMinutes == 1 ? "1 Minute ago" : (int)ts.TotalMinutes + " Minutes ago";
            if (ts.TotalDays < 1)//hours ago
                return (int)ts.TotalHours == 1 ? "1 Hour ago" : (int)ts.TotalHours + " Hours ago";
            if (ts.TotalDays < 7)//days ago
                return (int)ts.TotalDays == 1 ? "1 Day ago" : (int)ts.TotalDays + " Days ago";
            if (ts.TotalDays < 30.4368)//weeks ago
                return (int)(ts.TotalDays / 7) == 1 ? "1 Week ago" : (int)(ts.TotalDays / 7) + " Weeks ago";
            if (ts.TotalDays < 365.242)//months ago
                return (int)(ts.TotalDays / 30.4368) == 1 ? "1 Month ago" : (int)(ts.TotalDays / 30.4368) + " Months ago";
            //years ago
            return (int)(ts.TotalDays / 365.242) == 1 ? "1 Year ago" : (int)(ts.TotalDays / 365.242) + " Years ago";
        }

        //[DllImport("dwmapi.dll")]
        //private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        //private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        //private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        //private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        //{
        //    if (IsWindows10OrGreater(17763))
        //    {
        //        var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
        //        if (IsWindows10OrGreater(18985))
        //        {
        //            attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
        //        }

        //        int useImmersiveDarkMode = enabled ? 1 : 0;
        //        return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        //    }

        //    return false;
        //}

        //private static bool IsWindows10OrGreater(int build = -1)
        //{
        //    return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        //}

        //public static Color DarkColour = Color.FromArgb(32, 32, 32);
        //public static Color DarkColour2 = Color.FromArgb(56, 56, 56);
        //public static void ToggleDarkMode(Form form, bool enabled)
        //{
        //    UseImmersiveDarkMode(form.Handle, enabled);
        //    if (enabled && form.BackColor == SystemColors.Control)
        //    {
        //        form.BackColor = DarkColour;
        //        form.ForeColor = SystemColors.ControlLightLight;
        //    }
        //    foreach (Control component in form.Controls)
        //    {
        //        if (enabled)
        //        {
        //            if (component.BackColor == SystemColors.Control)
        //                component.BackColor = DarkColour;
        //            else if (component.BackColor == SystemColors.Window)
        //                component.BackColor = DarkColour2;
        //            if (!(component is Button))
        //                component.ForeColor = SystemColors.ControlLightLight;
        //        }
        //    }
        //}
    }
}