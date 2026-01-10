from flask import Blueprint, request, jsonify
from engines.ppocr_v5 import PPOCRV5

ocr_bp = Blueprint('ocr', __name__)

@ocr_bp.route('/recognize', methods=['POST'])
def recognize_text():
    try:
        data = request.get_json()
        image_base64 = data.get('image')
        if not image_base64:
            return jsonify({'error': 'No image provided'}), 400
        
        # Assuming PPOCRV5 has a method to recognize
        result = PPOCRV5().recognize(image_base64)
        return jsonify(result)
    except Exception as e:
        return jsonify({'error': str(e)}), 500