import base64
import cv2
import numpy as np

def encode_image_to_base64(image):
    _, buffer = cv2.imencode('.png', image)
    return base64.b64encode(buffer).decode('utf-8')

def decode_base64_to_image(base64_string):
    image_data = base64.b64decode(base64_string)
    nparr = np.frombuffer(image_data, np.uint8)
    return cv2.imdecode(nparr, cv2.IMREAD_COLOR)