
namespace WindowsFormsApp5
{
    partial class PrintLotNo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.btnPrintLotNo = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnOpenFormEdp = new System.Windows.Forms.Button();
            this.btnMau = new System.Windows.Forms.Button();
            this.btnLoadLotNo = new System.Windows.Forms.Button();
            this.rtbLotNo = new System.Windows.Forms.RichTextBox();
            this.plPriviewPageLotNo = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtbWarning = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPrintLotNo
            // 
            this.btnPrintLotNo.Location = new System.Drawing.Point(8, 32);
            this.btnPrintLotNo.Name = "btnPrintLotNo";
            this.btnPrintLotNo.Size = new System.Drawing.Size(180, 23);
            this.btnPrintLotNo.TabIndex = 0;
            this.btnPrintLotNo.Text = "Xem trước";
            this.btnPrintLotNo.UseVisualStyleBackColor = true;
            this.btnPrintLotNo.Click += new System.EventHandler(this.btnPrintLotNo_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 616F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 358F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.plPriviewPageLotNo, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(987, 565);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.btnOpenFormEdp);
            this.panel2.Controls.Add(this.btnMau);
            this.panel2.Controls.Add(this.btnLoadLotNo);
            this.panel2.Controls.Add(this.btnPrintLotNo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(619, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(365, 559);
            this.panel2.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(195, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(165, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Xuất dữ liệu EDP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnOpenFormEdp
            // 
            this.btnOpenFormEdp.Location = new System.Drawing.Point(8, 3);
            this.btnOpenFormEdp.Name = "btnOpenFormEdp";
            this.btnOpenFormEdp.Size = new System.Drawing.Size(180, 23);
            this.btnOpenFormEdp.TabIndex = 4;
            this.btnOpenFormEdp.Text = "In EDP";
            this.btnOpenFormEdp.UseVisualStyleBackColor = true;
            this.btnOpenFormEdp.Click += new System.EventHandler(this.btnOpenFormEdp_Click);
            // 
            // btnMau
            // 
            this.btnMau.Location = new System.Drawing.Point(195, 32);
            this.btnMau.Name = "btnMau";
            this.btnMau.Size = new System.Drawing.Size(165, 23);
            this.btnMau.TabIndex = 3;
            this.btnMau.Text = "Chỉnh Mẫu";
            this.btnMau.UseVisualStyleBackColor = true;
            this.btnMau.Click += new System.EventHandler(this.btnMau_Click);
            // 
            // btnLoadLotNo
            // 
            this.btnLoadLotNo.Location = new System.Drawing.Point(8, 61);
            this.btnLoadLotNo.Name = "btnLoadLotNo";
            this.btnLoadLotNo.Size = new System.Drawing.Size(180, 23);
            this.btnLoadLotNo.TabIndex = 2;
            this.btnLoadLotNo.Text = "Tải dữ liệu";
            this.btnLoadLotNo.UseVisualStyleBackColor = true;
            this.btnLoadLotNo.Click += new System.EventHandler(this.btnLoadLotNo_Click);
            // 
            // rtbLotNo
            // 
            this.rtbLotNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rtbLotNo.Location = new System.Drawing.Point(4, 16);
            this.rtbLotNo.Name = "rtbLotNo";
            this.rtbLotNo.Size = new System.Drawing.Size(174, 419);
            this.rtbLotNo.TabIndex = 1;
            this.rtbLotNo.Text = "";
            // 
            // plPriviewPageLotNo
            // 
            this.plPriviewPageLotNo.Location = new System.Drawing.Point(3, 3);
            this.plPriviewPageLotNo.Name = "plPriviewPageLotNo";
            this.plPriviewPageLotNo.Size = new System.Drawing.Size(610, 559);
            this.plPriviewPageLotNo.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.rtbWarning);
            this.groupBox1.Location = new System.Drawing.Point(194, 120);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(166, 442);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cảnh báo trùng";
            // 
            // rtbWarning
            // 
            this.rtbWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rtbWarning.Location = new System.Drawing.Point(6, 16);
            this.rtbWarning.Name = "rtbWarning";
            this.rtbWarning.Size = new System.Drawing.Size(156, 419);
            this.rtbWarning.TabIndex = 0;
            this.rtbWarning.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.rtbLotNo);
            this.groupBox2.Location = new System.Drawing.Point(4, 120);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(184, 442);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mã Lot";
            // 
            // PrintLotNo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(989, 561);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PrintLotNo";
            this.Text = "PrintLotNo";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPrintLotNo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox rtbLotNo;
        private System.Windows.Forms.Panel plPriviewPageLotNo;
        private System.Windows.Forms.Button btnLoadLotNo;
        private System.Windows.Forms.Button btnMau;
        private System.Windows.Forms.Button btnOpenFormEdp;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox rtbWarning;
    }
}