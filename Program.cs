using System.Text;
using System.Runtime.InteropServices;

class ActiveWindowTracker
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    const uint SPI_GETWORKAREA = 0x0030;

    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    private static string GetActiveWindowTitle(IntPtr handle)
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return "";
    }

    private static void PositionWindow(IntPtr hWnd, int x, int y, int width, int height)
    {
        SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, 0);
    }

    public static void Main()
    {
        IntPtr first = IntPtr.Zero;
        IntPtr second = IntPtr.Zero;
        string firstTitle = "";
        string secondTitle = "";

        first = GetForegroundWindow();
        while (first == IntPtr.Zero || firstTitle == "" || firstTitle == "Program Manager" || firstTitle == "Search")
        {
            first = GetForegroundWindow();
            firstTitle = GetActiveWindowTitle(first);
            Thread.Sleep(300);
        }

        Thread.Sleep(200);
        second = GetForegroundWindow();
        while (second == first || second == IntPtr.Zero || secondTitle == "" || secondTitle == "Program Manager" || secondTitle == "Search")
        {
            second = GetForegroundWindow();
            secondTitle = GetActiveWindowTitle(second);
            Thread.Sleep(300);
        }

        ShowWindow(first, 9);
        ShowWindow(second, 9);

        MONITORINFO monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        IntPtr monitor = MonitorFromWindow(first, 0x00000002);
        GetMonitorInfo(monitor, ref monitorInfo);
        int monitorWidth = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
        int halfWidth = monitorWidth / 2;

        RECT workArea = new RECT();
        int taskbarHeight = 0;
        if (SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0))
        {
            int screenHeight = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;
            taskbarHeight = screenHeight - workArea.Bottom;
        }
        PositionWindow(first, monitorInfo.rcMonitor.Left, monitorInfo.rcMonitor.Top, halfWidth, monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top - taskbarHeight);
        PositionWindow(second, monitorInfo.rcMonitor.Left + halfWidth, monitorInfo.rcMonitor.Top, halfWidth, monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top - taskbarHeight);
    }
}
