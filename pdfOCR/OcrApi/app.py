from flask import Flask
from routes.ocr_route import ocr_bp
from routes.structure_route import structure_bp

app = Flask(__name__)

app.register_blueprint(ocr_bp, url_prefix="/ocr")
app.register_blueprint(structure_bp, url_prefix="/structure")

@app.route('/')
def home():
    return {'message': 'OCR API is running!', 'endpoints': ['/ocr/recognize', '/structure/analyze']}

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=8000, debug=True)