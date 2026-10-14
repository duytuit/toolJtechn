using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;
using JtecFactory.Navigation;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Controls
{
    internal sealed partial class SidebarControl : UserControl
    {
        private const int ExpandedWidth = 230;
        private const int CollapsedWidth = 65;
        private readonly Dictionary<int, Button> menuButtons = new Dictionary<int, Button>();
        private bool collapsed;
        private int targetWidth;

        internal event Action<MenuItem> ParentSelected;
        internal event EventHandler LogoutRequested;

        internal SidebarControl()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            InitializeSidebarContent();
        }

        private void InitializeSidebarContent()
        {
            toggleButton.Image = UIHelper.CreateGlyphImage("\u2630", Color.White, 22);
            avatar.Image = UIHelper.CreateGlyphImage("\u25CF", Color.White, 25);
            layout.BackColor = UIHelper.Sidebar;
            this.BackColor = UIHelper.Sidebar;
            ConfigureMenuButton(settingsButton, "System", "\u2699");
            ConfigureMenuButton(logoutButton, "Đăng xuất", "\u21E5");
            settingsButton.Visible = false;
            layout.RowStyles[4].Height = 0F;

            toggleButton.Click += ToggleSidebar;
            logoutButton.Click += LogoutButtonClick;
            animationTimer.Tick += AnimateWidth;
            Resize += delegate { UpdateChildWidths(); };
        }

        private void LogoutButtonClick(object sender, EventArgs e)
        {
            EventHandler handler = LogoutRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal void BindParentMenus(IEnumerable<MenuItem> menus)
        {
            menuPanel.SuspendLayout();
            menuPanel.Controls.Clear();
            menuButtons.Clear();

            foreach (MenuItem menu in menus)
            {
                Button button = CreateMenuButton(menu.Name, menu.Icon);
                button.Tag = menu;
                button.Click += ParentMenuClick;
                menuButtons.Add(menu.Id, button);
                menuPanel.Controls.Add(button);
            }

            menuPanel.ResumeLayout(true);
            UpdateChildWidths();
        }

        private Button CreateMenuButton(string text, string glyph)
        {
            Button button = new Button();
            ConfigureMenuButton(button, text, glyph);
            return button;
        }

        private void ConfigureMenuButton(Button button, string text, string glyph)
        {
            button.Text = text;
            button.Font = UIHelper.BodyFont;
            button.BackColor = UIHelper.Sidebar;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = UIHelper.SidebarSecondary;
            button.FlatAppearance.MouseDownBackColor = UIHelper.SidebarSecondary;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.Height = 42;
            button.Width = 210;
            button.Margin = new Padding(10, 2, 10, 2);
            button.Padding = new Padding(12, 0, 4, 0);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Image = UIHelper.CreateGlyphImage(glyph, Color.White, 21);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        private void ParentMenuClick(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            MenuItem menu = (MenuItem)button.Tag;
            SetActiveParent(menu.Id);
            Action<MenuItem> handler = ParentSelected;
            if (handler != null)
                handler(menu);
        }

        internal void SetActiveParent(int menuId)
        {
            foreach (KeyValuePair<int, Button> item in menuButtons)
            {
                bool active = item.Key == menuId;
                item.Value.BackColor = active ? UIHelper.Primary : UIHelper.Sidebar;
                item.Value.ForeColor = Color.White;
                item.Value.FlatAppearance.MouseOverBackColor = active ? UIHelper.Primary : UIHelper.SidebarSecondary;
            }
        }

        private void ToggleSidebar(object sender, EventArgs e)
        {
            collapsed = !collapsed;
            targetWidth = collapsed ? CollapsedWidth : ExpandedWidth;
            animationTimer.Start();
        }

        private void AnimateWidth(object sender, EventArgs e)
        {
            int distance = targetWidth - Width;
            if (Math.Abs(distance) <= 20)
            {
                Width = targetWidth;
                animationTimer.Stop();
            }
            else
                Width += distance > 0 ? 20 : -20;

            ApplyCollapsedLayout();
        }

        private void ApplyCollapsedLayout()
        {
            bool isCollapsed = Width <= CollapsedWidth + 10;
            appName.Visible = !isCollapsed;
            logo.Location = new Point(isCollapsed ? 15 : 17, 15);
            userName.Visible = !isCollapsed;
            userRole.Visible = !isCollapsed;
            avatar.Location = isCollapsed ? new Point(16, 9) : new Point(9, 9);

            foreach (KeyValuePair<int, Button> item in menuButtons)
                StyleButtonForWidth(item.Value, isCollapsed);
            StyleButtonForWidth(logoutButton, isCollapsed);
            UpdateChildWidths();
        }

        private void StyleButtonForWidth(Button button, bool isCollapsed)
        {
            MenuItem menu = button.Tag as MenuItem;
            string name = menu == null ? "Đăng xuất" : menu.Name;
            button.Text = isCollapsed ? string.Empty : name;
            button.Padding = new Padding(isCollapsed ? 0 : 12, 0, 4, 0);
            button.TextAlign = isCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
            button.ImageAlign = isCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
            button.TextImageRelation = isCollapsed ? TextImageRelation.Overlay : TextImageRelation.ImageBeforeText;
        }

        private void UpdateChildWidths()
        {
            int width = Math.Max(CollapsedWidth - 20, Width - 20);
            foreach (Control control in menuPanel.Controls)
                control.Width = width;
            logoutButton.Width = width;
            Control userArea = layout.GetControlFromPosition(0, 5);
            if (userArea != null && userArea.Controls.Count > 0)
            {
                Control card = userArea.Controls[0];
                card.Width = Math.Max(CollapsedWidth - 18, Width - 32);
                card.Left = (Width - card.Width) / 2;
            }
        }
    }
}