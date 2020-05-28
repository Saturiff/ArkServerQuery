using System;
using System.Windows.Input;

namespace ARKServerQuery.Classes
{
    public class WindowManipulate
    {
        public void Init(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            WindowsServices.SetOriStyle(hwnd);
        }

        public void KeyDetect()
        {
            bool isKeyDown = ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0) ||
                ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);

            bool isManipulatable = (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0) ||
                ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;

            if (isKeyDown) ToggleManipulateWindow(KeyStates.Down);
            else if (isManipulatable) ToggleManipulateWindow(KeyStates.None);
        }

        private IntPtr hwnd;

        private bool canManipulateWindow = false;

        private KeyStates gKeyStates = KeyStates.None;

        // None -> Down, Down -> None : 改變可操縱視窗狀態並保存目前狀態，視窗可移動時將停止伺服器訪問以增進使用者體驗
        // None -> None, Down -> Down : 不做任何事
        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            if (inKeyStates != gKeyStates) // 狀態改變則致能
            {
                canManipulateWindow = !canManipulateWindow;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
            }
        }
    }
}
