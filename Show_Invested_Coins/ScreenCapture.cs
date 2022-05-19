using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Show_Invested_Coins
{
    internal class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        public static Image CaptureDesktop()
        {
            return CaptureWindow(GetDesktopWindow(), 0);
        }

        public static Bitmap CaptureActiveWindow(IntPtr handle, int counter)
        {
            return CaptureWindow(handle, counter);
        }

        public static Bitmap CaptureWindow(IntPtr handle, int counter)
        {
            var rect = new Rect();
            GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top + ((rect.Bottom - rect.Top) / 3)*2, rect.Right - rect.Left, (rect.Bottom - rect.Top)/3);
            var result = new Bitmap(1, 1);
            try
            {
                Debug.Print("["+counter+"]bounds: "+bounds.Width + " " + bounds.Height);
                result = new Bitmap(bounds.Width, bounds.Height);
            }
            catch(System.ArgumentException) {
                return null;
            }

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }

        public static Bitmap CaptureWindowOrig(IntPtr handle)
        {
            var rect = new Rect();
            GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(1, 1);
            try
            {
                Debug.Print("bounds: " + bounds.Width + " " + bounds.Height);
                result = new Bitmap(bounds.Width, bounds.Height);
            }
            catch (System.ArgumentException)
            {
                return null;
            }

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }
    }
}
