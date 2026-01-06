using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace pdfOCR
{
    public partial class Form2 : Form
    {
        Bitmap _image;
        List<TextBoxResult> _boxes = new List<TextBoxResult>();
        PaddleOcr _ocr;

        public Form2()
        {
            InitializeComponent();

            // Paths relative
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string detModel = Path.Combine(baseDir, "Model", "ch_PP-OCRv3_det_infer.onnx");
            string recModel = Path.Combine(baseDir, "Model", "en_PP-OCRv3_rec_infer.onnx");
            string keyFile = Path.Combine(baseDir, "Model", "ppocr_keys_v1.txt");

            if (!File.Exists(detModel) || !File.Exists(recModel) || !File.Exists(keyFile))
            {
                MessageBox.Show("Models or keys file not found in 'Model' folder!");
                return;
            }

            _ocr = new PaddleOcr(detModel, recModel, keyFile);
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
        // Detect + Recognize
        // =========================
        private void btnDetect_Click(object sender, EventArgs e)
        {
            if (_image == null) return;

            lblStatus.Text = "Detecting...";
            Application.DoEvents();

            using (Mat mat = BitmapToMat(_image))
            {
                // 1️⃣ Detect & recognize
                _boxes = _ocr.DetectAndRecognize(mat);
            }

            // 2️⃣ Hiển thị box trên ảnh
            pictureBox1.Image = DrawBoxes(_image, _boxes);

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
            if (_image == null || _boxes.Count == 0) return;

            string keyword = txtSearch.Text.Trim();
            if (keyword.Length == 0) return;

            pictureBox1.Image = DrawBoxes(_image, _boxes, keyword);
            lblStatus.Text = $"Highlight keyword '{keyword}'";
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
        // Draw boxes + highlight keyword
        // =========================
        public Bitmap DrawBoxes(Bitmap src, List<TextBoxResult> boxes, string keyword = "")
        {
            if (src == null) return null;
            Bitmap bmp = new Bitmap(src);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Brush bYellow = new SolidBrush(Color.FromArgb(80, Color.Yellow)))
                using (Brush bGreen = new SolidBrush(Color.FromArgb(80, Color.Lime)))
                using (Pen pRed = new Pen(Color.Red, 2))
                using (Pen pBlue = new Pen(Color.Blue, 2))
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                {
                    foreach (var box in boxes)
                    {
                        string txt = box.Text ?? "";

                        // Nếu có polygon, highlight polygon sát chữ
                        if (box.Polygon != null && box.Polygon.Length > 0)
                        {
                            PointF[] pts = box.Polygon;

                            // Highlight keyword toàn bộ box polygon nếu text chứa keyword
                            bool highlightKeyword = !string.IsNullOrEmpty(keyword) &&
                                                    txt.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

                            g.FillPolygon(highlightKeyword ? bGreen : bYellow, pts);
                            g.DrawPolygon(highlightKeyword ? pBlue : pRed, pts);

                            // Vẽ text trên polygon (lấy min X, min Y)
                            float px = pts.Min(p => p.X);
                            float py = pts.Min(p => p.Y) - 15;
                            if (py < 0) py = 0;
                            g.DrawString(txt, font, Brushes.Black, px, py);
                        }
                        else if (!string.IsNullOrEmpty(txt))
                        {
                            // Chia box theo số ký tự
                            int n = txt.Length;
                            if (n <= 0) continue;

                            float charWidth = box.Box.Width / n;

                            for (int i = 0; i < n; i++)
                            {
                                RectangleF charRect = new RectangleF(
                                    box.Box.X + i * charWidth,
                                    box.Box.Y,
                                    charWidth,
                                    box.Box.Height
                                );

                                bool charMatch = !string.IsNullOrEmpty(keyword) &&
                  keyword.ToUpper().IndexOf(char.ToUpper(txt[i])) >= 0;

                                // Fill màu
                                g.FillRectangle(charMatch ? bGreen : bYellow, charRect);
                                g.DrawRectangle(charMatch ? pBlue : pRed, charRect.X, charRect.Y, charRect.Width, charRect.Height);

                                // Vẽ ký tự
                                g.DrawString(txt[i].ToString(), font, Brushes.Black, charRect.X, charRect.Y - 15);
                            }
                        }
                        else
                        {
                            // Box trống → chỉ vẽ rectangle
                            g.DrawRectangle(pRed, box.Box.X, box.Box.Y, box.Box.Width, box.Box.Height);
                        }
                    }
                }
            }

            return bmp;
        }

        // =========================
        // Safe load image
        // =========================


        // =========================
        // Bitmap → Mat
        // =========================
        private Mat BitmapToMat(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
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
        public RectangleF Box { get; set; }        // Hình chữ nhật bao chữ
        public PointF[] Polygon { get; set; }      // Polygon contour chính xác
        public string Text { get; set; }           // Kết quả OCR
        public float Score { get; set; }           // Confidence (tạm 0.9)
    }

    // =========================
    // PaddleOCR wrapper
    // =========================
    public class PaddleOcr
    {
        private InferenceSession _detSession;
        private InferenceSession _recSession;
        private string[] _keys;

        public PaddleOcr(string detModel, string recModel, string keyFile)
        {
            _detSession = new InferenceSession(detModel);
            _recSession = new InferenceSession(recModel);
            _keys = File.ReadAllLines(keyFile); // đảm bảo chứa tất cả ký tự muốn nhận
        }

        public List<TextBoxResult> DetectAndRecognize(Mat img)
        {
            List<TextBoxResult> results = new List<TextBoxResult>();

            // =========================
            // 1️⃣ Convert to grayscale + adaptive threshold
            // =========================
            Mat gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

            Mat bin = new Mat();
            Cv2.AdaptiveThreshold(
                gray, bin,
                maxValue: 255,
                adaptiveMethod: AdaptiveThresholdTypes.MeanC,
                thresholdType: ThresholdTypes.BinaryInv,
                blockSize: 15,
                c: 5
            );

            // =========================
            // 2️⃣ Find contours
            // =========================
            OpenCvSharp.Point[][] contours;
            Cv2.FindContours(bin, out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            float scaleX = 1f; // vì adaptive threshold dùng trực tiếp ảnh gốc
            float scaleY = 1f;

            foreach (var contour in contours)
            {
                // Polygon approximation
                var approx = Cv2.ApproxPolyDP(contour, epsilon: 1.0, closed: true);
                var rect = Cv2.BoundingRect(approx);

                // Bỏ các box quá nhỏ
                if (rect.Width < 5 || rect.Height < 5) continue;

                // =========================
                // 3️⃣ Crop ROI với padding nhỏ
                // =========================
                int pad = 2;
                int x = Math.Max(rect.X - pad, 0);
                int y = Math.Max(rect.Y - pad, 0);
                int w = Math.Min(rect.Width + 2 * pad, img.Width - x);
                int h = Math.Min(rect.Height + 2 * pad, img.Height - y);
                var roiRect = new OpenCvSharp.Rect(x, y, w, h);
                Mat roi = new Mat(img, roiRect);

                // =========================
                // 4️⃣ Resize CRNN input H=32
                // =========================
                Mat crnnImg = new Mat();
                Cv2.Resize(roi, crnnImg, new OpenCvSharp.Size(320, 32));
                crnnImg.ConvertTo(crnnImg, MatType.CV_32FC3, 1.0 / 255);

                float[] crnnCHW = new float[1 * 3 * 32 * 320];
                unsafe
                {
                    int idx = 0;
                    for (int c = 0; c < 3; c++)
                        for (int y2 = 0; y2 < 32; y2++)
                            for (int x2 = 0; x2 < 320; x2++)
                                crnnCHW[idx++] = crnnImg.At<Vec3f>(y2, x2)[c];
                }

                var recTensor = new DenseTensor<float>(crnnCHW, new int[] { 1, 3, 32, 320 });
                var recInputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("x", recTensor) };
                var recOutputs = _recSession.Run(recInputs);

                // =========================
                // 5️⃣ Decode CRNN
                // =========================
                var output = recOutputs.First().AsEnumerable<float>().ToArray();
                int timeSteps = output.Length / _keys.Length;
                string text = "";
                int lastIdx = -1;
                for (int t = 0; t < timeSteps; t++)
                {
                    float maxVal = float.MinValue;
                    int maxIdx = 0;
                    for (int k = 0; k < _keys.Length; k++)
                    {
                        float v = output[t * _keys.Length + k];
                        if (v > maxVal)
                        {
                            maxVal = v;
                            maxIdx = k;
                        }
                    }

                    if (maxIdx != 0 && maxIdx != lastIdx)
                        text += _keys[maxIdx];

                    lastIdx = maxIdx;
                }

                // =========================
                // 6️⃣ Add result với polygon
                // =========================
                results.Add(new TextBoxResult
                {
                    Box = new RectangleF(rect.X * scaleX, rect.Y * scaleY, rect.Width * scaleX, rect.Height * scaleY),
                    Text = text,
                    Score = 0.9f
                });
            }

            return results;
        }
    }
}
