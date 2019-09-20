using System;
using System.Runtime.InteropServices;

namespace ARKWatchdog
{
    // static 語言變數
    public static class WindowsServices // 讓鼠標忽略該軟件
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            if (isA)
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, oriStyle);
            }
            else
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, transparentStyle);
            }
            isA = (isA) ? false : true;
        }
        public static void SetOriStyle(IntPtr hwnd)
        {
            oriStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            transparentStyle = oriStyle | WS_EX_TRANSPARENT;
            SetWindowLong(hwnd, GWL_EXSTYLE, transparentStyle);
            isA = true;
        }
        private static int oriStyle;
        private static int transparentStyle;
        private static bool isA;
    }
}
