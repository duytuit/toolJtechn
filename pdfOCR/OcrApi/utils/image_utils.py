import cv2
import numpy as np

def load_image_from_path(path):
    return cv2.imread(path)

def save_image(image, path):
    cv2.imwrite(path, image)

def convert_to_grayscale(image):
    return cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)