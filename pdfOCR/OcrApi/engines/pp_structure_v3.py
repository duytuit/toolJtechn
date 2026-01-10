import base64
import cv2
import numpy as np
from paddleocr import PPStructureV3

class PPStructureAnalyzer:
    def __init__(self):
        self.engine = PPStructureV3(show_log=False)
    
    def analyze(self, image_base64):
        # Decode base64 to image
        image_data = base64.b64decode(image_base64)
        nparr = np.frombuffer(image_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        
        # Perform structure analysis
        result = self.engine(img)
        
        # Process result
        structures = []
        for item in result:
            structures.append({
                'type': item['type'],
                'bbox': item['bbox'],
                'text': item.get('text', '')
            })
        
        return {'structures': structures}
    
    def analyze(self, image_base64):
        # Decode base64 to image
        image_data = base64.b64decode(image_base64)
        nparr = np.frombuffer(image_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        
        # Perform structure analysis
        result = self.engine(img)
        
        # Process result
        structures = []
        for item in result:
            structures.append({
                'type': item['type'],
                'bbox': item['bbox'],
                'text': item.get('text', '')
            })
        
        return {'structures': structures}
    
    def analyze(self, image_base64):
        # Decode base64 to image
        image_data = base64.b64decode(image_base64)
        nparr = np.frombuffer(image_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        
        # Perform structure analysis
        result = self.engine(img)
        
        # Process result
        structures = []
        for item in result:
            structures.append({
                'type': item['type'],
                'bbox': item['bbox'],
                'text': item.get('text', '')
            })
        
        return {'structures': structures}