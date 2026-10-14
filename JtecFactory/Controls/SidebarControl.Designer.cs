using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;

namespace JtecFactory.Controls
{
    internal sealed partial class SidebarControl
    {
        private IContainer components;
        private TableLayoutPanel layout;
        private FlowLayoutPanel menuPanel;
        private Label appName;
        private Label userName;
        private Label userRole;
        private PictureBox logo;
        private PictureBox avatar;
        private Button toggleButton;
        private Button settingsButton;
        private Button logoutButton;
        private Panel brand;
        private Panel separator;
        private Panel userArea;
        private Panel userCard;
        private Timer animationTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SidebarControl));
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.brand = new System.Windows.Forms.Panel();
            this.logo = new System.Windows.Forms.PictureBox();
            this.appName = new System.Windows.Forms.Label();
            this.toggleButton = new System.Windows.Forms.Button();
            this.menuPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.separator = new System.Windows.Forms.Panel();
            this.settingsButton = new System.Windows.Forms.Button();
            this.userArea = new System.Windows.Forms.Panel();
            this.userCard = new System.Windows.Forms.Panel();
            this.avatar = new System.Windows.Forms.PictureBox();
            this.userName = new System.Windows.Forms.Label();
            this.userRole = new System.Windows.Forms.Label();
            this.logoutButton = new System.Windows.Forms.Button();
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.layout.SuspendLayout();
            this.brand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.userArea.SuspendLayout();
            this.userCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.avatar)).BeginInit();
            this.SuspendLayout();
            // 
            // layout
            // 
            this.layout.ColumnCount = 1;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Controls.Add(this.brand, 0, 0);
            this.layout.Controls.Add(this.toggleButton, 0, 1);
            this.layout.Controls.Add(this.menuPanel, 0, 2);
            this.layout.Controls.Add(this.separator, 0, 3);
            this.layout.Controls.Add(this.settingsButton, 0, 4);
            this.layout.Controls.Add(this.userArea, 0, 5);
            this.layout.Controls.Add(this.logoutButton, 0, 6);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(0, 0);
            this.layout.Name = "layout";
            this.layout.RowCount = 7;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.layout.Size = new System.Drawing.Size(230, 517);
            this.layout.TabIndex = 0;
            // 
            // brand
            // 
            this.brand.Controls.Add(this.logo);
            this.brand.Controls.Add(this.appName);
            this.brand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.brand.Location = new System.Drawing.Point(3, 3);
            this.brand.Name = "brand";
            this.brand.Size = new System.Drawing.Size(224, 60);
            this.brand.TabIndex = 0;
            // 
            // logo
            // 
            this.logo.Image = ((System.Drawing.Image)(resources.GetObject("logo.Image")));
            this.logo.Location = new System.Drawing.Point(17, 15);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(36, 36);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.logo.TabIndex = 0;
            this.logo.TabStop = false;
            // 
            // appName
            // 
            this.appName.AutoSize = true;
            this.appName.BackColor = System.Drawing.Color.Transparent;
            this.appName.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.appName.ForeColor = System.Drawing.Color.White;
            this.appName.Location = new System.Drawing.Point(62, 24);
            this.appName.Name = "appName";
            this.appName.Size = new System.Drawing.Size(136, 25);
            this.appName.TabIndex = 1;
            this.appName.Text = "JTEC FACTORY";
            // 
            // toggleButton
            // 
            this.toggleButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toggleButton.FlatAppearance.BorderSize = 0;
            this.toggleButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toggleButton.ForeColor = System.Drawing.Color.White;
            this.toggleButton.Location = new System.Drawing.Point(3, 69);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(224, 38);
            this.toggleButton.TabIndex = 1;
            this.toggleButton.UseVisualStyleBackColor = false;
            // 
            // menuPanel
            // 
            this.menuPanel.AutoScroll = true;
            this.menuPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.menuPanel.Location = new System.Drawing.Point(3, 113);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Padding = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.menuPanel.Size = new System.Drawing.Size(224, 238);
            this.menuPanel.TabIndex = 2;
            this.menuPanel.WrapContents = false;
            // 
            // separator
            // 
            this.separator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.separator.Location = new System.Drawing.Point(14, 354);
            this.separator.Margin = new System.Windows.Forms.Padding(14, 0, 14, 0);
            this.separator.Name = "separator";
            this.separator.Size = new System.Drawing.Size(202, 1);
            this.separator.TabIndex = 3;
            // 
            // settingsButton
            // 
            this.settingsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.settingsButton.FlatAppearance.BorderSize = 0;
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.ForeColor = System.Drawing.Color.White;
            this.settingsButton.Location = new System.Drawing.Point(3, 358);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(75, 23);
            this.settingsButton.TabIndex = 4;
            this.settingsButton.UseVisualStyleBackColor = false;
            // 
            // userArea
            // 
            this.userArea.Controls.Add(this.userCard);
            this.userArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userArea.Location = new System.Drawing.Point(3, 406);
            this.userArea.Name = "userArea";
            this.userArea.Size = new System.Drawing.Size(224, 60);
            this.userArea.TabIndex = 5;
            // 
            // userCard
            // 
            this.userCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userCard.Controls.Add(this.avatar);
            this.userCard.Controls.Add(this.userName);
            this.userCard.Controls.Add(this.userRole);
            this.userCard.Location = new System.Drawing.Point(5, 7);
            this.userCard.Name = "userCard";
            this.userCard.Size = new System.Drawing.Size(222, 50);
            this.userCard.TabIndex = 0;
            // 
            // avatar
            // 
            this.avatar.Location = new System.Drawing.Point(9, 9);
            this.avatar.Name = "avatar";
            this.avatar.Size = new System.Drawing.Size(32, 32);
            this.avatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.avatar.TabIndex = 0;
            this.avatar.TabStop = false;
            // 
            // userName
            // 
            this.userName.AutoSize = true;
            this.userName.BackColor = System.Drawing.Color.Transparent;
            this.userName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.userName.ForeColor = System.Drawing.Color.White;
            this.userName.Location = new System.Drawing.Point(49, 8);
            this.userName.Name = "userName";
            this.userName.Size = new System.Drawing.Size(33, 15);
            this.userName.TabIndex = 1;
            this.userName.Text = "User";
            // 
            // userRole
            // 
            this.userRole.AutoSize = true;
            this.userRole.BackColor = System.Drawing.Color.Transparent;
            this.userRole.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.userRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.userRole.Location = new System.Drawing.Point(49, 26);
            this.userRole.Name = "userRole";
            this.userRole.Size = new System.Drawing.Size(77, 13);
            this.userRole.TabIndex = 2;
            this.userRole.Text = "Administrator";
            // 
            // logoutButton
            // 
            this.logoutButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logoutButton.FlatAppearance.BorderSize = 0;
            this.logoutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logoutButton.ForeColor = System.Drawing.Color.White;
            this.logoutButton.Location = new System.Drawing.Point(3, 472);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(75, 23);
            this.logoutButton.TabIndex = 6;
            this.logoutButton.UseVisualStyleBackColor = false;
            // 
            // animationTimer
            // 
            this.animationTimer.Interval = 10;
            // 
            // SidebarControl
            // 
            this.Controls.Add(this.layout);
            this.MinimumSize = new System.Drawing.Size(65, 0);
            this.Name = "SidebarControl";
            this.Size = new System.Drawing.Size(230, 517);
            this.layout.ResumeLayout(false);
            this.brand.ResumeLayout(false);
            this.brand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.userArea.ResumeLayout(false);
            this.userCard.ResumeLayout(false);
            this.userCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.avatar)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
