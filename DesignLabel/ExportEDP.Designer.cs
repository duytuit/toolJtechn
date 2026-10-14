
namespace WindowsFormsApp5
{
    partial class ExportEDP
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.rbGop = new System.Windows.Forms.RadioButton();
            this.rbTach = new System.Windows.Forms.RadioButton();
            this.rtbMSP = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rtbMSP);
            this.groupBox1.Controls.Add(this.rbTach);
            this.groupBox1.Controls.Add(this.rbGop);
            this.groupBox1.Controls.Add(this.btnExport);
            this.groupBox1.Location = new System.Drawing.Point(6, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(247, 549);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bảng điều khiển";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(7, 17);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(232, 23);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Xuất Excel";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // rbGop
            // 
            this.rbGop.AutoSize = true;
            this.rbGop.Location = new System.Drawing.Point(21, 47);
            this.rbGop.Name = "rbGop";
            this.rbGop.Size = new System.Drawing.Size(61, 17);
            this.rbGop.TabIndex = 2;
            this.rbGop.Text = "Gộp file";
            this.rbGop.UseVisualStyleBackColor = true;
            this.rbGop.CheckedChanged += new System.EventHandler(this.rbGop_CheckedChanged);
            // 
            // rbTach
            // 
            this.rbTach.AutoSize = true;
            this.rbTach.Checked = true;
            this.rbTach.Location = new System.Drawing.Point(129, 47);
            this.rbTach.Name = "rbTach";
            this.rbTach.Size = new System.Drawing.Size(66, 17);
            this.rbTach.TabIndex = 3;
            this.rbTach.TabStop = true;
            this.rbTach.Text = "Tách file";
            this.rbTach.UseVisualStyleBackColor = true;
            this.rbTach.CheckedChanged += new System.EventHandler(this.rbTach_CheckedChanged);
            // 
            // rtbMSP
            // 
            this.rtbMSP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rtbMSP.Location = new System.Drawing.Point(6, 70);
            this.rtbMSP.Name = "rtbMSP";
            this.rtbMSP.Size = new System.Drawing.Size(235, 473);
            this.rtbMSP.TabIndex = 4;
            this.rtbMSP.Text = "";
            this.rtbMSP.TextChanged += new System.EventHandler(this.rtbMSP_TextChanged);
            // 
            // ExportEDP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 557);
            this.Controls.Add(this.groupBox1);
            this.Name = "ExportEDP";
            this.Text = "ExportEDP";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbTach;
        private System.Windows.Forms.RadioButton rbGop;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.RichTextBox rtbMSP;
    }
}