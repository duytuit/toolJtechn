
namespace AppVtec
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Label databaseStatusLabel;
        private System.Windows.Forms.GroupBox screenModeGroup;
        private System.Windows.Forms.FlowLayoutPanel screenModeLayout;
        private System.Windows.Forms.RadioButton oneScreenRadioButton;
        private System.Windows.Forms.RadioButton twoScreensRadioButton;
        private System.Windows.Forms.RadioButton threeScreensRadioButton;
        private System.Windows.Forms.Label scanLabel;
        private System.Windows.Forms.TextBox scanTextBox;
        private System.Windows.Forms.Button openPdfButton;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox pdfPathTextBox;
        private System.Windows.Forms.Label basePathLabel;
        private System.Windows.Forms.TextBox basePathTextBox;
        private System.Windows.Forms.Button saveBasePathButton;
        private System.Windows.Forms.Label messageLabel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.databaseStatusLabel = new System.Windows.Forms.Label();
            this.screenModeGroup = new System.Windows.Forms.GroupBox();
            this.screenModeLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.oneScreenRadioButton = new System.Windows.Forms.RadioButton();
            this.twoScreensRadioButton = new System.Windows.Forms.RadioButton();
            this.threeScreensRadioButton = new System.Windows.Forms.RadioButton();
            this.scanLabel = new System.Windows.Forms.Label();
            this.scanTextBox = new System.Windows.Forms.TextBox();
            this.openPdfButton = new System.Windows.Forms.Button();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pdfPathTextBox = new System.Windows.Forms.TextBox();
            this.basePathLabel = new System.Windows.Forms.Label();
            this.basePathTextBox = new System.Windows.Forms.TextBox();
            this.saveBasePathButton = new System.Windows.Forms.Button();
            this.messageLabel = new System.Windows.Forms.Label();
            this.layout.SuspendLayout();
            this.screenModeGroup.SuspendLayout();
            this.screenModeLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // layout
            // 
            this.layout.AutoScroll = true;
            this.layout.ColumnCount = 3;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layout.Controls.Add(this.databaseStatusLabel, 0, 0);
            this.layout.Controls.Add(this.screenModeGroup, 0, 1);
            this.layout.Controls.Add(this.scanLabel, 0, 2);
            this.layout.Controls.Add(this.scanTextBox, 1, 2);
            this.layout.Controls.Add(this.openPdfButton, 2, 2);
            this.layout.Controls.Add(this.pathLabel, 0, 3);
            this.layout.Controls.Add(this.pdfPathTextBox, 1, 3);
            this.layout.Controls.Add(this.basePathLabel, 0, 4);
            this.layout.Controls.Add(this.basePathTextBox, 1, 4);
            this.layout.Controls.Add(this.saveBasePathButton, 2, 4);
            this.layout.Controls.Add(this.messageLabel, 0, 5);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(0, 0);
            this.layout.Name = "layout";
            this.layout.Padding = new System.Windows.Forms.Padding(12);
            this.layout.RowCount = 6;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.layout.Size = new System.Drawing.Size(504, 320);
            this.layout.TabIndex = 0;
            // 
            // databaseStatusLabel
            // 
            this.databaseStatusLabel.AutoEllipsis = true;
            this.layout.SetColumnSpan(this.databaseStatusLabel, 3);
            this.databaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.databaseStatusLabel.Location = new System.Drawing.Point(15, 12);
            this.databaseStatusLabel.Name = "databaseStatusLabel";
            this.databaseStatusLabel.Size = new System.Drawing.Size(474, 29);
            this.databaseStatusLabel.TabIndex = 0;
            this.databaseStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // screenModeGroup
            // 
            this.layout.SetColumnSpan(this.screenModeGroup, 3);
            this.screenModeGroup.Controls.Add(this.screenModeLayout);
            this.screenModeGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenModeGroup.Location = new System.Drawing.Point(15, 44);
            this.screenModeGroup.MinimumSize = new System.Drawing.Size(0, 70);
            this.screenModeGroup.Name = "screenModeGroup";
            this.screenModeGroup.Size = new System.Drawing.Size(474, 70);
            this.screenModeGroup.TabIndex = 1;
            this.screenModeGroup.TabStop = false;
            this.screenModeGroup.Text = "Chế độ mở PDF";
            // 
            // screenModeLayout
            // 
            this.screenModeLayout.Controls.Add(this.oneScreenRadioButton);
            this.screenModeLayout.Controls.Add(this.twoScreensRadioButton);
            this.screenModeLayout.Controls.Add(this.threeScreensRadioButton);
            this.screenModeLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenModeLayout.Location = new System.Drawing.Point(3, 16);
            this.screenModeLayout.Name = "screenModeLayout";
            this.screenModeLayout.Padding = new System.Windows.Forms.Padding(8, 4, 0, 0);
            this.screenModeLayout.Size = new System.Drawing.Size(468, 51);
            this.screenModeLayout.TabIndex = 0;
            // 
            // oneScreenRadioButton
            // 
            this.oneScreenRadioButton.AutoSize = true;
            this.oneScreenRadioButton.Checked = true;
            this.oneScreenRadioButton.Location = new System.Drawing.Point(11, 7);
            this.oneScreenRadioButton.Name = "oneScreenRadioButton";
            this.oneScreenRadioButton.Size = new System.Drawing.Size(77, 17);
            this.oneScreenRadioButton.TabIndex = 0;
            this.oneScreenRadioButton.TabStop = true;
            this.oneScreenRadioButton.Text = "1 màn hình";
            this.oneScreenRadioButton.UseVisualStyleBackColor = true;
            // 
            // twoScreensRadioButton
            // 
            this.twoScreensRadioButton.AutoSize = true;
            this.twoScreensRadioButton.Location = new System.Drawing.Point(94, 7);
            this.twoScreensRadioButton.Name = "twoScreensRadioButton";
            this.twoScreensRadioButton.Size = new System.Drawing.Size(77, 17);
            this.twoScreensRadioButton.TabIndex = 1;
            this.twoScreensRadioButton.Text = "2 màn hình";
            this.twoScreensRadioButton.UseVisualStyleBackColor = true;
            // 
            // threeScreensRadioButton
            // 
            this.threeScreensRadioButton.AutoSize = true;
            this.threeScreensRadioButton.Location = new System.Drawing.Point(177, 7);
            this.threeScreensRadioButton.Name = "threeScreensRadioButton";
            this.threeScreensRadioButton.Size = new System.Drawing.Size(77, 17);
            this.threeScreensRadioButton.TabIndex = 2;
            this.threeScreensRadioButton.Text = "3 màn hình";
            this.threeScreensRadioButton.UseVisualStyleBackColor = true;
            // 
            // scanLabel
            // 
            this.scanLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.scanLabel.AutoSize = true;
            this.scanLabel.Location = new System.Drawing.Point(15, 130);
            this.scanLabel.Name = "scanLabel";
            this.scanLabel.Size = new System.Drawing.Size(51, 13);
            this.scanLabel.TabIndex = 2;
            this.scanLabel.Text = "Mã scan:";
            // 
            // scanTextBox
            // 
            this.scanTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scanTextBox.Location = new System.Drawing.Point(108, 118);
            this.scanTextBox.Name = "scanTextBox";
            this.scanTextBox.Size = new System.Drawing.Size(300, 38);
            this.scanTextBox.TabIndex = 3;
            this.scanTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScanTextBox_KeyDown);
            // 
            // openPdfButton
            // 
            this.openPdfButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openPdfButton.Location = new System.Drawing.Point(414, 118);
            this.openPdfButton.Name = "openPdfButton";
            this.openPdfButton.Size = new System.Drawing.Size(75, 38);
            this.openPdfButton.TabIndex = 4;
            this.openPdfButton.Text = "Mở PDF";
            this.openPdfButton.UseVisualStyleBackColor = true;
            this.openPdfButton.Click += new System.EventHandler(this.OpenPdfButton_Click);
            // 
            // pathLabel
            // 
            this.pathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(15, 170);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(87, 13);
            this.pathLabel.TabIndex = 5;
            this.pathLabel.Text = "Đường dẫn PDF:";
            // 
            // pdfPathTextBox
            // 
            this.layout.SetColumnSpan(this.pdfPathTextBox, 2);
            this.pdfPathTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfPathTextBox.Location = new System.Drawing.Point(108, 162);
            this.pdfPathTextBox.Name = "pdfPathTextBox";
            this.pdfPathTextBox.ReadOnly = true;
            this.pdfPathTextBox.Size = new System.Drawing.Size(381, 29);
            this.pdfPathTextBox.TabIndex = 6;
            // 
            // basePathLabel
            // 
            this.basePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.basePathLabel.AutoSize = true;
            this.basePathLabel.Location = new System.Drawing.Point(15, 209);
            this.basePathLabel.Name = "basePathLabel";
            this.basePathLabel.Size = new System.Drawing.Size(76, 13);
            this.basePathLabel.TabIndex = 7;
            this.basePathLabel.Text = "Thư mục PDF:";
            // 
            // basePathTextBox
            // 
            this.basePathTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.basePathTextBox.Location = new System.Drawing.Point(108, 197);
            this.basePathTextBox.Name = "basePathTextBox";
            this.basePathTextBox.Size = new System.Drawing.Size(300, 38);
            this.basePathTextBox.TabIndex = 8;
            // 
            // saveBasePathButton
            // 
            this.saveBasePathButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveBasePathButton.Location = new System.Drawing.Point(414, 197);
            this.saveBasePathButton.Name = "saveBasePathButton";
            this.saveBasePathButton.Size = new System.Drawing.Size(75, 38);
            this.saveBasePathButton.TabIndex = 9;
            this.saveBasePathButton.Text = "Lưu đường dẫn";
            this.saveBasePathButton.UseVisualStyleBackColor = true;
            this.saveBasePathButton.Click += new System.EventHandler(this.SaveBasePathButton_Click);
            // 
            // messageLabel
            // 
            this.messageLabel.AutoEllipsis = true;
            this.layout.SetColumnSpan(this.messageLabel, 3);
            this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageLabel.ForeColor = System.Drawing.Color.DimGray;
            this.messageLabel.Location = new System.Drawing.Point(15, 238);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(474, 70);
            this.messageLabel.TabIndex = 10;
            this.messageLabel.Text = "Quét mã rồi nhấn Enter để tra cứu và mở PDF.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 320);
            this.Controls.Add(this.layout);
            this.MinimumSize = new System.Drawing.Size(520, 300);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VTEC PDF Viewer";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.screenModeGroup.ResumeLayout(false);
            this.screenModeLayout.ResumeLayout(false);
            this.screenModeLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}

