using System;
using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Navigation
{
    internal sealed class MenuCard : UserControl
    {
        private readonly Label iconLabel;
        private readonly Label nameLabel;
        private readonly Label descriptionLabel;
        private bool hovered;
        private bool selected;

        internal MenuCard(MenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Size = new Size(254, 132);
            MinimumSize = new Size(220, 132);
            Margin = new Padding(0, 0, 14, 14);
            Padding = new Padding(14, 12, 14, 10);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            TabStop = true;

            iconLabel = new Label
            {
                AutoSize = false,
                Text = item.Icon,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Symbol", 17F),
                ForeColor = UIHelper.Primary,
                BackColor = Color.FromArgb(239, 246, 255),
                Location = new Point(14, 12),
                Size = new Size(34, 34)
            };
            nameLabel = new Label
            {
                AutoSize = false,
                Text = item.Name,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UIHelper.Text,
                Location = new Point(14, 54),
                Size = new Size(220, 23)
            };
            descriptionLabel = new Label
            {
                AutoSize = false,
                Text = item.Description,
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.SecondaryText,
                Location = new Point(14, 79),
                Size = new Size(220, 38)
            };

            Controls.Add(iconLabel);
            Controls.Add(nameLabel);
            Controls.Add(descriptionLabel);
            WirePointerEvents(this);
        }

        internal event EventHandler Selected;

        internal bool IsSelected
        {
            get { return selected; }
            set
            {
                selected = value;
                BackColor = selected ? Color.FromArgb(239, 246, 255) : Color.White;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color border = hovered || selected ? UIHelper.Primary : UIHelper.Border;
            using (Pen pen = new Pen(border))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void WirePointerEvents(Control control)
        {
            control.MouseEnter += PointerEntered;
            control.MouseLeave += PointerLeft;
            control.Click += RaiseSelected;
            foreach (Control child in control.Controls)
                WirePointerEvents(child);
        }

        private void PointerEntered(object sender, EventArgs e)
        {
            hovered = true;
            Invalidate();
        }

        private void PointerLeft(object sender, EventArgs e)
        {
            Point pointer = PointToClient(Cursor.Position);
            hovered = ClientRectangle.Contains(pointer);
            Invalidate();
        }

        private void RaiseSelected(object sender, EventArgs e)
        {
            EventHandler handler = Selected;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}