using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using PdfiumViewer;
using Tesseract;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Layout.Properties;

namespace pdfOCR
{
    public partial class Form1 : Form
    {
        private PdfiumViewer.PdfDocument _pdfDoc;
        private Bitmap _currentImage;
        private Dictionary<int, List<OCRWord>> _ocrResults = new Dictionary<int, List<OCRWord>>();
        private const int OCR_DPI = 600;
        private string tessDataPath = @"./tessdata";
        private string _searchTerm = "";

        // Controls
        private PdfiumViewer.PdfRenderer pdfRenderer;
        private TextBox txtSearch;
        private Button btnSelectPdf, btnOCR, btnPrevPage, btnNextPage, btnSavePDF;
        private ListBox lstResults;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomControls();
        }

        private void InitializeCustomControls()
        {
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Vertical,
                SplitterDistance = 650
            };
            this.Controls.Add(split);

            pdfRenderer = new PdfiumViewer.PdfRenderer { Dock = DockStyle.Fill };
            pdfRenderer.Paint += PdfRenderer_Paint;
            split.Panel1.Controls.Add(pdfRenderer);

            lstResults = new ListBox { Dock = DockStyle.Fill };
            lstResults.Click += LstResults_Click;
            split.Panel2.Controls.Add(lstResults);

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            this.Controls.Add(topPanel);

            txtSearch = new TextBox { Left = 10, Top = 8, Width = 200 };
            topPanel.Controls.Add(txtSearch);

            btnSelectPdf = new Button { Left = 220, Top = 6, Width = 80, Text = "Select PDF" };
            btnSelectPdf.Click += BtnSelectPdf_Click;
            topPanel.Controls.Add(btnSelectPdf);

            btnOCR = new Button { Left = 310, Top = 6, Width = 120, Text = "OCR & Search" };
            btnOCR.Click += BtnOCR_Click;
            topPanel.Controls.Add(btnOCR);

            btnPrevPage = new Button { Left = 440, Top = 6, Width = 60, Text = "<" };
            btnPrevPage.Click += BtnPrevPage_Click;
            topPanel.Controls.Add(btnPrevPage);

            btnNextPage = new Button { Left = 510, Top = 6, Width = 60, Text = ">" };
            btnNextPage.Click += BtnNextPage_Click;
            topPanel.Controls.Add(btnNextPage);

