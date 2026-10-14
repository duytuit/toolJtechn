using System.Drawing;
using System.Windows.Forms;

namespace JtecFactory.Forms
{
    public sealed partial class FrmDashboard
    {
        private TableLayoutPanel pageLayout;
        private Panel heading;
        private Label titleLabel;
        private Label subtitleLabel;
        private TableLayoutPanel stats;
        private TableLayoutPanel lower;

        private void InitializeComponent()
        {
            this.pageLayout = new System.Windows.Forms.TableLayoutPanel();
            this.heading = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.stats = new System.Windows.Forms.TableLayoutPanel();
            this.lower = new System.Windows.Forms.TableLayoutPanel();
            this.pageLayout.SuspendLayout();
            this.heading.SuspendLayout();
            this.SuspendLayout();
            // 
            // pageLayout
            // 
            this.pageLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pageLayout.ColumnCount = 1;
            this.pageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pageLayout.Controls.Add(this.heading, 0, 0);
            this.pageLayout.Controls.Add(this.stats, 0, 1);
            this.pageLayout.Controls.Add(this.lower, 0, 2);
            this.pageLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageLayout.Location = new System.Drawing.Point(25, 23);
            this.pageLayout.Name = "pageLayout";
            this.pageLayout.RowCount = 3;
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pageLayout.Size = new System.Drawing.Size(1130, 715);
            this.pageLayout.TabIndex = 0;
            // 
            // heading
            // 
            this.heading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.heading.Controls.Add(this.titleLabel);
            this.heading.Controls.Add(this.subtitleLabel);
            this.heading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heading.Location = new System.Drawing.Point(3, 3);
            this.heading.Name = "heading";
            this.heading.Size = new System.Drawing.Size(1124, 63);
            this.heading.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.titleLabel.Location = new System.Drawing.Point(0, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(190, 41);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "JTEC Hà Nội";
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.AutoSize = true;
            this.subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.subtitleLabel.Location = new System.Drawing.Point(1, 38);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(252, 17);
            this.subtitleLabel.TabIndex = 1;
            this.subtitleLabel.Text = "Chúc bạn có một ngày làm việc hiệu quả !";
            // 
            // stats
            // 
            this.stats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.stats.ColumnCount = 3;
            this.stats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            this.stats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            this.stats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            this.stats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stats.Location = new System.Drawing.Point(0, 77);
            this.stats.Margin = new System.Windows.Forms.Padding(0, 8, 0, 10);
            this.stats.Name = "stats";
            this.stats.RowCount = 1;
            this.stats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.stats.Size = new System.Drawing.Size(1130, 112);
            this.stats.TabIndex = 1;
            // 
            // lower
            // 
            this.lower.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.lower.ColumnCount = 3;
            this.lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.lower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lower.Location = new System.Drawing.Point(3, 202);
            this.lower.Name = "lower";
            this.lower.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lower.RowCount = 1;
            this.lower.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lower.Size = new System.Drawing.Size(1124, 510);
            this.lower.TabIndex = 2;
            // 
            // FrmDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1180, 760);
            this.Controls.Add(this.pageLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmDashboard";
            this.Padding = new System.Windows.Forms.Padding(25, 23, 25, 22);
            this.Text = "Dashboard";
            this.Load += new System.EventHandler(this.FrmDashboard_Load);
            this.pageLayout.ResumeLayout(false);
            this.heading.ResumeLayout(false);
            this.heading.PerformLayout();
            this.ResumeLayout(false);

        }

    }
}
