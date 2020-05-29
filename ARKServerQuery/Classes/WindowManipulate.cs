using System;
using System.Windows.Input;

namespace ARKServerQuery.Classes
{
    public class WindowManipulate
    {
        public void Init(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            WindowsServices.SaveOriginStyle(hwnd);
        }

        public void KeyDetect()
        {
            bool isKeyDown =
                ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);

            bool isManipulatable =
                (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0)
                || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;

            if (isKeyDown) ToggleManipulateWindow(KeyStates.Down);
            else if (isManipulatable) ToggleManipulateWindow(KeyStates.None);
        }

        private IntPtr hwnd;

        private bool canManipulateWindow = false;

        private KeyStates gKeyStates = KeyStates.None;

        // 改變可操縱視窗狀態並保存目前狀態，由按鍵狀態的改變來致能 (None -> Down or Down -> None)
        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            if (inKeyStates != gKeyStates)
            {
                canManipulateWindow = !canManipulateWindow;

                WindowsServices.SetWindowExTransparent(hwnd);

                gKeyStates = inKeyStates;
            }
        }
    }
}
