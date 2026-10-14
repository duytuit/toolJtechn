using System.Drawing;
using System.Windows.Forms;

namespace JtecFactory.Forms
{
    public sealed partial class FrmLogin
    {
        private Panel card;
        private TableLayoutPanel fields;
        private PictureBox logo;
        private Label titleLabel;
        private Label descriptionLabel;
        private Label usernameLabel;
        private TextBox usernameTextBox;
        private Panel spacer;
        private Button loginButton;
        private Label versionLabel;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLogin));
            this.card = new System.Windows.Forms.Panel();
            this.fields = new System.Windows.Forms.TableLayoutPanel();
            this.logo = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.spacer = new System.Windows.Forms.Panel();
            this.loginButton = new System.Windows.Forms.Button();
            this.versionLabel = new System.Windows.Forms.Label();
            this.card.SuspendLayout();
            this.fields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.SuspendLayout();
            // 
            // card
            // 
            this.card.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.card.BackColor = System.Drawing.Color.White;
            this.card.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.card.Controls.Add(this.fields);
            this.card.Location = new System.Drawing.Point(-28, -40);
            this.card.Margin = new System.Windows.Forms.Padding(0);
            this.card.Name = "card";
            this.card.Size = new System.Drawing.Size(430, 570);
            this.card.TabIndex = 0;
            // 
            // fields
            // 
            this.fields.BackColor = System.Drawing.Color.White;
            this.fields.ColumnCount = 1;
            this.fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.fields.Controls.Add(this.logo, 0, 0);
            this.fields.Controls.Add(this.titleLabel, 0, 1);
            this.fields.Controls.Add(this.descriptionLabel, 0, 2);
            this.fields.Controls.Add(this.usernameLabel, 0, 3);
            this.fields.Controls.Add(this.usernameTextBox, 0, 4);
            this.fields.Controls.Add(this.spacer, 0, 5);
            this.fields.Controls.Add(this.loginButton, 0, 6);
            this.fields.Controls.Add(this.versionLabel, 0, 7);
            this.fields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fields.Location = new System.Drawing.Point(0, 0);
            this.fields.Margin = new System.Windows.Forms.Padding(0);
            this.fields.Name = "fields";
            this.fields.Padding = new System.Windows.Forms.Padding(38, 26, 38, 20);
            this.fields.RowCount = 8;
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.fields.Size = new System.Drawing.Size(428, 568);
            this.fields.TabIndex = 0;
            // 
            // logo
            // 
            this.logo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.logo.Image = ((System.Drawing.Image)(resources.GetObject("logo.Image")));
            this.logo.Location = new System.Drawing.Point(185, 30);
            this.logo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(58, 58);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.logo.TabIndex = 0;
            this.logo.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 21F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.titleLabel.Location = new System.Drawing.Point(62, 104);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(303, 38);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Nhà Máy JTEC Hà Nội";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.descriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.descriptionLabel.Location = new System.Drawing.Point(214, 147);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(0, 17);
            this.descriptionLabel.TabIndex = 2;
            // 
            // usernameLabel
            // 
            this.usernameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.usernameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.usernameLabel.Location = new System.Drawing.Point(38, 179);
            this.usernameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(59, 15);
            this.usernameLabel.TabIndex = 3;
            this.usernameLabel.Text = "Tài khoản";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.usernameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usernameTextBox.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.usernameTextBox.Location = new System.Drawing.Point(41, 202);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(346, 36);
            this.usernameTextBox.TabIndex = 4;
            // 
            // spacer
            // 
            this.spacer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spacer.Location = new System.Drawing.Point(41, 246);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(346, 8);
            this.spacer.TabIndex = 5;
            // 
            // loginButton
            // 
            this.loginButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.loginButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.loginButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loginButton.FlatAppearance.BorderSize = 0;
            this.loginButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loginButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.loginButton.ForeColor = System.Drawing.Color.White;
            this.loginButton.Location = new System.Drawing.Point(41, 260);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(346, 40);
            this.loginButton.TabIndex = 5;
            this.loginButton.Text = "Đăng nhập";
            this.loginButton.UseVisualStyleBackColor = false;
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.versionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.versionLabel.Location = new System.Drawing.Point(178, 533);
            this.versionLabel.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(72, 15);
            this.versionLabel.TabIndex = 6;
            this.versionLabel.Text = "Version 1.0.0";
            // 
            // FrmLogin
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(484, 611);
            this.Controls.Add(this.card);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(500, 650);
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập | Jtec Factory";
            this.card.ResumeLayout(false);
            this.fields.ResumeLayout(false);
            this.fields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
