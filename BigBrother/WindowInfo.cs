using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.Text;
using BigBrother.Helpers;

namespace BigBrother
{
    public class WindowInfo
    {
        public IntPtr Handle;
        public string TitleText;
        public int Width;
        public int Height;
        public int VisiblePixels;
        public int RegionType;

        /// <summary>
        /// Gets a list of information for all open windows.
        /// Shells, invisible windows, and windows without any text are ignored.
        /// </summary>
        /// <returns></returns>
        public static List<WindowInfo> GetOpenWindows()
        {
            var shellWindow = WinApi.GetShellWindow();
            var windows = new List<WindowInfo>();
            foreach (var hWnd in GetWindowHandles())
            {
                if (hWnd != shellWindow && WinApi.IsWindowVisible(hWnd) && WinApi.GetWindowTextLength(hWnd) != 0)
                {
                    windows.Add(Populate(hWnd));
                }
            }
            return windows;
        }

        private static List<IntPtr> GetWindowHandles()
        {
            var handles = new List<IntPtr>();
            WinApi.EnumWindows((hWnd, lParam) =>
            {
                handles.Add(hWnd);
                return true;
            }, 0);
            return handles;
        }

        /// <summary>
        /// Populates a <see cref="WindowInfo"/> object using the window handle.
        /// </summary>
        /// <param name="hWnd">Handle for an open window.</param>
        /// <returns></returns>
        public static WindowInfo Populate(IntPtr hWnd)
        {
            var info = new WindowInfo();
            info.Handle = hWnd;

            int windowTextLength = WinApi.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(windowTextLength);
            WinApi.GetWindowText(hWnd, sb, windowTextLength + 1);
            info.TitleText = sb.ToString();

            var rect = new Rect();
            WinApi.GetWindowRect(info.Handle, ref rect);
            info.Width = rect.Right - rect.Left;
            info.Height = rect.Bottom - rect.Top;
            
            using (var graphics = Graphics.FromHwnd(hWnd))
            {
                info.RegionType = WinApi.GetClipBox(graphics.GetHdc(), ref rect);
            }

            return info;
        }

        public static void CalculateVisiblePixels(List<WindowInfo> openWindows)
        {
            var desktop = new List<Rectangle>()
            {
                Screen.AllScreens.Select(screen => screen.Bounds).Aggregate(Rectangle.Union)
            };

            foreach (var window in openWindows)
            {
                var areasToRemove = new List<Rectangle>();
                var areasToAdd = new List<Rectangle>();

                var rect = new Rect();
                WinApi.GetWindowRect(window.Handle, ref rect);
                var windowArea = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                foreach (var desktopArea in desktop)
                {
                    if (windowArea.IntersectsWith(desktopArea))
                    {
                        var intersection = Rectangle.Intersect(desktopArea, windowArea);
                        window.VisiblePixels += (intersection.Width * intersection.Height);

                        var difference = RectangleHelper.FourZoneDifference(desktopArea, windowArea);
                        areasToAdd.AddRange(difference);
                        areasToRemove.Add(desktopArea);
                    }
                }

                foreach
            }

            
        }
    }
}
