using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Controls;
using JtecFactory.Helpers;

namespace JtecFactory.Forms
{
    public sealed partial class FrmMain
    {
        private Panel contentPanel;
        private SidebarControl sidebar;

        private void InitializeComponent()
        {
            contentPanel = new Panel();
            sidebar = new SidebarControl();
            SuspendLayout();

            Text = "Jtec Factory";
            BackColor = UIHelper.Background;
            ClientSize = new Size(1200, 760);
            MinimumSize = new Size(940, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = UIHelper.Background;
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 230;

            Controls.Add(contentPanel);
            Controls.Add(sidebar);
            ResumeLayout(false);
        }
    }
}
