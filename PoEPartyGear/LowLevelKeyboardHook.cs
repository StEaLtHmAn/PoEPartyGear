using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

public class LowLevelKeyboardHook
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    //private const int WM_KEYUP = 0x0101;

    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    //public static List<Keys> CurrentKeys = new List<Keys>();
    public static event EventHandler/*<KeyPressedEventArgs>*/ KeyPressed;
    //public class KeyPressedEventArgs : EventArgs
    //{
    //    private Keys[] _keys;

    //    internal KeyPressedEventArgs(Keys[] keys)
    //    {
    //        _keys = keys;
    //    }
    //    public Keys[] Keys
    //    {
    //        get { return _keys; }
    //    }
    //}

    public static bool pause = false;

    public static void Start()
    {
        _hookID = SetHook(_proc);
        //CurrentKeys = new List<Keys>();
        pause = false;
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && KeyPressed != null)
        {
            //Keys vkCode = (Keys)Marshal.ReadInt32(lParam);
            //if (wParam == (IntPtr)WM_KEYUP && CurrentKeys.Contains(vkCode))
            //{
            //    CurrentKeys.Remove(vkCode);
            //}
            /*else */if (wParam == (IntPtr)WM_KEYDOWN /*&& !CurrentKeys.Contains(vkCode)*/ && !pause)
            {
                //CurrentKeys.Add(vkCode);

                Task.Run(new Action(() => { KeyPressed(null, null/*new KeyPressedEventArgs(CurrentKeys.ToArray())*/); }));
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}