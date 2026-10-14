using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;

namespace JtecFactory.Forms
{
    public partial class BaseFunctionForm
    {
        private TableLayoutPanel rootLayout;
        private Panel header;
        private Button backButton;
        private Label titleLabel;
        private Label descriptionLabel;
        protected Panel contentPanel;
        private FlowLayoutPanel actionPanel;

        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.header = new System.Windows.Forms.Panel();
            this.backButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.actionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.rootLayout.SuspendLayout();
            this.header.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.BackColor = UIHelper.Background;
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.header, 0, 0);
            this.rootLayout.Controls.Add(this.contentPanel, 0, 1);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(24, 18, 24, 20);
            this.rootLayout.RowCount = 2;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(900, 600);
            this.rootLayout.TabIndex = 0;
            // 
            // header
            // 
            this.header.BackColor = UIHelper.Background;
            this.header.Controls.Add(this.backButton);
            this.header.Controls.Add(this.titleLabel);
            this.header.Controls.Add(this.descriptionLabel);
            this.header.Controls.Add(this.actionPanel);
            this.header.Dock = System.Windows.Forms.DockStyle.Fill;
            this.header.Location = new System.Drawing.Point(27, 21);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(846, 82);
            this.header.TabIndex = 0;
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.White;
            this.backButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.FlatAppearance.BorderColor = UIHelper.Border;
            this.backButton.Location = new System.Drawing.Point(0, 5);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(82, 30);
            this.backButton.TabIndex = 0;
            this.backButton.Text = "\u2190 Back";
            this.backButton.UseVisualStyleBackColor = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = UIHelper.HeadingFont;
            this.titleLabel.ForeColor = UIHelper.Text;
            this.titleLabel.Location = new System.Drawing.Point(98, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(126, 41);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Function";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Font = UIHelper.SmallFont;
            this.descriptionLabel.ForeColor = UIHelper.SecondaryText;
            this.descriptionLabel.Location = new System.Drawing.Point(100, 44);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(0, 13);
            this.descriptionLabel.TabIndex = 2;
            // 
            // actionPanel
            // 
            this.actionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionPanel.AutoSize = true;
            this.actionPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.actionPanel.BackColor = UIHelper.Background;
            this.actionPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.actionPanel.Location = new System.Drawing.Point(580, 8);
            this.actionPanel.Name = "actionPanel";
            this.actionPanel.Size = new System.Drawing.Size(0, 0);
            this.actionPanel.TabIndex = 3;
            this.actionPanel.WrapContents = false;
            // 
            // contentPanel
            // 
            this.contentPanel.BackColor = System.Drawing.Color.White;
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(27, 109);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(18);
            this.contentPanel.Size = new System.Drawing.Size(846, 468);
            this.contentPanel.TabIndex = 1;
            // 
            // BaseFunctionForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(600, 420);
            this.Name = "BaseFunctionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Function";
            this.rootLayout.ResumeLayout(false);
            this.header.ResumeLayout(false);
            this.header.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}