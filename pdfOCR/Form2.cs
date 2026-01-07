using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenCvSharp;
using Tesseract;

namespace pdfOCR
{
    public partial class Form2 : Form
    {
        Bitmap _image;
        List<TextBoxResult> _boxes = new List<TextBoxResult>();
        private string tessDataPath = @"./tessdata";

        public Form2()
        {
            InitializeComponent();

            // Check if tessdata exists
            string tessDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            if (!Directory.Exists(tessDataDir) || !File.Exists(Path.Combine(tessDataDir, "eng.traineddata")))
            {
                MessageBox.Show("Tessdata folder or eng.traineddata not found!");
                return;
            }
        }

        // =========================
        // Open Image
        // =========================
        private void btnOpenImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image|*.png;*.jpg;*.jpeg;*.bmp";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _image?.Dispose();
                _image = LoadImage(ofd.FileName);
                pictureBox1.Image = _image;
                // Hiển thị full ảnh, giữ tỉ lệ
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                lblStatus.Text = "Image loaded";
            }
        }

        // =========================
        // Detect + Recognize toàn bộ chữ viết
        // =========================
        private void btnDetect_Click(object sender, EventArgs e)
        {
            if (_image == null) return;

            lblStatus.Text = "Detecting...";
            Application.DoEvents();

            _boxes.Clear();

            // First, detect all text with Tesseract
            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                engine.DefaultPageSegMode = PageSegMode.Auto;
                engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,-()");

                Bitmap processedImage = PreprocessImage(_image);

                using (var page = engine.Process(processedImage))
                {
                    using (var iter = page.GetIterator())
                    {
                        iter.Begin();
                        do
                        {
                            string text = iter.GetText(PageIteratorLevel.Word);
                            if (!string.IsNullOrEmpty(text.Trim()))
                            {
                                if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Tesseract.Rect rect))
                                {
                                    var box = new TextBoxResult
                                    {
                                        Box = new RectangleF(rect.X1, rect.Y1, rect.Width, rect.Height),
                                        TightBox = new RectangleF(rect.X1, rect.Y1, rect.Width, rect.Height),
                                        Text = text.Trim(),
                                        Score = iter.GetConfidence(PageIteratorLevel.Word) / 100f
                                    };
                                    _boxes.Add(box);
                                }
                            }
                        } while (iter.Next(PageIteratorLevel.Word));
                    }
                }
            }

            // Then, detect blue text regions and recognize with Tesseract
            using (Mat mat = BitmapToMat(_image))
            {
                Mat processedMat = PreprocessForColorDetection(mat);

                // Detect regions with blue color (wide range for bold and light)
                Mat hsv = new Mat();
                Cv2.CvtColor(processedMat, hsv, ColorConversionCodes.BGR2HSV);

                Scalar lowerBlue = new Scalar(70, 20, 20); // Wider range for light blue
                Scalar upperBlue = new Scalar(150, 255, 255); // Wider range for bold blue
                Mat mask = new Mat();
                Cv2.InRange(hsv, lowerBlue, upperBlue, mask);

                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
                Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);
                Cv2.Dilate(mask, mask, kernel);

                // Find contours
                OpenCvSharp.Point[][] contours;
                Cv2.FindContours(mask, out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                {
                    engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                    engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,-()");

                    foreach (var contour in contours)
                    {
                        OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                        if (rect.Width < 10 || rect.Height < 10) continue;

                        // Crop ROI
                        Mat roi = new Mat(processedMat, rect);
                        Bitmap roiBitmap = MatToBitmap(roi);

                        using (var page = engine.Process(roiBitmap))
                        {
                            string text = page.GetText().Trim();
                            if (!string.IsNullOrEmpty(text))
                            {
                                var box = new TextBoxResult
                                {
                                    Box = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                                    TightBox = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                                    Text = text,
                                    Score = page.GetMeanConfidence() / 100f
                                };
                                // Add if not already present (simple check by text)
                                if (!_boxes.Any(b => b.Text == text))
                                    _boxes.Add(box);
                            }
                        }
                    }
                }
            }

            // 2️⃣ Hiển thị box trên ảnh
            pictureBox1.Image = DrawBoxes(_image, _boxes, "");

            // 3️⃣ Đổ text vào ListBox1
            listBox1.Items.Clear();
            foreach (var box in _boxes)
            {
                listBox1.Items.Add(box.Text);
            }

            lblStatus.Text = $"Detected {_boxes.Count} text boxes";
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }
        // =========================
        // Search + Highlight keyword
        // =========================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (_image == null || _boxes == null || _boxes.Count == 0)
                return;

            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
                return;

            // 1️⃣ Lọc box match
            var matched = new List<TextBoxResult>();

            foreach (var box in _boxes)
            {
                string txt = (box.Text ?? "").Trim();
                string kw = keyword.Trim();
                bool isMatch = txt.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               kw.IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0; // linh hoạt hơn
                if (isMatch)
                    matched.Add(box);
            }

            // 2️⃣ Highlight ảnh - chỉ vẽ những box matched và highlight chúng
            pictureBox1.Image = DrawBoxes(_image, matched, keyword);

            // 3️⃣ Hiển thị ListBox
            listBox1.DataSource = null;
            listBox1.DisplayMember = "Text";
            listBox1.DataSource = matched;

            lblStatus.Text = $"Found {matched.Count} results for '{keyword}'";

            if (matched.Count == 0)
            {
                string allTexts = string.Join("\n", _boxes.Select(b => b.Text ?? ""));
                MessageBox.Show($"No matches found for '{keyword}'.\n\nAll detected texts:\n{allTexts}", "Search Result");
            }
        }
        Bitmap LoadImage(string path)
        {
            using (Bitmap raw = new Bitmap(path))
            {
                int max = 3500;
                float scale = Math.Min(1f, max / (float)Math.Max(raw.Width, raw.Height));
                int w = (int)(raw.Width * scale);
                int h = (int)(raw.Height * scale);

                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                    g.DrawImage(raw, 0, 0, w, h);

                return bmp;
            }
        }

        // =========================
        // Preprocess image for better OCR
        // =========================
        private Bitmap PreprocessImage(Bitmap original)
        {
            Bitmap grayscale = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(grayscale))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                    0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            return grayscale;
        }

        // =========================
        // Preprocess for color detection
        // =========================
        private Mat PreprocessForColorDetection(Mat img)
        {
            Mat processed = new Mat();
            Cv2.GaussianBlur(img, processed, new OpenCvSharp.Size(3, 3), 0);
            return processed;
        }

        // =========================
        // Draw boxes + highlight keyword
        // =========================
        public Bitmap DrawBoxes(Bitmap src, List<TextBoxResult> boxes, string keyword = "")
        {
            Bitmap bmp = new Bitmap(src);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (Brush bYellow = new SolidBrush(Color.FromArgb(80, Color.Yellow)))
                using (Brush bGreen = new SolidBrush(Color.FromArgb(80, Color.Lime)))
                using (Pen pRed = new Pen(Color.Red, 2))
                using (Pen pBlue = new Pen(Color.Blue, 2))
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                {
                    foreach (var box in boxes)
                    {
                        RectangleF r = box.TightBox.Width > 0 ? box.TightBox : box.Box;

                        bool highlight = !string.IsNullOrEmpty(keyword) &&
                                         box.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

                        // g.FillRectangle(highlight ? bGreen : bYellow, r);
                        g.DrawRectangle(highlight ? pBlue : pRed, r.X, r.Y, r.Width, r.Height);

                        g.DrawString(box.Text, font, Brushes.Black, r.X + 1, r.Y + 1);
                    }
                }
            }

            return bmp;
        }


        // =========================
        // Bitmap → Mat
        // =========================
        private Mat BitmapToMat(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bytes = ms.ToArray();
                Mat mat = Mat.FromImageData(bytes, ImreadModes.Color);
                return mat;
            }
        }
        // =========================
        // Mat → Bitmap (thuần, không BitmapConverter)
        // =========================
        private Bitmap MatToBitmap(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC3)
                mat = mat.CvtColor(ColorConversionCodes.BGR2RGB);

            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                 ImageLockMode.WriteOnly,
                                                 bitmap.PixelFormat);

            int bytesPerRow = mat.Width * mat.ElemSize(); // 3 bytes cho CV_8UC3
            byte[] rowData = new byte[bytesPerRow];

            for (int y = 0; y < mat.Height; y++)
            {
                Marshal.Copy(mat.Ptr(y), rowData, 0, bytesPerRow);
                Marshal.Copy(rowData, 0, bmpData.Scan0 + y * bmpData.Stride, bytesPerRow);
            }

            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    // =========================
    // Text box result
    // =========================
    public class TextBoxResult
    {
        public RectangleF Box { get; set; }
        public RectangleF TightBox { get; set; }
        public PointF[] Polygon { get; set; }
        public string Text { get; set; }
        public float Score { get; set; }

        public override string ToString()
        {
            return $"{Text}  ({Score:0.00})";
        }
    }
}
