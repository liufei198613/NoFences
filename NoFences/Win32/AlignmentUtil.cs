using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace NoFences.Win32
{
    public class AlignmentUtil
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static Point CalculateSnappedPosition(Rectangle rect, List<Rectangle> otherFences, Rectangle screenBounds, int snapThreshold)
        {
            Point result = rect.Location;

            // Snap to screen edges
            if (Math.Abs(rect.Left - screenBounds.Left) < snapThreshold)
                result.X = screenBounds.Left;
            if (Math.Abs(rect.Right - screenBounds.Right) < snapThreshold)
                result.X = screenBounds.Right - rect.Width;
            if (Math.Abs(rect.Top - screenBounds.Top) < snapThreshold)
                result.Y = screenBounds.Top;
            if (Math.Abs(rect.Bottom - screenBounds.Bottom) < snapThreshold)
                result.Y = screenBounds.Bottom - rect.Height;

            // Snap to other fences
            foreach (var fence in otherFences)
            {
                // Left edge snapping
                if (Math.Abs(rect.Left - fence.Right) < snapThreshold)
                    result.X = fence.Right;
                if (Math.Abs(rect.Right - fence.Left) < snapThreshold)
                    result.X = fence.Left - rect.Width;

                // Right edge snapping
                if (Math.Abs(rect.Left - fence.Left) < snapThreshold)
                    result.X = fence.Left;
                if (Math.Abs(rect.Right - fence.Right) < snapThreshold)
                    result.X = fence.Right - rect.Width;

                // Top edge snapping
                if (Math.Abs(rect.Top - fence.Bottom) < snapThreshold)
                    result.Y = fence.Bottom;
                if (Math.Abs(rect.Bottom - fence.Top) < snapThreshold)
                    result.Y = fence.Top - rect.Height;

                // Bottom edge snapping
                if (Math.Abs(rect.Top - fence.Top) < snapThreshold)
                    result.Y = fence.Top;
                if (Math.Abs(rect.Bottom - fence.Bottom) < snapThreshold)
                    result.Y = fence.Bottom - rect.Height;

                // Corner snapping
                if (Math.Abs(rect.Left - fence.Right) < snapThreshold &&
                    Math.Abs(rect.Top - fence.Bottom) < snapThreshold)
                {
                    result.X = fence.Right;
                    result.Y = fence.Bottom;
                }
            }

            return result;
        }

        public static List<Rectangle> GetAllFenceWindows(IntPtr excludeHandle)
        {
            var fenceWindows = new List<Rectangle>();

            EnumWindows((hWnd, lParam) =>
            {
                var length = GetWindowText(hWnd, null, 0);
                if (length == 0)
                    return true;

                var builder = new StringBuilder(length + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                var title = builder.ToString();

                if (hWnd == excludeHandle)
                    return true;

                if (GetWindowRect(hWnd, out RECT rect))
                {
                    fenceWindows.Add(rect.ToRectangle());
                }

                return true;
            }, IntPtr.Zero);

            return fenceWindows;
        }

        public static Point AlignToGrid(Point position, int gridSize = 50)
        {
            return new Point(
                (int)Math.Round(position.X / (double)gridSize) * gridSize,
                (int)Math.Round(position.Y / (double)gridSize) * gridSize
            );
        }

        public enum EdgeAlignmentType
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public static EdgeAlignmentType DetectEdgeAlignment(Rectangle source, Rectangle target, int tolerance = 10)
        {
            bool leftAligned = Math.Abs(source.Left - target.Left) < tolerance;
            bool rightAligned = Math.Abs(source.Right - target.Right) < tolerance;
            bool topAligned = Math.Abs(source.Top - target.Top) < tolerance;
            bool bottomAligned = Math.Abs(source.Bottom - target.Bottom) < tolerance;

            if (leftAligned && topAligned) return EdgeAlignmentType.TopLeft;
            if (rightAligned && topAligned) return EdgeAlignmentType.TopRight;
            if (leftAligned && bottomAligned) return EdgeAlignmentType.BottomLeft;
            if (rightAligned && bottomAligned) return EdgeAlignmentType.BottomRight;
            if (leftAligned) return EdgeAlignmentType.Left;
            if (rightAligned) return EdgeAlignmentType.Right;
            if (topAligned) return EdgeAlignmentType.Top;
            if (bottomAligned) return EdgeAlignmentType.Bottom;

            return EdgeAlignmentType.None;
        }
    }
}
