using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Controls
{
    [ToolboxItem(true)]
    public class CustomRoundedPanel : Panel
    {
        // Border radius for panel (adjustable)
        private int borderRadius = 10;

        public CustomRoundedPanel()
        {
            // Initialize panel properties
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // Attach event handlers
            Paint += CustomRoundedPanel_Paint;
            Resize += CustomRoundedPanel_Resize;
        }

        // Property for border radius customization
        public int BorderRadius
        {
            get { return borderRadius; }
            set
            {
                borderRadius = value;
                ApplyRoundedCorners();
                Invalidate();
            }
        }

        // Apply rounded corners to panel using Region for clipping
        private void ApplyRoundedCorners()
        {
            if (Width == 0 || Height == 0) return;

            // Use a smooth path with better precision
            GraphicsPath path = new GraphicsPath();
            RectangleF rect = new RectangleF(0, 0, Width, Height);

            float diameter = borderRadius * 2;

            // Create rounded rectangle path
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Top-left
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Bottom-left
            path.CloseAllFigures();

            // Use Region for clipping
            Region = new Region(path);
        }

        private void CustomRoundedPanel_Resize(object sender, EventArgs e)
        {
            ApplyRoundedCorners();
            Invalidate();
        }

        // Custom paint event with high-quality rendering
        private void CustomRoundedPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Width == 0 || Height == 0) return;

            Graphics g = e.Graphics;

            // Set high-quality rendering settings
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Create rounded rectangle path with precise coordinates
            GraphicsPath path = new GraphicsPath();
            RectangleF rect = new RectangleF(0, 0, Width, Height);

            float diameter = borderRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Top-left
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Bottom-left
            path.CloseAllFigures();

            // Fill the panel with its background color
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border if BorderStyle is set (optional - you can add border color property if needed)
            if (BorderStyle != BorderStyle.None)
            {
                using (Pen pen = new Pen(ForeColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }
}