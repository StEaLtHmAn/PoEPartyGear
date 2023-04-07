﻿using System.Drawing;
using System.Runtime.InteropServices;
using System;

public class Win32
{
    public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);


    //Mouse actions for sendmessage and postmessage
    public const int WM_LBUTTONDOWN = 0x201;
    public const int WM_LBUTTONUP = 0x202;
    public const int WM_LBUTTONDBLCLK = 0x203;
    public const int WM_RBUTTONDOWN = 0x204;
    public const int WM_RBUTTONUP = 0x205;
    public const int WM_RBUTTONDBLCLK = 0x206;
    public const int WM_MOUSEMOVE = 0x200;



    //Mouse actions for mouse_event function
    public const int MOUSEEVENTF_LEFTDOWN = 0x02;
    public const int MOUSEEVENTF_LEFTUP = 0x04;
    public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    public const int MOUSEEVENTF_RIGHTUP = 0x10;
    public const int MOUSEEVENTF_WHEEL = 0x0800;
    public const int MOUSEEVENTF_MOVE = 0x0001;
    public const int MOUSEEVENTF_ABSOLUTE = 0x8000;



    //Keyboard key codes
    public const int WM_KEYDOWN = 0x100;
    public const int WM_KEYUP = 0x101;
    public const int WM_SYSCOMMAND = 0x018;
    public const int SC_CLOSE = 0x053;




    // The FindWindow function retrieves a handle to the top-level window whose class name
    // and window name match the specified strings. This function does not search child windows.
    // This function does not perform a case-sensitive search.
    [DllImport("User32.dll")]
    public static extern int FindWindow(string strClassName, string strWindowName);

    // The FindWindowEx function retrieves a handle to a window whose class name 
    // and window name match the specified strings. The function searches child windows, beginning
    // with the one following the specified child window. This function does not perform a case-sensitive search.
    [DllImport("User32.dll")]
    public static extern int FindWindowEx(int hwndParent, int hwndChildAfter, string strClassName, string strWindowName);

    // The SendMessage function sends the specified message to a 
    // window or windows. It calls the window procedure for the specified 
    // window and does not return until the window procedure has processed the message. 
    [DllImport("User32.dll")]
    public static extern Int32 SendMessage(
        int hWnd,               // handle to destination window
        int Msg,                // message
        int wParam,             // first message parameter
        [MarshalAs(UnmanagedType.LPStr)] string lParam); // second message parameter


    [DllImport("User32.dll")]
    public static extern Int32 SendMessage(
    int hWnd,               // handle to destination window
    uint Msg,                // message
    int wParam,             // first message parameter
    uint lParam);            // second message parameter


    [DllImport("user32.dll")]
    public static extern IntPtr PostMessage(int hWnd, uint Msg, int wParam, uint lParam);



    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(int hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(int hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(int hWnd);
    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(int hWnd, [In] ref RECT lprcUpdate, IntPtr hrgnUpdate, int flags);
    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(int hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(int hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(int hWnd, IntPtr hdcBlt, int nFlags);


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
    //Mouse actions

    [DllImport("user32.dll")]
    public static extern bool EnableWindow(int hWnd, bool enable);

    //The ClientToScreen function converts the client-area coordinates of a specified point to screen coordinates.
    //The screen coordinates are relative to the upper-left corner of the screen.
    [DllImport("User32.Dll")]
    public static extern bool ClientToScreen(int hWnd, ref Point point);

    [DllImport("User32.Dll")]
    public static extern int ShowWindow(int hWnd, int nCmdShow);
    [DllImport("User32.Dll")]
    public static extern int ShowWindowAsync(int hWnd, int nCmdShow);


    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(int hWnd);
    [DllImport("user32.dll")]
    public static extern int GetForegroundWindow();

    public Win32()
    {

    }

    ~Win32()
    {
    }
}

public struct RECT
{
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get { return Left; }
        set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
        get { return Top; }
        set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
        get { return Bottom - Top; }
        set { Bottom = value + Top; }
    }

    public int Width
    {
        get { return Right - Left; }
        set { Right = value + Left; }
    }

    public System.Drawing.Point Location
    {
        get { return new System.Drawing.Point(Left, Top); }
        set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
        get { return new System.Drawing.Size(Width, Height); }
        set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r)
    {
        return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(System.Drawing.Rectangle r)
    {
        return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
        return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
        return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
        return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
        if (obj is RECT)
            return Equals((RECT)obj);
        else if (obj is System.Drawing.Rectangle)
            return Equals(new RECT((System.Drawing.Rectangle)obj));
        return false;
    }

    public override int GetHashCode()
    {
        return ((System.Drawing.Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
}