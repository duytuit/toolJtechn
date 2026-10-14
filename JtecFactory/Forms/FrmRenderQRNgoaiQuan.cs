using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JtecFactory.Helpers;
using JtecFactory.Navigation;
using QRCoder;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    public sealed partial class FrmRenderQRNgoaiQuan : Form
    {
        private readonly IPermissionService permissionService;
        private readonly int menuId;
        private readonly Action backAction;
        private OracleHelper oracle;
        private FlowLayoutPanel qrResults;

        public FrmRenderQRNgoaiQuan()
        {
            InitializeComponent();
            InitializeQrWorkflow();
        }

        internal FrmRenderQRNgoaiQuan(MenuItem menu, IPermissionService permissionService, Action backAction)
        {
            if (menu == null)
                throw new ArgumentNullException("menu");
            if (permissionService != null && !permissionService.CanView(menu.Id))
                throw new UnauthorizedAccessException("The current role cannot view this function.");

            this.permissionService = permissionService;
            this.menuId = menu.Id;
            this.backAction = backAction;

            InitializeComponent();
            titleLabel.Text = menu.Name;
            subtitleLabel.Text = menu.Description;
            InitializeQrWorkflow();
            ApplyPermissions();
        }

        private void InitializeQrWorkflow()
        {
            var settings = ConfigurationManager.ConnectionStrings["OracleNgoaiQuan"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("Chưa cấu hình Oracle connection 'OracleNgoaiQuan'.");

            oracle = new OracleHelper(settings.ConnectionString);
            backButton.Click += BackButtonClick;
            titleLabel.Text = "Render QR Ngoại Quan";
            subtitleLabel.Text = "Quét mã 発注SEQ để tạo QR theo thứ tự sản xuất.";
            searchTextBox.Text = string.Empty;
            addProductButton.Text = "Tạo QR";
            addProductButton.BackColor = Color.FromArgb(37, 99, 235);
            addProductButton.Click += RenderQrForOrder;
            searchTextBox.KeyDown += SearchTextBoxKeyDown;

            pageLayout.Controls.Remove(productGrid);
            productGrid.Dispose();
            qrResults = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(12),
                WrapContents = true
            };
            pageLayout.Controls.Add(qrResults, 0, 2);
        }

        private void ApplyPermissions()
        {
            if (permissionService != null)
                addProductButton.Visible = permissionService.CanAdd(menuId);
        }

        private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            RenderQrForOrder(sender, EventArgs.Empty);
        }

        private async void RenderQrForOrder(object sender, EventArgs e)
        {
            string scannedSequence;
            if (!TryParseScannedSequence(searchTextBox.Text, out scannedSequence))
            {
                MessageBox.Show(this, "Định dạng scan không hợp lệ. Hãy quét '0発注SEQ 00'.",
                    "Mã scan không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                searchTextBox.Focus();
                return;
            }

            addProductButton.Enabled = false;
            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                DataTable rows = await Task.Run(() => LoadOrderRows(scannedSequence));
                var cards = await Task.Run(() => CreateQrCards(rows));
                ClearQrResults();
                foreach (QrCard card in cards)
                    qrResults.Controls.Add(CreateQrCardControl(card));

                if (cards.Count == 0)
                {
                    MessageBox.Show(this, "Không tìm thấy dữ liệu K00 cho mã " + scannedSequence + ".",
                        "Không có dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Không thể tạo QR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
                addProductButton.Enabled = true;
                searchTextBox.Focus();
                searchTextBox.SelectAll();
            }
        }

        private static bool TryParseScannedSequence(string input, out string sequence)
        {
            sequence = null;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string[] parts = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 && IsAsciiDigits(parts[0]))
            {
                sequence = parts[0];
                return true;
            }

            if (parts.Length == 2 && parts[0].Length > 1 && parts[0][0] == '0' &&
                parts[1] == "00" && IsAsciiDigits(parts[0].Substring(1)))
            {
                sequence = parts[0].Substring(1);
                return true;
            }

            if (parts.Length == 3 && parts[0] == "0" && parts[2] == "00" && IsAsciiDigits(parts[1]))
            {
                sequence = parts[1];
                return true;
            }

            return false;
        }

        private static bool IsAsciiDigits(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                value.All(character => character >= '0' && character <= '9');
        }

        private DataTable LoadOrderRows(string scannedSequence)
        {
            string sql = @"
SELECT H10F.工程C AS PROCESS_CODE,
       H10F.業者C AS VENDOR_CODE,
       H10F.製番 AS PRODUCT_NO,
       H10F.品目C AS ITEM_CODE,
       H10F.発注SEQ AS ORDER_SEQ,
       H10F.発注数 AS ORDER_QTY,
       H10F.備考1 AS H10F_REMARK_1,
       H10F.備考2 AS H10F_REMARK_2,
       H10F.品目K AS ITEM_KIND,
       H10F.納期 AS DUE_DATE,
       H10F.発注日 AS ORDER_DATE,
       H10F.注文書発行日 AS ORDER_ISSUED_DATE,
       M40M.品目C AS M40_ITEM_CODE,
       M40M.品目K AS M40_ITEM_KIND,
       M40M.本数 AS PIECE_COUNT,
       M46M.*
FROM DFW_H10F H10F
LEFT JOIN DFW_M40M M40M
       ON H10F.品目K = M40M.品目K
      AND H10F.品目C = M40M.品目C
LEFT JOIN TAD_M46M M46M
       ON H10F.品目K = M46M.品目K
      AND H10F.品目C = M46M.品目C
WHERE EXISTS (
    SELECT 1
    FROM DFW_H10F X
    WHERE X.製番 = H10F.製番
      AND X.品目C = H10F.品目C
      AND H10F.工程C = 'K00'
      AND X.発注SEQ IN ('" + scannedSequence + @"')
)
ORDER BY H10F.製番";

            return oracle.ExecuteDataTable(sql);
        }

        private static System.Collections.Generic.List<QrCard> CreateQrCards(DataTable rows)
        {
            var cards = new System.Collections.Generic.List<QrCard>();
            var processedOrders = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            foreach (DataRow row in rows.Rows)
            {
                string sequence = Convert.ToString(row["ORDER_SEQ"], CultureInfo.InvariantCulture).Trim();
                string itemCode = Convert.ToString(row["ITEM_CODE"], CultureInfo.InvariantCulture).Trim();
                if (string.IsNullOrWhiteSpace(sequence) || string.IsNullOrWhiteSpace(itemCode))
                    continue;

                string productNo = Convert.ToString(row["PRODUCT_NO"], CultureInfo.InvariantCulture).Trim();
                string orderKey = sequence + "\u001f" + itemCode + "\u001f" + productNo;
                if (!processedOrders.Add(orderKey))
                    continue;

                decimal quantityValue = row["ORDER_QTY"] == null || row["ORDER_QTY"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(row["ORDER_QTY"], CultureInfo.InvariantCulture);
                if (quantityValue <= 0)
                    continue;
                if (quantityValue != decimal.Truncate(quantityValue))
                    throw new InvalidOperationException("発注数 không phải số nguyên cho 発注SEQ " + sequence + ".");

                int quantityCount = decimal.ToInt32(quantityValue);
                using (var generator = new QRCodeGenerator())
                {
                    for (int unitNumber = 1; unitNumber <= quantityCount; unitNumber++)
                    {
                        string payload = "H0" + sequence + " 00 " + FormatQuantity(unitNumber) +
                            ",1," + itemCode + ",,,,,";
                        using (QRCodeData data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
                        using (var qrCode = new QRCode(data))
                        {
                            cards.Add(new QrCard
                            {
                                Sequence = sequence,
                                ItemCode = itemCode,
                                Payload = payload,
                                UnitNumber = unitNumber,
                                UnitCount = quantityCount,
                                Image = qrCode.GetGraphic(6)
                            });
                        }
                    }
                }
            }

            return cards;
        }

        private static string FormatQuantity(decimal quantity)
        {
            return quantity.ToString("0000", CultureInfo.InvariantCulture);
        }

        private static Control CreateQrCardControl(QrCard card)
        {
            var panel = new Panel
            {
                Width = 180,
                Height = 218,
                Margin = new Padding(8, 2, 8, 0),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            var image = new PictureBox
            {
                Image = card.Image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(10, 8),
                Size = new Size(158, 158)
            };
            var itemLabel = new Label
            {
                Text = card.ItemCode,
                Location = new Point(4, 170),
                Size = new Size(170, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            var sequenceLabel = new Label
            {
                Text = "発注 " + card.Sequence + " (" + card.UnitNumber + "/" + card.UnitCount + ")",
                Location = new Point(4, 191),
                Size = new Size(170, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            // var payloadLabel = new Label
            // {
            //     Text = card.Payload,
            //     Location = new Point(8, 217),
            //     Size = new Size(172, 28),
            //     TextAlign = ContentAlignment.MiddleCenter,
            //     AutoEllipsis = true,
            //     ForeColor = Color.DimGray,
            //     Font = new Font("Segoe UI", 7.5F)
            // };
            panel.Controls.Add(image);
            panel.Controls.Add(itemLabel);
            panel.Controls.Add(sequenceLabel);
            // panel.Controls.Add(payloadLabel);
            return panel;
        }

        private void ClearQrResults()
        {
            var disposedImages = new System.Collections.Generic.HashSet<Image>();
            foreach (Control control in qrResults.Controls)
            {
                var picture = control.Controls.OfType<PictureBox>().FirstOrDefault();
                if (picture != null)
                {
                    Image image = picture.Image;
                    picture.Image = null;
                    if (image != null && disposedImages.Add(image))
                        image.Dispose();
                }
                control.Dispose();
            }
            qrResults.Controls.Clear();
        }

        private sealed class QrCard
        {
            public string Sequence { get; set; }
            public string ItemCode { get; set; }
            public string Payload { get; set; }
            public int UnitNumber { get; set; }
            public int UnitCount { get; set; }
            public Bitmap Image { get; set; }
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            if (backAction != null)
                backAction();
            else
                Close();
        }
    }
}