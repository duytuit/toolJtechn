from paddleocr import PaddleOCR
import cv2
import json
import os

os.environ["GLOG_minloglevel"] = "2"

ocr = PaddleOCR(
    lang='en',
    use_textline_orientation=True
)

img = cv2.imread("tests/images/test_ocr.png")
result = ocr.ocr(img)

output = []

for line in result[0]:
    box = line[0]
    data = line[1]

    if isinstance(data, dict):
        text = data.get("text", "")
        score = float(data.get("confidence", 1.0))
    else:
        text = str(data)
        score = 1.0

    output.append({
        "text": text,
        "confidence": score,
        "box": box
    })

print(json.dumps(output, indent=2, ensure_ascii=False))
