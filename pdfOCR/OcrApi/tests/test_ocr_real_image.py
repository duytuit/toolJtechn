import cv2
import numpy as np
import os
from pdf2image import convert_from_path


# -------------------------------
# 1. Convert PDF → IMAGE (BGR)
# -------------------------------
def pdf_to_image(pdf_path, dpi=300, page=0, poppler_path=None):
    if not os.path.exists(pdf_path):
        raise FileNotFoundError(f"Không tìm thấy PDF: {pdf_path}")

    images = convert_from_path(
        pdf_path,
        dpi=dpi,
        first_page=page + 1,
        last_page=page + 1,
        poppler_path=poppler_path
    )

    if not images:
        raise ValueError("Không đọc được PDF")

    img_rgb = np.array(images[0])
    img_bgr = cv2.cvtColor(img_rgb, cv2.COLOR_RGB2BGR)
    return img_bgr


# -------------------------------------------------
# 2. Tạo PNG nếu chưa có (fallback cho test)
# -------------------------------------------------
def create_png_if_not_exists(image_path, width=1200, height=1600):
    os.makedirs(os.path.dirname(image_path), exist_ok=True)

    if not os.path.exists(image_path):
        print("⚠ PNG chưa tồn tại, đang tạo mới:", image_path)

        img = np.full((height, width, 3), 255, dtype=np.uint8)
        cv2.putText(
            img,
            "AUTO CREATED IMAGE 0000B8",
            (50, height // 2),
            cv2.FONT_HERSHEY_SIMPLEX,
            2,
            (184, 0, 0),  # BGR = #0000B8
            4
        )
        cv2.imwrite(image_path, img)


# -------------------------------------------------
# 3. Giữ & tăng đậm màu #0000B8
# -------------------------------------------------
def keep_only_and_boost_0000B8(img):
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    lower_blue = np.array([110, 80, 40])
    upper_blue = np.array([130, 255, 255])

    mask = cv2.inRange(hsv, lower_blue, upper_blue)

    kernel = np.ones((3, 3), np.uint8)
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel, iterations=2)

    h, s, v = cv2.split(hsv)

    s = np.where(mask > 0, np.clip(s * 1.6, 0, 255), s)
    v = np.where(mask > 0, np.clip(v * 1.3, 0, 255), v)

    boosted_hsv = cv2.merge([h, s.astype(np.uint8), v.astype(np.uint8)])
    boosted_img = cv2.cvtColor(boosted_hsv, cv2.COLOR_HSV2BGR)

    result = cv2.bitwise_and(boosted_img, boosted_img, mask=mask)
    return result


# -------------------------------------------------
# 4. MAIN PIPELINE
# -------------------------------------------------
def pdf_to_boosted_png(
    pdf_path,
    output_png,
    dpi=300,
    page=0,
    poppler_path=None
):
    # Nếu PNG chưa có → tạo từ PDF
    if not os.path.exists(output_png):
        print("📄 Convert PDF → PNG")

        img = pdf_to_image(
            pdf_path,
            dpi=dpi,
            page=page,
            poppler_path=poppler_path
        )
    else:
        print("🖼 PNG đã tồn tại, đọc lại")
        img = cv2.imread(output_png)

    if img is None:
        print("⚠ PNG lỗi → tạo fallback")
        create_png_if_not_exists(output_png)
        img = cv2.imread(output_png)

    # Boost màu xanh
    result = keep_only_and_boost_0000B8(img)

    os.makedirs(os.path.dirname(output_png), exist_ok=True)
    cv2.imwrite(output_png, result)

    print("✅ Hoàn tất, đã lưu:", output_png)


# -------------------------------------------------
# 5. RUN
# -------------------------------------------------
if __name__ == "__main__":
    pdf_to_boosted_png(
        pdf_path="tests/images/0684200285-07-v7.pdf",
        output_png="tests/images/0684200285-07-v7.png",
        dpi=300,
        page=0,
        # poppler_path=r"C:\poppler\Library\bin"  # bật nếu chưa add PATH
    )
