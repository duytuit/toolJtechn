import cv2
import numpy as np

def extract_colored_regions(img):
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    # ví dụ: màu hồng / tím
    lower = np.array([125, 40, 40])
    upper = np.array([165, 255, 255])

    mask = cv2.inRange(hsv, lower, upper)

    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5,5))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)

    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    regions = []
    for cnt in contours:
        x,y,w,h = cv2.boundingRect(cnt)
        if w > 40 and h > 40:   # lọc nhiễu nhỏ
            regions.append((x,y,w,h))

    return regions
