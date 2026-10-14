using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace JtecFactory.Helpers
{
    internal static class UIHelper
    {
        internal static readonly Color Primary = ColorTranslator.FromHtml("#2563EB");
        internal static readonly Color Sidebar = ColorTranslator.FromHtml("#111827");
        internal static readonly Color SidebarSecondary = ColorTranslator.FromHtml("#1F2937");
        internal static readonly Color Background = ColorTranslator.FromHtml("#F5F7FA");
        internal static readonly Color Text = ColorTranslator.FromHtml("#111827");
        internal static readonly Color SecondaryText = ColorTranslator.FromHtml("#6B7280");
        internal static readonly Color Border = ColorTranslator.FromHtml("#E5E7EB");
        internal static readonly Font BodyFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        internal static readonly Font SmallFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        internal static readonly Font HeadingFont = new Font("Segoe UI", 22F, FontStyle.Bold);

        internal static Label CreateLabel(string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent
            };
        }

        internal static Button CreateButton(string text, Color backColor, Color foreColor)
        {
            return new Button
            {
                Text = text,
                Font = BodyFont,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
        }

        internal static Bitmap CreateGlyphImage(string glyph, Color color, int size)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Font font = new Font("Segoe UI Symbol", size * 0.72F, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Brush brush = new SolidBrush(color))
            {
                graphics.Clear(Color.Transparent);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(glyph, font, brush, new PointF(0, -1));
            }
            return bitmap;
        }

        internal static Bitmap CreateLogoImage(int size)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (GraphicsPath path = new GraphicsPath())
            using (Brush background = new SolidBrush(Primary))
            using (Brush mark = new SolidBrush(Color.White))
            using (Font font = new Font("Segoe UI", size * 0.43F, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                path.AddArc(0, 0, size - 1, size - 1, 0, 360);
                graphics.FillPath(background, path);
                graphics.DrawString("J", font, mark, new PointF(size * 0.29F, size * 0.16F));
            }
            return bitmap;
        }

        internal static Panel CreateSurface()
        {
            return new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0)
            };
        }
    }
}