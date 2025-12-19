"""
LightGBM Strategy for Demand Forecasting
"""
import json
import numpy as np
import lightgbm as lgb
from pathlib import Path
from typing import Dict, Any
import logging

from api.core.base import BaseStrategy
from api.core.config import get_config

logger = logging.getLogger(__name__)


class LightGBMStrategy(BaseStrategy):
    """LightGBM strategy using ensemble of 10 models"""
    
    def __init__(self):
        super().__init__()
        self.models = []
        self.config = None
        self.config_dict = None
    
    def load(self) -> None:
        """Load 10 LightGBM models and configuration"""
        if self._loaded:
            return
        
        self.config = get_config()
        demand_config = self.config["demand"]
        
        model_dir = Path(demand_config["model_dir"])
        config_path = Path(demand_config["config_path"])
        
        # Load config
        with open(config_path, 'r') as f:
            self.config_dict = json.load(f)
        
        # Load models
        n_folds = demand_config["n_folds"]
        for i in range(n_folds):
            model_path = model_dir / f"model_fold_{i}.txt"
            if not model_path.exists():
                logger.warning(f"Model file not found: {model_path}")
                continue
            
            model = lgb.Booster(model_file=str(model_path))
            self.models.append(model)
        
        if not self.models:
            raise ValueError(f"No models loaded from {model_dir}")
        
        logger.info(f"Loaded {len(self.models)} LightGBM models")
        self._loaded = True
    
    def predict(self, features: Dict[str, Any]) -> float:
        """Make prediction using ensemble of models
        
        Args:
            features: Dictionary of feature values
            
        Returns:
            Predicted units_sold
        """
        self.ensure_loaded()
        
        # Get feature columns in correct order
        feature_columns = self.config_dict["feature_columns"]
        
        # Build feature array in correct order
        feature_array = []
        for col in feature_columns:
            if col not in features:
                raise ValueError(f"Missing feature: {col}")
            feature_array.append(features[col])
        
        X = np.array([feature_array], dtype=np.float32)
        
        # Get categorical columns
        categorical_cols = self.config_dict["categorical_columns"]
        cat_indices = [i for i, col in enumerate(feature_columns) if col in categorical_cols]
        
        # Make predictions from all models
        predictions = []
        for model in self.models:
            # Transform to log1p scale
            pred_log = model.predict(X, num_iteration=model.best_iteration)[0]
            # Transform back from log1p
            pred = np.exp(pred_log) - 1.0
            predictions.append(pred)
        
        # Ensemble: average of all models
        predicted_units_sold = np.mean(predictions)
        
        return float(predicted_units_sold)
    
    def get_name(self) -> str:
        """Return strategy name"""
        return "lightgbm"

