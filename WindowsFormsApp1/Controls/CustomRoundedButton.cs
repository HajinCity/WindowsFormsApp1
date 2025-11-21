using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace WindowsFormsApp1.Controls
{
    public class CustomRoundedButton : Button
    {
        // Border radius for buttons (adjustable)
        private int borderRadius = 10;

        // Fixed colors for hover and clicked states
        private Color hoverColor = Color.FromArgb(250, 205, 255);
        private Color clickedColor = Color.FromArgb(139, 50, 168);

        // Store original colors
        private Color originalColor;
        private Color originalForeColor;

        // Track if this button is active (clicked)
        private bool isActive = false;

        // Flag to prevent BackColor setter from updating originalColor during programmatic changes
        private bool isUpdatingColor = false;

        // Static reference to track active button across all instances
        private static CustomRoundedButton activeButton = null;

        public CustomRoundedButton()
        {
            // Initialize button properties
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;

            // Store original colors
            originalColor = BackColor;
            originalForeColor = ForeColor;

            // Attach event handlers
            MouseEnter += CustomRoundedButton_MouseEnter;
            MouseLeave += CustomRoundedButton_MouseLeave;
            Click += CustomRoundedButton_Click;
            Paint += CustomRoundedButton_Paint;
            Resize += CustomRoundedButton_Resize;
            HandleCreated += CustomRoundedButton_HandleCreated;

            // Apply rounded corners
            ApplyRoundedCorners();
        }

        // Capture original colors after handle is created (designer values are set)
        private void CustomRoundedButton_HandleCreated(object sender, EventArgs e)
        {
            if (!isActive && (originalColor == SystemColors.Control || originalColor.IsEmpty))
            {
                originalColor = BackColor;
                originalForeColor = ForeColor;
            }
        }

        // Properties for customization
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

        public Color HoverColor
        {
            get { return hoverColor; }
            set
            {
                hoverColor = value;
                Invalidate();
            }
        }

        public Color ClickedColor
        {
            get { return clickedColor; }
            set
            {
                clickedColor = value;
                if (isActive)
                {
                    BackColor = clickedColor;
                    Invalidate();
                }
            }
        }

        // Override BackColor to track original color
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                // Only update originalColor if we're not in the middle of a programmatic color change
                // and the button is not active
                if (!isUpdatingColor && !isActive)
                {
                    // Only update if originalColor is default (Control color) or empty
                    if (originalColor == SystemColors.Control || originalColor.IsEmpty)
                    {
                        originalColor = value;
                    }
                }
                base.BackColor = value;
            }
        }

        // Override ForeColor to track original text color
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                if (!isActive)
                {
                    originalForeColor = value;
                }
                base.ForeColor = value;
            }
        }

        // Apply rounded corners to button (using Region for clipping, but with better quality)
        private void ApplyRoundedCorners()
        {
            if (Width == 0 || Height == 0) return;

            // Use a smoother path with better precision
            GraphicsPath path = new GraphicsPath();
            RectangleF rect = new RectangleF(0, 0, Width, Height);

            float diameter = borderRadius * 2;

            // Use RectangleF for smoother calculations
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Top-left
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Bottom-left
            path.CloseAllFigures();

            // Use Region for clipping, but we'll render with anti-aliasing in Paint
            Region = new Region(path);
        }

        private void CustomRoundedButton_Resize(object sender, EventArgs e)
        {
            ApplyRoundedCorners();
            Invalidate();
        }

        // Custom paint event with high-quality rendering
        private void CustomRoundedButton_Paint(object sender, PaintEventArgs e)
        {
            if (Width == 0 || Height == 0) return;

            Graphics g = e.Graphics;

            // Set high-quality rendering settings
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Create rounded rectangle path with precise coordinates
            GraphicsPath path = new GraphicsPath();
            RectangleF rect = new RectangleF(0, 0, Width, Height);

            float diameter = borderRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Top-left
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Bottom-left
            path.CloseAllFigures();

            // Fill the button with its background color
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw image if present
            if (Image != null)
            {
                RectangleF imageRect = GetImageRectangle(rect);
                if (imageRect.Width > 0 && imageRect.Height > 0)
                {
                    // Use high-quality image rendering
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // If button is active, convert image to white using ColorMatrix
                    if (isActive)
                    {
                        using (ImageAttributes imageAttributes = new ImageAttributes())
                        {
                            // Create a ColorMatrix to convert image to white while preserving transparency
                            // This matrix converts any color to white by:
                            // 1. Removing original color (set RGB channels to 0)
                            // 2. Adding white color (set translation values to 1 for RGB)
                            // 3. Preserving alpha channel
                            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                            {
                                new float[] {0, 0, 0, 0, 0},      // Red channel - remove original
                                new float[] {0, 0, 0, 0, 0},      // Green channel - remove original
                                new float[] {0, 0, 0, 0, 0},      // Blue channel - remove original
                                new float[] {0, 0, 0, 1, 0},      // Alpha channel - preserve
                                new float[] {1, 1, 1, 0, 1}       // Translation - add white (1,1,1)
                            });

                            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                            g.DrawImage(Image,
                                new Rectangle((int)imageRect.X, (int)imageRect.Y, (int)imageRect.Width, (int)imageRect.Height),
                                0, 0, Image.Width, Image.Height,
                                GraphicsUnit.Pixel, imageAttributes);
                        }
                    }
                    else
                    {
                        // Draw image normally when not active
                        g.DrawImage(Image, imageRect);
                    }
                }
            }

            // Draw text with high quality
            if (!string.IsNullOrEmpty(Text))
            {
                RectangleF textRect = GetTextRectangle(rect);
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                {
                    StringFormat format = new StringFormat
                    {
                        Alignment = GetHorizontalAlignment(),
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(Text, Font, textBrush, textRect, format);
                }
            }
        }

        private StringAlignment GetHorizontalAlignment()
        {
            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return StringAlignment.Near;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Center;
            }
        }

        private RectangleF GetImageRectangle(RectangleF buttonRect)
        {
            if (Image == null) return RectangleF.Empty;

            float padding = 8; // Padding from edges
            SizeF imageSize = Image.Size;
            float x = 0, y = 0;

            // Default to MiddleLeft if not set (most common for left-side icons)
            ContentAlignment alignment = ImageAlign;
            if (alignment == ContentAlignment.TopLeft) // Default value
            {
                alignment = ContentAlignment.MiddleLeft;
            }

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    x = padding; y = padding;
                    break;
                case ContentAlignment.TopCenter:
                    x = (buttonRect.Width - imageSize.Width) / 2; y = padding;
                    break;
                case ContentAlignment.TopRight:
                    x = buttonRect.Width - imageSize.Width - padding; y = padding;
                    break;
                case ContentAlignment.MiddleLeft:
                    x = padding; y = (buttonRect.Height - imageSize.Height) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    x = (buttonRect.Width - imageSize.Width) / 2; y = (buttonRect.Height - imageSize.Height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    x = buttonRect.Width - imageSize.Width - padding; y = (buttonRect.Height - imageSize.Height) / 2;
                    break;
                case ContentAlignment.BottomLeft:
                    x = padding; y = buttonRect.Height - imageSize.Height - padding;
                    break;
                case ContentAlignment.BottomCenter:
                    x = (buttonRect.Width - imageSize.Width) / 2; y = buttonRect.Height - imageSize.Height - padding;
                    break;
                case ContentAlignment.BottomRight:
                    x = buttonRect.Width - imageSize.Width - padding; y = buttonRect.Height - imageSize.Height - padding;
                    break;
            }

            return new RectangleF(x, y, imageSize.Width, imageSize.Height);
        }

        private RectangleF GetTextRectangle(RectangleF buttonRect)
        {
            float padding = 8;
            float imageWidth = 0;
            float spacing = 8; // Space between image and text

            // Account for image width if present and aligned to left
            if (Image != null)
            {
                ContentAlignment alignment = ImageAlign;
                if (alignment == ContentAlignment.TopLeft) // Default value, treat as MiddleLeft
                {
                    alignment = ContentAlignment.MiddleLeft;
                }

                if (alignment == ContentAlignment.MiddleLeft ||
                    alignment == ContentAlignment.TopLeft ||
                    alignment == ContentAlignment.BottomLeft)
                {
                    imageWidth = Image.Width + spacing; // Image width + spacing
                }
            }

            return new RectangleF(
                padding + imageWidth,
                padding,
                buttonRect.Width - (padding * 2) - imageWidth,
                buttonRect.Height - (padding * 2)
            );
        }

        private void CustomRoundedButton_MouseEnter(object sender, EventArgs e)
        {
            // Only change to hover color if button is not active
            if (!isActive)
            {
                isUpdatingColor = true;
                BackColor = hoverColor;
                isUpdatingColor = false;
                Invalidate();
            }
        }

        private void CustomRoundedButton_MouseLeave(object sender, EventArgs e)
        {
            // Only revert to original color if button is not active
            if (!isActive)
            {
                isUpdatingColor = true;
                BackColor = originalColor;
                ForeColor = originalForeColor;
                isUpdatingColor = false;
                Invalidate();
            }
        }

        private void CustomRoundedButton_Click(object sender, EventArgs e)
        {
            // Reset previously active button to its original colors
            if (activeButton != null && activeButton != this)
            {
                activeButton.isActive = false;
                activeButton.isUpdatingColor = true;
                activeButton.BackColor = activeButton.originalColor;
                activeButton.ForeColor = activeButton.originalForeColor;
                activeButton.isUpdatingColor = false;
                activeButton.Invalidate();
            }

            // Set this button as active and apply clicked color with white text
            isActive = true;
            activeButton = this;
            isUpdatingColor = true;
            BackColor = clickedColor;
            ForeColor = Color.White;
            isUpdatingColor = false;
            Invalidate();
        }

        // Method to reset button to original state
        public void Reset()
        {
            if (isActive)
            {
                isActive = false;
                if (activeButton == this)
                {
                    activeButton = null;
                }
                isUpdatingColor = true;
                BackColor = originalColor;
                ForeColor = originalForeColor;
                isUpdatingColor = false;
                Invalidate();
            }
        }

        // Method to set as active programmatically
        public void SetActive()
        {
            CustomRoundedButton_Click(this, EventArgs.Empty);
        }
    }
}
