using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BigBrother;
using NUnit.Framework;
using System.Windows.Forms;

namespace BigBrother.Tests
{
    [TestFixture]
    [Parallelizable]
    public class VisiblePixelsFixture
    {
        public IEnumerable<WindowInfo> GetOpenWindows()
        {
            var visibleDesktop = new List<Rectangle>();
            visibleDesktop.Add(Screen.AllScreens.Select(scr => scr.Bounds).Aggregate(Rectangle.Union));

            var shellWindow = WinApi.GetShellWindow();
            var windows = new List<WindowInfo>();

            WinApi.EnumWindows((hWnd, lParam) =>
            {
                int len = WinApi.GetWindowTextLength(hWnd);
                if (hWnd != shellWindow && WinApi.IsWindowVisible(hWnd) && len != 0)
                {
                    var windowInfo = new WindowInfo()
                    {
                        Handle = hWnd
                    };

                    var sb = new StringBuilder(len);
                    WinApi.GetWindowText(hWnd, sb, len + 1);
                    windowInfo.TitleText = sb.ToString();

                    var rect1 = new Rect();
                    using (var graphics = Graphics.FromHwnd(hWnd))
                    {
                        windowInfo.RegionType = WinApi.GetClipBox(graphics.GetHdc(), ref rect1);
                    }

                    var rect2 = new Rect();
                    WinApi.GetWindowRect(windowInfo.Handle, ref rect2);

                    windowInfo.Height = rect2.bottom - rect2.top;
                    windowInfo.Width = rect2.right - rect2.left;

                    windows.Add(windowInfo);
                }

                return true;
            }, 0);

            foreach (var openWindow in windows)
            {
                var rect3 = new Rect();
                WinApi.GetWindowRect(openWindow.Handle, ref rect3);
                var windowArea = new Rectangle(rect3.left, rect3.top, rect3.right - rect3.left, rect3.bottom - rect3.top);
                var areasToRemove = new List<Rectangle>();
                var areasToAdd = new List<Rectangle>();

                foreach (var desktopArea in visibleDesktop)
                {
                    if (windowArea.IntersectsWith(desktopArea))
                    {
                        areasToAdd.Add(desktopArea);

                        var intersection = Rectangle.Intersect(desktopArea, windowArea);

                        openWindow.VisiblePixels += (intersection.Width * intersection.Height);

                        var difference = RectDiff(desktopArea, windowArea);
                        areasToAdd.AddRange(difference);
                    }
                }

                foreach (var area in areasToRemove)
                {
                    visibleDesktop.Remove(area);
                }
                visibleDesktop.AddRange(areasToAdd);
            }

            return windows;
        }

        // from https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Rectangle_difference
        public static IEnumerable<Rectangle> RectDiff(Rectangle rectA, Rectangle rectB)
        {
            if (rectB == null || !rectA.IntersectsWith(rectB) || rectB.Contains(rectA))
                return new Rectangle[0];

            var rects = new List<Rectangle>();

            //compute the top rectangle
            int raHeight = rectB.Y - rectA.Y;
            if (raHeight > 0)
            {
                rects.Add(new Rectangle(rectA.X, rectA.Y, rectA.Width, raHeight));
            }

            //compute the bottom rectangle
            int rbY = rectB.Y + rectB.Height;
            int rbHeight = rectA.Height - (rbY - rectA.Y);
            if (rbHeight > 0 && rbY < rectA.Y + rectA.Height)
            {
                rects.Add(new Rectangle(rectA.X, rbY, rectA.Width, rbHeight));
            }

            int rectAYH = rectA.Y + rectA.Height;
            int y1 = Math.Max(rectA.Y, rectB.Y);
            int y2 = Math.Min(rbY, rectAYH);
            int rcHeight = y2 - y1;

            //compute the left rectangle
            int rcWidth = rectB.X - rectA.X;
            if (rcWidth > 0 && rcHeight > 0)
            {
                rects.Add(new Rectangle(rectA.X, y1, rcWidth, rcHeight));
            }

            //compute the right rectangle
            int rbX = rectB.X + rectB.Width;
            int rdWidth = rectA.Width - (rbX - rectA.X);
            if (rdWidth > 0)
            {
                rects.Add(new Rectangle(rbX, y1, rdWidth, rcHeight));
            }

            return rects;
        }

        [Test]
        public void VisualStudioWindowIsFullyVisible()
        {
            var openWindows = GetOpenWindows();

            var visualStudio = openWindows.Where(win => {
                return win.TitleText.EndsWith("Microsoft Visual Studio") &&
                    win.TitleText.Contains("BigBrother");
            }).FirstOrDefault();

            double visiblePercent = (double)visualStudio.VisiblePixels / (double)(visualStudio.Width * visualStudio.Height);

            Assert.That(visiblePercent, Is.EqualTo(1));
        }
    }
}
