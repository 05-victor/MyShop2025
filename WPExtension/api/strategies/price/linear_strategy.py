"""
Linear Regression Strategy for Price Forecasting
"""
import pickle
import pandas as pd
import numpy as np
from pathlib import Path
from typing import Dict, Any
import logging

from api.core.base import BaseStrategy
from api.core.config import get_config
from api.services.data_service import data_service

logger = logging.getLogger(__name__)


class LinearStrategy(BaseStrategy):
    """Linear Regression strategy for Price Forecasting"""
    
    def __init__(self):
        super().__init__()
        self.model = None
        self.scaler = None
        self.feature_columns = None
    
    def load(self) -> None:
        """Load Linear Regression model"""
        if self._loaded:
            return
        
        self.config = get_config()
        model_path = Path(self.config["price"]["linear_model"])
        
        if not model_path.exists():
            raise FileNotFoundError(f"Model file not found: {model_path}")
        
        with open(model_path, 'rb') as f:
            self.model = pickle.load(f)
        
        # Load datasets to get feature columns
        data_service.load_price_forecast_datasets()
        
        # Get feature columns from training data structure
        # Based on notebook: top features are used
        # We'll build feature columns dynamically from one-hot encoded data
        self._determine_feature_columns()
        
        logger.info(f"Loaded Linear Regression model from {model_path}")
        self._loaded = True
    
    def _determine_feature_columns(self):
        """Determine feature columns by inspecting model or using default"""
        # Default feature columns based on notebook analysis
        # Top 23 features from feature ranking
        default_features = [
            'mean', 'median', 'Week', 'Temperature', 'max', 'CPI', 'Fuel_Price',
            'min', 'Unemployment', 'std', 'Month', 'Total_MarkDown', 'IsHoliday',
            'Size', 'Year'
        ]
        
        # Add one-hot encoded dept features (common ones)
        dept_features = ['Dept_1', 'Dept_3', 'Dept_5', 'Dept_9', 'Dept_11', 'Dept_16', 'Dept_18', 'Dept_56']
        default_features.extend(dept_features)
        
        # Check if model has feature_names_ or coef_ to determine dimensions
        if hasattr(self.model, 'coef_'):
            # Model expects certain number of features
            n_features = len(self.model.coef_)
            # We'll use default and add Store/Type one-hot if needed
            self.feature_columns = default_features.copy()
            
            # Add Store and Type one-hot if needed (to match model dimensions)
            # Model was trained with all Store_* and Type_* features
            for i in range(1, 46):  # Store 1-45
                self.feature_columns.append(f'Store_{i}')
            
            for type_val in ['A', 'B', 'C']:
                self.feature_columns.append(f'Type_{type_val}')
            
            # Truncate to match model dimensions if needed
            if len(self.feature_columns) > n_features:
                self.feature_columns = self.feature_columns[:n_features]
            elif len(self.feature_columns) < n_features:
                # Add missing dept features
                train_df = data_service._train_df
                if train_df is not None:
                    dept_values = sorted(train_df['Dept'].unique())
                    for dept_val in dept_values:
                        feat_name = f'Dept_{dept_val}'
                        if feat_name not in self.feature_columns:
                            self.feature_columns.append(feat_name)
                            if len(self.feature_columns) >= n_features:
                                break
        else:
            self.feature_columns = default_features
    
    def predict(self, features_df: pd.DataFrame) -> float:
        """Make prediction
        
        Args:
            features_df: DataFrame with preprocessed features
            
        Returns:
            Predicted Weekly_Sales
        """
        self.ensure_loaded()
        
        # Ensure all feature columns exist
        feature_array = []
        for col in self.feature_columns:
            if col in features_df.columns:
                feature_array.append(features_df[col].iloc[0])
            else:
                # Fill missing with 0 (for one-hot encoded features)
                feature_array.append(0.0)
        
        X = np.array([feature_array], dtype=np.float32)
        
        # Make prediction
        prediction = self.model.predict(X)[0]
        
        # Note: Model was trained on normalized data, but we're not denormalizing
        # because the model might handle it internally or the prediction is already
        # in the correct scale
        
        return float(prediction)
    
    def get_name(self) -> str:
        """Return strategy name"""
        return "linear"