            btnSavePDF = new Button { Left = 580, Top = 6, Width = 160, Text = "Save Highlighted PDF" };
            btnSavePDF.Click += BtnSavePDF_Click;
            topPanel.Controls.Add(btnSavePDF);
        }

        private void BtnSelectPdf_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "PDF Files|*.pdf" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            _pdfDoc?.Dispose();
            _currentImage?.Dispose();
            _ocrResults.Clear();
            lstResults.Items.Clear();

            _pdfDoc = PdfiumViewer.PdfDocument.Load(ofd.FileName);
            pdfRenderer.Load(_pdfDoc);
        }

        private void BtnOCR_Click(object sender, EventArgs e)
        {
            if (_pdfDoc == null) { MessageBox.Show("Chọn PDF trước!"); return; }

            _searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(_searchTerm))
            {
                MessageBox.Show("Nhập từ/cụm từ để tìm kiếm!");
                return;
            }

            _ocrResults.Clear();
            lstResults.Items.Clear();
            Cursor = Cursors.WaitCursor;

            using (var engine = new TesseractEngine(tessDataPath, "vie+eng", EngineMode.Default))
            {
                engine.DefaultPageSegMode = PageSegMode.SparseText;
                engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./()-");

                for (int pageIndex = 0; pageIndex < _pdfDoc.PageCount; pageIndex++)
                {
                    _currentImage?.Dispose();
                    Bitmap bmp = new Bitmap(_pdfDoc.Render(pageIndex, OCR_DPI, OCR_DPI, true));
                    // Bỏ preprocess, dùng ảnh gốc
                    _currentImage = bmp;

                    List<OCRWord> pageWords = new List<OCRWord>();

                    using (var page = engine.Process(_currentImage))
                    {
                        using (var iter = page.GetIterator())
                        {
                            iter.Begin();
                            do
                            {
                                if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Tesseract.Rect rect))
                                {
                                    string text = iter.GetText(PageIteratorLevel.Word);
                                    if (string.IsNullOrEmpty(text)) continue;

                                    pageWords.Add(new OCRWord
                                    {
                                        Text = text,
                                        BoundingBox = rect
                                    });

                                    // Add tất cả words để debug
                                    lstResults.Items.Add($"Page {pageIndex + 1}: {text}");

                                    // Nếu khớp, đánh dấu
                                    if (text.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        // Có thể highlight khác, nhưng tạm add lại
                                    }
                                }
                            } while (iter.Next(PageIteratorLevel.Word));
                        }
                    }

                    _ocrResults[pageIndex] = pageWords;
                }
            }

            Cursor = Cursors.Default;
            MessageBox.Show($"OCR xong {_pdfDoc.PageCount} trang. Tổng {lstResults.Items.Count} từ OCR được.");
        }

        private Bitmap PreprocessImage(Bitmap bmp)
        {
            Bitmap gray = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(gray))
            {
                var cm = new ColorMatrix(new float[][]
                {
                    new float[]{0.299f,0.299f,0.299f,0,0},
                    new float[]{0.587f,0.587f,0.587f,0,0},
                    new float[]{0.114f,0.114f,0.114f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1}
                });
                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
                    0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
            }

            // Bỏ threshold, chỉ grayscale
            return gray;
        }

        private void BtnSavePDF_Click(object sender, EventArgs e)
        {
            if (_pdfDoc == null) return;

            SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            using (var writer = new PdfWriter(sfd.FileName))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                for (int pageIndex = 0; pageIndex < _pdfDoc.PageCount; pageIndex++)
                {
                    Bitmap bmp = new Bitmap(_pdfDoc.Render(pageIndex, OCR_DPI, OCR_DPI, true));
                    // Dùng ảnh gốc, không preprocess

                    // Vẽ highlight trên bitmap nếu có từ khớp
                    if (_ocrResults.ContainsKey(pageIndex))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            using (Brush highlightBrush = new SolidBrush(System.Drawing.Color.FromArgb(128, System.Drawing.Color.Yellow)))
                            {
                                foreach (var word in _ocrResults[pageIndex])
                                {
                                    if (word.Text.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        RectangleF rect = new RectangleF(word.BoundingBox.X1, word.BoundingBox.Y1, word.BoundingBox.Width, word.BoundingBox.Height);
                                        g.FillRectangle(highlightBrush, rect);
                                    }
                                }
                            }
                        }
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;
                        var imageData = ImageDataFactory.Create(ms.ToArray());
                        var pdfImage = new iText.Layout.Element.Image(imageData);
                        pdfImage.SetAutoScale(true);
                        doc.Add(pdfImage);
                    }

                    if (pageIndex < _pdfDoc.PageCount - 1)
                        doc.Add(new AreaBreak());
                }
            }

            MessageBox.Show("Đã lưu PDF với highlight!");
        }

        private void LstResults_Click(object sender, EventArgs e)
        {
            if (lstResults.SelectedItem == null) return;
            string line = lstResults.SelectedItem.ToString();
            int pageIndex = int.Parse(line.Substring(5, line.IndexOf(':') - 5)) - 1;
            pdfRenderer.Page = pageIndex;
            pdfRenderer.Invalidate();
        }

        private void PdfRenderer_Paint(object sender, PaintEventArgs e)
        {
            if (_pdfDoc == null || _ocrResults.Count == 0 || _currentImage == null) return;

            int pageIndex = pdfRenderer.Page;
            if (!_ocrResults.ContainsKey(pageIndex)) return;

            var words = _ocrResults[pageIndex];
            if (words.Count == 0) return;

            var pageSize = _pdfDoc.PageSizes[pageIndex];
            float scaleX = (pdfRenderer.Width / pageSize.Width) * (72f / OCR_DPI);
            float scaleY = (pdfRenderer.Height / pageSize.Height) * (72f / OCR_DPI);

            using (Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Yellow)))
            using (Pen pen = new Pen(System.Drawing.Color.Red, 2))
            {
                foreach (var w in words)
                {
                    // Chỉ highlight từ khớp
                    if (string.IsNullOrEmpty(_searchTerm) ||
                        w.Text.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    RectangleF r = new RectangleF(
                        w.BoundingBox.X1 * scaleX,
                        (_currentImage.Height - w.BoundingBox.Y2) * scaleY,
                        w.BoundingBox.Width * scaleX,
                        w.BoundingBox.Height * scaleY
                    );
                    e.Graphics.FillRectangle(brush, r);
                    e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                }
            }
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_pdfDoc == null || pdfRenderer.Page <= 0) return;
            pdfRenderer.Page--;
            pdfRenderer.Invalidate();
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (_pdfDoc == null || pdfRenderer.Page >= _pdfDoc.PageCount - 1) return;
            pdfRenderer.Page++;
            pdfRenderer.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _pdfDoc?.Dispose();
            _currentImage?.Dispose();
        }
    }

    public class OCRWord
    {
        public string Text { get; set; }
        public Tesseract.Rect BoundingBox { get; set; }
    }
}
