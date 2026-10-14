using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JtecFactory.Controls;
using JtecFactory.Helpers;

namespace JtecFactory.Forms
{
    public sealed partial class FrmDashboard : Form
    {
        public FrmDashboard()
        {
            InitializeComponent();
            InitializeStatCards();
            InitializeActivities();
        }

        internal FrmDashboard(string pageTitle)
        {
            InitializeComponent();
            titleLabel.Text = pageTitle;
            InitializeStatCards();
            InitializeActivities();
        }

        private void InitializeStatCards()
        {
          
        }

        private void InitializeActivities()
        {
          
        }

        private void AddActivity(string text, string time, bool highlighted)
        {
            Panel row = new Panel
            {
                Size = new Size(180, 58),
                Margin = new Padding(0, 0, 0, 8)
            };
            Panel dot = new Panel
            {
                BackColor = highlighted ? UIHelper.Primary : UIHelper.Border,
                Size = new Size(8, 8),
                Location = new Point(1, 8)
            };
            Label action = new Label
            {
                Text = text,
                Location = new Point(18, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = UIHelper.Text
            };
            Label timeLabel = new Label
            {
                Text = time,
                Location = new Point(18, 23),
                AutoSize = true,
                Font = UIHelper.SmallFont,
                ForeColor = UIHelper.SecondaryText
            };
            row.Controls.Add(dot);
            row.Controls.Add(action);
            row.Controls.Add(timeLabel);
        }

        private void DrawChart(object sender, PaintEventArgs e)
        {
            Panel chart = (Panel)sender;
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(4, 8, chart.ClientSize.Width - 12, chart.ClientSize.Height - 16);
            if (bounds.Width < 40 || bounds.Height < 40)
                return;

            using (Pen gridPen = new Pen(UIHelper.Border))
            using (Pen linePen = new Pen(UIHelper.Primary, 3F))
            using (Brush pointBrush = new SolidBrush(UIHelper.Primary))
            {
                for (int row = 0; row < 4; row++)
                {
                    int y = bounds.Top + row * bounds.Height / 3;
                    graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
                }
                Point[] points =
                {
                    new Point(bounds.Left, bounds.Top + bounds.Height * 2 / 3),
                    new Point(bounds.Left + bounds.Width / 5, bounds.Top + bounds.Height / 2),
                    new Point(bounds.Left + bounds.Width * 2 / 5, bounds.Top + bounds.Height * 3 / 5),
                    new Point(bounds.Left + bounds.Width * 3 / 5, bounds.Top + bounds.Height / 3),
                    new Point(bounds.Left + bounds.Width * 4 / 5, bounds.Top + bounds.Height * 2 / 5),
                    new Point(bounds.Right, bounds.Top + bounds.Height / 5)
                };
                graphics.DrawLines(linePen, points);
                foreach (Point point in points)
                    graphics.FillEllipse(pointBrush, point.X - 3, point.Y - 3, 6, 6);
            }
        }


        private void FrmDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}