using System;
using System.Runtime.InteropServices;

namespace ARKServerQuery
{
    // 讓使用者的滑鼠無視監控標籤
    public static class WindowsServices
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = -20;

        public static void SaveOriginStyle(IntPtr hwnd)
        {
            oriStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            transparentStyle = oriStyle | WS_EX_TRANSPARENT;

            SetWindowLong(hwnd, GWL_EXSTYLE, transparentStyle);

            isA = true;
        }

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, (isA) ? oriStyle : transparentStyle);

            isA = !isA;
        }

        private static int oriStyle;

        private static int transparentStyle;

        private static bool isA;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}
