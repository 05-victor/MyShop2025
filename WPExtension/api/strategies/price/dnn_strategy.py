"""
DNN (Deep Neural Network) Strategy for Price Forecasting
"""
import importlib
import json
import pandas as pd
import numpy as np
from pathlib import Path
from typing import Dict, Any
import logging

from api.core.base import BaseStrategy
from api.core.config import get_config
from api.services.data_service import data_service

logger = logging.getLogger(__name__)


class DNNStrategy(BaseStrategy):
    """Deep Neural Network strategy for Price Forecasting"""
    
    def __init__(self):
        super().__init__()
        self.model = None
        self.feature_columns = None
        self._model_from_json = None
        self._keras_import_error = None
    
    def load(self) -> None:
        """Load DNN model (JSON + weights)"""
        if self._loaded:
            return

        self._ensure_keras_available()
        
        self.config = get_config()
        json_path = Path(self.config["price"]["dnn_json"])
        weights_path = Path(self.config["price"]["dnn_weights"])
        
        if not json_path.exists():
            raise FileNotFoundError(f"DNN JSON file not found: {json_path}")
        if not weights_path.exists():
            raise FileNotFoundError(f"DNN weights file not found: {weights_path}")
        
        # Load model architecture
        with open(json_path, 'r') as json_file:
            loaded_model_json = json_file.read()
        self.model = self._model_from_json(loaded_model_json)
        
        # Load weights
        self.model.load_weights(str(weights_path))
        
        # Compile model (as per notebook)
        self.model.compile(loss='mean_absolute_error', optimizer='adam')
        
        # Load datasets to determine feature structure
        data_service.load_price_forecast_datasets()
        
        # DNN expects 23 features (from notebook: input_dim=23)
        self._determine_feature_columns()
        
        logger.info(f"Loaded DNN model from {json_path} and {weights_path}")
        self._loaded = True
    
    def _determine_feature_columns(self):
        """Determine feature columns for DNN (23 features)"""
        # Based on notebook: model expects 23 input features
        # Top features from feature ranking + one-hot encoded
        self.feature_columns = [
            'mean', 'median', 'Week', 'Temperature', 'max', 'CPI', 'Fuel_Price',
            'min', 'Unemployment', 'std', 'Month', 'Total_MarkDown', 'IsHoliday',
            'Size', 'Year', 'Dept_1', 'Dept_3', 'Dept_5', 'Dept_9', 'Dept_11',
            'Dept_16', 'Dept_18', 'Dept_56'
        ]
        
        # Ensure we have exactly 23 features
        # Add more dept/store/type features if needed
        if len(self.feature_columns) < 23:
            train_df = data_service._train_df
            if train_df is not None:
                dept_values = sorted(train_df['Dept'].unique())[:10]
                for dept_val in dept_values:
                    feat_name = f'Dept_{dept_val}'
                    if feat_name not in self.feature_columns:
                        self.feature_columns.append(feat_name)
                        if len(self.feature_columns) >= 23:
                            break
        
        # Truncate to 23 if more
        self.feature_columns = self.feature_columns[:23]
    
    def predict(self, features_df: pd.DataFrame) -> float:
        """Make prediction using DNN
        
        Args:
            features_df: DataFrame with preprocessed features
            
        Returns:
            Predicted Weekly_Sales
        """
        self.ensure_loaded()
        
        # Prepare feature array
        feature_array = []
        for col in self.feature_columns:
            if col in features_df.columns:
                feature_array.append(float(features_df[col].iloc[0]))
            else:
                # Fill missing with 0 (for one-hot encoded features)
                feature_array.append(0.0)
        
        X = np.array([feature_array], dtype=np.float32)
        
        # Make prediction
        prediction = self.model.predict(X, verbose=0)[0][0]
        
        return float(prediction)
    
    def get_name(self) -> str:
        """Return strategy name"""
        return "dnn"

    def _ensure_keras_available(self) -> None:
        """Lazily import Keras/TensorFlow only when needed."""
        if self._model_from_json is not None:
            return

        if self._keras_import_error:
            raise RuntimeError(
                "TensorFlow/Keras previously failed to import; DNN strategy is unavailable."
            ) from self._keras_import_error

        module_candidates = (
            "keras.models",
            "tensorflow.keras.models",
        )
        last_exc = None

        for module_name in module_candidates:
            try:
                module = importlib.import_module(module_name)
                self._model_from_json = module.model_from_json
                return
            except Exception as exc:
                last_exc = exc
                logger.warning(
                    "Unable to import %s for DNN strategy: %s",
                    module_name,
                    exc,
                )

        self._keras_import_error = last_exc
        raise RuntimeError(
            "TensorFlow/Keras could not be imported. Install the correct GPU/CPU build "
            "or disable the DNN strategy."
        ) from last_exc

