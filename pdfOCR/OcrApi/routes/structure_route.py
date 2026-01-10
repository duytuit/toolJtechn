from flask import Blueprint, request, jsonify
from engines.pp_structure_v3 import PPStructureAnalyzer

structure_bp = Blueprint('structure', __name__)

@structure_bp.route('/analyze', methods=['POST'])
def analyze_structure():
    try:
        data = request.get_json()
        image_base64 = data.get('image')
        if not image_base64:
            return jsonify({'error': 'No image provided'}), 400
        
        # Assuming PPStructureAnalyzer has a method to analyze
        result = PPStructureAnalyzer().analyze(image_base64)
        return jsonify(result)
    except Exception as e:
        return jsonify({'error': str(e)}), 500