import cv2
import numpy as np
from paddleocr import PaddleOCR


def extract_and_bold_blue(img):
    """
    Giữ màu #0000B8 và tô đậm chữ
    """
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    # Khoảng xanh dương đậm gần #0000B8
    lower_blue = np.array([110, 80, 40])
    upper_blue = np.array([130, 255, 255])

    mask = cv2.inRange(hsv, lower_blue, upper_blue)

    # 🔥 TÔ ĐẬM NÉT CHỮ
    kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3))
    mask = cv2.dilate(mask, kernel, iterations=2)

    # Chỉ giữ chữ xanh
    blue_only = cv2.bitwise_and(img, img, mask=mask)

    return blue_only, mask


def ocr_bold_blue_text(image_path):
    ocr = PaddleOCR(
        lang="en",
        use_angle_cls=True
    )

    img = cv2.imread(image_path)
    if img is None:
        raise ValueError("Không đọc được ảnh")

    # 1️⃣ Giữ + tô đậm chữ xanh
    blue_img, mask = extract_and_bold_blue(img)

    # 2️⃣ Chuyển sang grayscale
    gray = cv2.cvtColor(blue_img, cv2.COLOR_BGR2GRAY)

    # 3️⃣ Threshold → chữ trắng, nền đen
    _, binary = cv2.threshold(
        gray, 0, 255,
        cv2.THRESH_BINARY + cv2.THRESH_OTSU
    )

    # PaddleOCR cần BGR
    ocr_input = cv2.cvtColor(binary, cv2.COLOR_GRAY2BGR)

    # 4️⃣ OCR (DÙNG predict)
    result = ocr.predict(ocr_input)

    print("\n========== OCR RESULT ==========")

    count = 0
    if result:
        for line in result:
            text = line.get("text", "").strip()
            score = line.get("score", 0)

            if not text:
                continue

            count += 1
            print(f"{count:03d} | {text} | score={score:.2f}")

    print("================================")
    print("TOTAL TEXT FOUND:", count)

    # (tuỳ chọn) lưu ảnh đã tô đậm để kiểm tra
    cv2.imwrite("debug_bold_blue.png", binary)


if __name__ == "__main__":
    ocr_bold_blue_text("tests/images/only_0000B8.png")
