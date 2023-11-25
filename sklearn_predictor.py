# sklearn_predictor.py

from sklearn.externals import joblib

class SklearnPredictor:
    def __init__(self, model_path):
        self.model = joblib.load(model_path)

    def predict(self, input_data):
        return self.model.predict(input_data)