using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Controls
{
    [ToolboxItem(true)]
    [DefaultProperty(nameof(GradientColor1))]
    [DesignerCategory("Code")]
    public class GradientPanel : Panel
    {
        private Color gradientColor1 = Color.White;
        private Color gradientColor2 = Color.LightGray;
        private GradientDirection gradientDirection = GradientDirection.Horizontal;

        public GradientPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
        }

        [Category("Appearance")]
        [Description("Gets or sets the starting color of the gradient fill.")]
        public Color GradientColor1
        {
            get => gradientColor1;
            set
            {
                if (gradientColor1 == value)
                    return;

                gradientColor1 = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Gets or sets the ending color of the gradient fill.")]
        public Color GradientColor2
        {
            get => gradientColor2;
            set
            {
                if (gradientColor2 == value)
                    return;

                gradientColor2 = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the direction of the gradient fill.")]
        [DefaultValue(GradientDirection.Horizontal)]
        public GradientDirection GradientDirection
        {
            get => gradientDirection;
            set
            {
                if (gradientDirection == value)
                    return;

                gradientDirection = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ClientSize.Width == 0 || ClientSize.Height == 0)
            {
                base.OnPaintBackground(e);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var brush = new LinearGradientBrush(ClientRectangle, gradientColor1, gradientColor2, GetGradientAngle()))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private float GetGradientAngle()
        {
            switch (gradientDirection)
            {
                case GradientDirection.Vertical:
                    return 90f;
                case GradientDirection.Diagonal:
                    return 45f;
                default:
                    return 0f;
            }
        }
    }

    public enum GradientDirection
    {
        Horizontal,
        Vertical,
        Diagonal
    }
}

