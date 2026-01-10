import base64
import cv2
import numpy as np
from paddleocr import PaddleOCR


class PPOCRV5:
    def __init__(self):
        self.ocr = PaddleOCR(
            ocr_version="PP-OCRv5",
            lang="en",              # đổi thành "japan" / "vi" nếu cần
            use_angle_cls=True,
            det_db_box_thresh=0.4,
            det_db_unclip_ratio=1.8,
            rec_batch_num=6,
            show_log=False
        )

    # -------------------------------------------------
    # Decode base64 -> OpenCV image
    # -------------------------------------------------
    def _base64_to_img(self, image_base64: str):
        try:
            image_data = base64.b64decode(image_base64)
            nparr = np.frombuffer(image_data, np.uint8)
            img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
            return img
        except Exception:
            return None

    # -------------------------------------------------
    # Parse OCR box (handle all PP-OCRv5 formats)
    # -------------------------------------------------
    def _parse_box(self, raw_box):
        box = []

        if not raw_box:
            return box

        # Case 1: [[x,y], [x,y], [x,y], [x,y]]
        if isinstance(raw_box[0], (list, tuple)):
            for pt in raw_box:
                if isinstance(pt, (list, tuple)) and len(pt) >= 2:
                    try:
                        box.append([float(pt[0]), float(pt[1])])
                    except Exception:
                        pass
            return box

        # Case 2: [x1,y1,x2,y2,x3,y3,x4,y4]
        if isinstance(raw_box[0], (int, float)):
            nums = list(raw_box)
            for i in range(0, len(nums) - 1, 2):
                try:
                    box.append([float(nums[i]), float(nums[i + 1])])
                except Exception:
                    pass
            return box

        return box

    # -------------------------------------------------
    # Parse 1 OCR line safely
    # -------------------------------------------------
    def _parse_line(self, line):
        # box
        raw_box = line[0]
        box = self._parse_box(raw_box)

        # text + score
        text = ""
        score = None
        rec = line[1]

        if isinstance(rec, (list, tuple)):
            if len(rec) > 0:
                text = str(rec[0])
            if len(rec) > 1:
                try:
                    score = float(rec[1])
                except Exception:
                    score = None
        else:
            text = str(rec)

        return {
            "text": text,
            "score": score,
            "box": box
        }

    # -------------------------------------------------
    # Public OCR API
    # -------------------------------------------------
    def recognize(self, image_base64: str):
        # 1️⃣ Decode image
        img = self._base64_to_img(image_base64)

        if img is None:
            return {
                "success": False,
                "error": "IMAGE_DECODE_FAILED"
            }

        # 2️⃣ Resize nếu ảnh quá nhỏ (tăng độ detect)
        h, w = img.shape[:2]
        if max(h, w) < 800:
            scale = 800 / max(h, w)
            img = cv2.resize(img, None, fx=scale, fy=scale)

        # 3️⃣ OCR
        result = self.ocr.ocr(img)

        # 4️⃣ Không có text
        if not result or not result[0]:
            return {
                "success": True,
                "count": 0,
                "texts": [],
                "message": "NO_TEXT_DETECTED"
            }

        # 5️⃣ Parse kết quả
        outputs = []
        for line in result[0]:
            outputs.append(self._parse_line(line))

        return {
            "success": True,
            "count": len(outputs),
            "texts": outputs
        }
