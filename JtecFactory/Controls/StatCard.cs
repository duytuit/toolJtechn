using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;

namespace JtecFactory.Controls
{
    internal sealed class StatCard : Panel
    {
        internal StatCard(string title, string value, string glyph)
        {
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(18, 15, 18, 12);

            PictureBox icon = new PictureBox
            {
                Image = UIHelper.CreateGlyphImage(glyph, UIHelper.Primary, 28),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Dock = DockStyle.Fill
            };
            Label titleLabel = UIHelper.CreateLabel(title, 9F, FontStyle.Regular, UIHelper.SecondaryText);
            titleLabel.Dock = DockStyle.Fill;

            Label valueLabel = UIHelper.CreateLabel(value, 24F, FontStyle.Bold, UIHelper.Text);
            valueLabel.Dock = DockStyle.Fill;

            TableLayoutPanel cardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.White
            };
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            cardLayout.Controls.Add(titleLabel, 0, 0);
            cardLayout.Controls.Add(valueLabel, 0, 1);
            cardLayout.Controls.Add(icon, 1, 0);
            cardLayout.SetRowSpan(icon, 2);

            Controls.Add(cardLayout);
        }
    }
}