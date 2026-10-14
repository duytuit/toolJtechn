using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace WindowsFormsApp5
{
    public partial class DesignForm : Form
    {
        private readonly TextBox txtProductCode = new TextBox();
        private readonly TextBox txtProductName = new TextBox();
        private readonly NumericUpDown numQuantity = new NumericUpDown();
        private readonly WebBrowser preview = new WebBrowser();
        private string currentHtml = string.Empty;

        public DesignForm()
        {
            InitializeComponent();
            BuildDemoUi();
        }

        private void BuildDemoUi()
        {
            Text = "Demo phiếu chỉ thị công việc HTML + Barcode";
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;

            var inputPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var title = new Label
            {
                Text = "Dữ liệu tem",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            inputPanel.Controls.Add(title);

            AddInput(inputPanel, "Mã sản phẩm", txtProductCode, "SP-001", 48);
            AddInput(inputPanel, "Tên sản phẩm", txtProductName, "Sản phẩm mẫu", 128);

            var quantityLabel = new Label { Text = "Số lượng", Location = new Point(16, 208), AutoSize = true };
            numQuantity.Location = new Point(16, 230);
            numQuantity.Size = new Size(220, 24);
            numQuantity.Minimum = 1;
            numQuantity.Maximum = 99999;
            numQuantity.Value = 1;
            inputPanel.Controls.Add(quantityLabel);
            inputPanel.Controls.Add(numQuantity);

            var previewButton = new Button
            {
                Text = "Xem trước",
                Location = new Point(16, 280),
                Size = new Size(220, 34)
            };
            previewButton.Click += (sender, args) => RenderLabel();

            var printButton = new Button
            {
                Text = "In tem",
                Location = new Point(16, 324),
                Size = new Size(220, 34)
            };
            printButton.Click += PrintLabel;
            inputPanel.Controls.Add(previewButton);
            inputPanel.Controls.Add(printButton);

            preview.Dock = DockStyle.Fill;
            preview.AllowNavigation = false;
            preview.ScriptErrorsSuppressed = true;

            Controls.Add(preview);
            Controls.Add(inputPanel);
            Shown += (sender, args) => RenderLabel();
        }

        private static void AddInput(Control parent, string labelText, TextBox textBox, string value, int top)
        {
            parent.Controls.Add(new Label
            {
                Text = labelText,
                Location = new Point(16, top),
                AutoSize = true
            });
            textBox.Text = value;
            textBox.Location = new Point(16, top + 22);
            textBox.Size = new Size(220, 24);
            parent.Controls.Add(textBox);
        }

        private void RenderLabel()
        {
                        var productCode = txtProductCode.Text.Trim();
                        var productName = txtProductName.Text.Trim();
                        var productQuantity = numQuantity.Value.ToString();

            currentHtml = @"
<!doctype html>
<html><head><meta charset='utf-8'><style>
@page { size: A4 portrait; margin: 8mm; }
html, body { margin: 0; padding: 0; background: #eeeeee; }
body { font-family: Arial, 'Yu Gothic', sans-serif; color: #111; font-size: 9pt; }
.sheet { width: 190mm; margin: 0 auto; background: white; }
.document-title { position: relative; height: 18mm; text-align: center; font-size: 20pt;
                                    font-weight: bold; padding-top: 3mm; box-sizing: border-box; }
.issue { position: absolute; right: 0; top: 1mm; text-align: left; font-size: 9pt; font-weight: normal; }
.company { font-size: 11pt; font-weight: bold; margin-top: 2mm; }
.process { page-break-inside: avoid; margin-bottom: 3mm; }
.process-title { font-size: 15pt; font-weight: bold; margin: 1mm 0; }
table { width: 100%; border-collapse: collapse; table-layout: fixed; }
td { border: 1px solid #111; padding: 1.2mm; vertical-align: middle; height: 7mm; }
.label-cell { width: 15mm; font-weight: bold; }
.wide { width: 48mm; }
.medium { width: 30mm; }
.large-text { font-size: 13pt; font-weight: bold; }
.barcode-cell { width: 47mm; text-align: center; padding: 1mm; }
.barcode { width: 43mm; height: 14mm; }
.notes { height: 16mm; vertical-align: top; }
.small { font-size: 8pt; }
</style></head><body><div class='sheet'>
<div class='document-title'>作業指示書
    <div class='issue'>発行日　2026/09/04<br><span class='company'>東亜電機工業株式会社</span></div>
</div>

<div class='process'><div class='process-title'>切圧工程</div>
<table><tr><td class='label-cell'>場所</td><td class='wide'>切圧</td><td>製造課切圧係</td><td>製番　2608001253-0001</td><td>着手日　2026/08/20</td></tr>
<tr><td class='label-cell'>品目区分</td><td>3</td><td class='label-cell'>品目コード</td><td class='large-text'>{{ProductCode}}</td><td class='label-cell'>数量　{{Quantity}}</td></tr>
<tr><td>備考1</td><td colspan='2'></td><td>納期　2026/09/03</td><td rowspan='2' class='barcode-cell'>発注SEQ　4187625 0<br><img class='barcode' src='data:image/png;base64,{{Barcode1}}'></td></tr>
<tr><td>備考2</td><td colspan='2'>（自動発注更新）</td><td>本数　11　　工数　12.69</td></tr>
<tr><td colspan='5' class='notes'>工程備考</td></tr></table></div>

<div class='process'><div class='process-title'>挿入工程</div>
<table><tr><td class='label-cell'>場所</td><td class='wide'>挿入</td><td>製造課組付係挿入</td><td>製番　2608001253-0001</td><td>着手日　2026/09/03</td></tr>
<tr><td class='label-cell'>品目区分</td><td>2</td><td class='label-cell'>品目コード</td><td class='large-text'>{{ProductCode}}</td><td class='label-cell'>数量　{{Quantity}}</td></tr>
<tr><td>備考1</td><td colspan='2'></td><td>納期　2026/09/08</td><td rowspan='2' class='barcode-cell'>発注SEQ　4187624 0<br><img class='barcode' src='data:image/png;base64,{{Barcode2}}'></td></tr>
<tr><td>備考2</td><td colspan='2'>（自動発注更新）</td><td>本数　11　　工数　6.5</td></tr>
<tr><td>工程備考</td><td colspan='4' class='notes'>{{ProductName}}</td></tr></table></div>

<div class='process'><div class='process-title'>組付工程</div>
<table><tr><td class='label-cell'>場所</td><td class='wide'>組付</td><td>製造課組付係組付</td><td>製番　2608001253-0001</td><td>着手日　2026/09/08</td></tr>
<tr><td class='label-cell'>品目区分</td><td>1</td><td class='label-cell'>品目コード</td><td class='large-text'>{{ProductCode}}</td><td class='label-cell'>数量　{{Quantity}}</td></tr>
<tr><td>備考1</td><td colspan='2'></td><td>納期　2026/09/14</td><td rowspan='2' class='barcode-cell'>発注SEQ　4187623 0<br><img class='barcode' src='data:image/png;base64,{{Barcode3}}'></td></tr>
<tr><td>備考2</td><td colspan='2'>（自動発注更新）</td><td>本数　11　　工数　18.44</td></tr></table></div>

<div class='process'><div class='process-title'>導通工程</div>
<table><tr><td class='label-cell'>場所</td><td class='wide'>導通検査</td><td>検査課導通検査</td><td>製番　2608001253-0001</td><td>着手日　2026/09/14</td></tr>
<tr><td class='label-cell'>品目区分</td><td>1</td><td class='label-cell'>品目コード</td><td class='large-text'>{{ProductCode}}</td><td class='label-cell'>数量　{{Quantity}}</td></tr>
<tr><td>備考1</td><td colspan='2'></td><td>納期　2026/09/15</td><td rowspan='2' class='barcode-cell'>発注SEQ　4187622 0<br><img class='barcode' src='data:image/png;base64,{{Barcode4}}'></td></tr>
<tr><td>備考2</td><td colspan='2'>（自動発注更新）</td><td>本数　11　　工数　5</td></tr></table></div>

<div class='process'><div class='process-title'>外観工程</div>
<table><tr><td class='label-cell'>場所</td><td class='wide'>外観検査</td><td>検査課外観検査</td><td>製番　2608001253-0001</td><td>着手日　2026/09/15</td></tr>
<tr><td class='label-cell'>品目区分</td><td>1</td><td class='label-cell'>品目コード</td><td class='large-text'>{{ProductCode}}</td><td class='label-cell'>数量　{{Quantity}}</td></tr>
<tr><td>備考1</td><td colspan='2'></td><td>納期　2026/09/17</td><td rowspan='2' class='barcode-cell'>発注SEQ　4187621 0<br><img class='barcode' src='data:image/png;base64,{{Barcode5}}'></td></tr>
<tr><td>備考2</td><td colspan='2'>（自動発注更新）</td><td>本数　11　　工数　5</td></tr></table></div>
</div></body></html>";

            currentHtml = currentHtml
                .Replace("{{ProductCode}}", HtmlEncode(productCode))
                .Replace("{{ProductName}}", HtmlEncode(productName))
                                .Replace("{{Quantity}}", productQuantity)
                                .Replace("{{Barcode1}}", CreateBarcodeBase64("0418762500"))
                                .Replace("{{Barcode2}}", CreateBarcodeBase64("0418762400"))
                                .Replace("{{Barcode3}}", CreateBarcodeBase64("0418762300"))
                                .Replace("{{Barcode4}}", CreateBarcodeBase64("0418762200"))
                                .Replace("{{Barcode5}}", CreateBarcodeBase64("0418762100"));

            preview.DocumentText = currentHtml;
        }

        private static string CreateBarcodeBase64(string value)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions { Width = 420, Height = 120, Margin = 2, PureBarcode = false }
            };

            using (var bitmap = writer.Write(value))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private static string HtmlEncode(string value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private void PrintLabel(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentHtml))
                RenderLabel();

            if (preview.Document == null)
            {
                MessageBox.Show("Mẫu tem chưa sẵn sàng.", "In tem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            preview.Document.ExecCommand("Print", false, null);
        }
    }
}
