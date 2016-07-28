using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BigBrother.Helpers
{
    public static class RectangleHelper
    {
        /// <summary>
        /// 4-zone Rectangle Difference Algorithm.
        /// Converted from code at https://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Rectangle_difference
        /// </summary>
        /// <param name="rectA">First rectangle</param>
        /// <param name="rectB">Second rectangle</param>
        public static IEnumerable<Rectangle> FourZoneDifference(Rectangle rectA, Rectangle rectB)
        {
            var rects = new List<Rectangle>();

            if (rectB == null || !rectA.IntersectsWith(rectB) || rectB.Contains(rectA))
                return rects;

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
    }
}
