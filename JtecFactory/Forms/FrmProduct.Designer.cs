using System.Drawing;
using System.Windows.Forms;
using JtecFactory.Helpers;

namespace JtecFactory.Forms
{
    public sealed partial class FrmProduct
    {
        private TableLayoutPanel pageLayout;
        private Panel heading;
        private Button backButton;
        private Label titleLabel;
        private Label subtitleLabel;
        private TableLayoutPanel toolbar;
        private TextBox searchTextBox;
        private Button addProductButton;
        private DataGridView productGrid;

        private void InitializeComponent()
        {
            this.pageLayout = new System.Windows.Forms.TableLayoutPanel();
            this.heading = new System.Windows.Forms.Panel();
            this.backButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.toolbar = new System.Windows.Forms.TableLayoutPanel();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.addProductButton = new System.Windows.Forms.Button();
            this.productGrid = new System.Windows.Forms.DataGridView();
            this.codeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.categoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pageLayout.SuspendLayout();
            this.heading.SuspendLayout();
            this.toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.productGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // pageLayout
            // 
            this.pageLayout.BackColor = System.Drawing.Color.White;
            this.pageLayout.ColumnCount = 1;
            this.pageLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pageLayout.Controls.Add(this.heading, 0, 0);
            this.pageLayout.Controls.Add(this.toolbar, 0, 1);
            this.pageLayout.Controls.Add(this.productGrid, 0, 2);
            this.pageLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageLayout.Location = new System.Drawing.Point(0, 0);
            this.pageLayout.Name = "pageLayout";
            this.pageLayout.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.pageLayout.RowCount = 3;
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.pageLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pageLayout.Size = new System.Drawing.Size(900, 600);
            this.pageLayout.TabIndex = 0;
            // 
            // heading
            // 
            this.heading.Controls.Add(this.backButton);
            this.heading.Controls.Add(this.titleLabel);
            this.heading.Controls.Add(this.subtitleLabel);
            this.heading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heading.Location = new System.Drawing.Point(3, 15);
            this.heading.Name = "heading";
            this.heading.Size = new System.Drawing.Size(894, 64);
            this.heading.TabIndex = 0;
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.White;
            this.backButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Location = new System.Drawing.Point(0, 8);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(82, 30);
            this.backButton.TabIndex = 0;
            this.backButton.Text = "Å© Back";
            this.backButton.UseVisualStyleBackColor = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(97, 4);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(117, 32);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Products";
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.AutoSize = true;
            this.subtitleLabel.Location = new System.Drawing.Point(100, 36);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(161, 15);
            this.subtitleLabel.TabIndex = 2;
            this.subtitleLabel.Text = "Manage product information";
            // 
            // toolbar
            // 
            this.toolbar.ColumnCount = 2;
            this.toolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.toolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.toolbar.Controls.Add(this.searchTextBox, 0, 0);
            this.toolbar.Controls.Add(this.addProductButton, 1, 0);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolbar.Location = new System.Drawing.Point(3, 85);
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(0, 4, 0, 10);
            this.toolbar.RowCount = 1;
            this.toolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.toolbar.Size = new System.Drawing.Size(894, 52);
            this.toolbar.TabIndex = 1;
            // 
            // searchTextBox
            // 
            this.searchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchTextBox.Location = new System.Drawing.Point(0, 4);
            this.searchTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(732, 23);
            this.searchTextBox.TabIndex = 0;
            this.searchTextBox.Text = "Search product...";
            // 
            // addProductButton
            // 
            this.addProductButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.addProductButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.addProductButton.FlatAppearance.BorderSize = 0;
            this.addProductButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addProductButton.ForeColor = System.Drawing.Color.White;
            this.addProductButton.Location = new System.Drawing.Point(747, 7);
            this.addProductButton.Name = "addProductButton";
            this.addProductButton.Size = new System.Drawing.Size(144, 32);
            this.addProductButton.TabIndex = 1;
            this.addProductButton.Text = "+  Add Product";
            this.addProductButton.UseVisualStyleBackColor = false;
            // 
            // productGrid
            // 
            this.productGrid.AllowUserToAddRows = false;
            this.productGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.productGrid.BackgroundColor = System.Drawing.Color.White;
            this.productGrid.ColumnHeadersHeight = 40;
            this.productGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.codeColumn,
            this.nameColumn,
            this.categoryColumn,
            this.statusColumn});
            this.productGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.productGrid.Location = new System.Drawing.Point(3, 143);
            this.productGrid.MultiSelect = false;
            this.productGrid.Name = "productGrid";
            this.productGrid.ReadOnly = true;
            this.productGrid.RowHeadersVisible = false;
            this.productGrid.RowTemplate.Height = 38;
            this.productGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.productGrid.Size = new System.Drawing.Size(894, 454);
            this.productGrid.TabIndex = 2;
            // 
            // codeColumn
            // 
            this.codeColumn.HeaderText = "Code";
            this.codeColumn.Name = "codeColumn";
            this.codeColumn.ReadOnly = true;
            // 
            // nameColumn
            // 
            this.nameColumn.HeaderText = "Product Name";
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.ReadOnly = true;
            // 
            // categoryColumn
            // 
            this.categoryColumn.HeaderText = "Category";
            this.categoryColumn.Name = "categoryColumn";
            this.categoryColumn.ReadOnly = true;
            // 
            // statusColumn
            // 
            this.statusColumn.HeaderText = "Status";
            this.statusColumn.Name = "statusColumn";
            this.statusColumn.ReadOnly = true;
            // 
            // FrmProduct
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.pageLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(700, 480);
            this.Name = "FrmProduct";
            this.Text = "Products";
            this.pageLayout.ResumeLayout(false);
            this.heading.ResumeLayout(false);
            this.heading.PerformLayout();
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.productGrid)).EndInit();
            this.ResumeLayout(false);

        }

        private DataGridViewTextBoxColumn codeColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn statusColumn;
    }
}
