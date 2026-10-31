
namespace WindowsFormsApp5
{
    partial class PrintEDP
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.plPriviewPageLotNo = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnOpenCondition = new System.Windows.Forms.Button();
            this.btnLoadLotNo = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnPrintLotNo = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.rtbLotNo = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 616F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.plPriviewPageLotNo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1185, 562);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // plPriviewPageLotNo
            // 
            this.plPriviewPageLotNo.Location = new System.Drawing.Point(3, 3);
            this.plPriviewPageLotNo.Name = "plPriviewPageLotNo";
            this.plPriviewPageLotNo.Size = new System.Drawing.Size(610, 555);
            this.plPriviewPageLotNo.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.btnOpenCondition);
            this.panel2.Controls.Add(this.btnLoadLotNo);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Controls.Add(this.btnPrintLotNo);
            this.panel2.Controls.Add(this.btnExportExcel);
            this.panel2.Controls.Add(this.rtbLotNo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(619, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(563, 556);
            this.panel2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Location = new System.Drawing.Point(371, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(185, 556);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Điều chỉnh in";
            // 
            // btnOpenCondition
            // 
            this.btnOpenCondition.Location = new System.Drawing.Point(6, 40);
            this.btnOpenCondition.Name = "btnOpenCondition";
            this.btnOpenCondition.Size = new System.Drawing.Size(165, 23);
            this.btnOpenCondition.TabIndex = 5;
            this.btnOpenCondition.Text = "Cài đặt điều kiện in";
            this.btnOpenCondition.UseVisualStyleBackColor = true;
            this.btnOpenCondition.Click += new System.EventHandler(this.btnOpenCondition_Click);
            // 
            // btnLoadLotNo
            // 
            this.btnLoadLotNo.Location = new System.Drawing.Point(6, 98);
            this.btnLoadLotNo.Name = "btnLoadLotNo";
            this.btnLoadLotNo.Size = new System.Drawing.Size(165, 23);
            this.btnLoadLotNo.TabIndex = 4;
            this.btnLoadLotNo.Text = "Nhập Excel";
            this.btnLoadLotNo.UseVisualStyleBackColor = true;
            this.btnLoadLotNo.Click += new System.EventHandler(this.btnLoadLotNo_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Location = new System.Drawing.Point(181, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(185, 556);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thống kê kết quả";
            // 
            // btnPrintLotNo
            // 
            this.btnPrintLotNo.Location = new System.Drawing.Point(6, 69);
            this.btnPrintLotNo.Name = "btnPrintLotNo";
            this.btnPrintLotNo.Size = new System.Drawing.Size(165, 23);
            this.btnPrintLotNo.TabIndex = 3;
            this.btnPrintLotNo.Text = "Xem trước / in";
            this.btnPrintLotNo.UseVisualStyleBackColor = true;
            this.btnPrintLotNo.Click += new System.EventHandler(this.btnPrintLotNo_Click);
            //
            // btnExportExcel
            //
            this.btnExportExcel.Location = new System.Drawing.Point(6, 127);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(165, 23);
            this.btnExportExcel.TabIndex = 7;
            this.btnExportExcel.Text = "Xuất Excel kết quả";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // rtbLotNo
            // 
            this.rtbLotNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rtbLotNo.Location = new System.Drawing.Point(1, 159);
            this.rtbLotNo.Name = "rtbLotNo";
            this.rtbLotNo.Size = new System.Drawing.Size(176, 397);
            this.rtbLotNo.TabIndex = 2;
            this.rtbLotNo.Text = "Chưa nhập file Excel";
            // 
            // PrintEDP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 562);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PrintEDP";
            this.Text = "PrintEDP";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox rtbLotNo;
        private System.Windows.Forms.Panel plPriviewPageLotNo;
        private System.Windows.Forms.Button btnLoadLotNo;
        private System.Windows.Forms.Button btnPrintLotNo;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnOpenCondition;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}
