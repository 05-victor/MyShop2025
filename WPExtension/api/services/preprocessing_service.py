"""
Preprocessing Service - Feature engineering and preprocessing
"""
import pandas as pd
import numpy as np
from datetime import datetime, timedelta
from typing import Dict, Any, Optional
import logging
import joblib

from api.core.config import get_config

logger = logging.getLogger(__name__)


class PreprocessingService:
    """Service for preprocessing features"""
    
    _instance = None
    _initialized = False
    _demand_config = None
    _demand_encoders = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(PreprocessingService, cls).__new__(cls)
        return cls._instance
    
    def __init__(self):
        if not self._initialized:
            self.config = get_config()
            self._initialized = True
    
    def _load_demand_config_and_encoders(self):
        """Lazy load demand config and encoders"""
        if self._demand_config is not None and self._demand_encoders is not None:
            return
        
        from pathlib import Path
        
        # Load config
        config_path = Path(self.config["demand"]["config_path"])
        import json
        with open(config_path, 'r') as f:
            self._demand_config = json.load(f)
        
        # Load encoders
        encoder_path = Path(self.config["demand"]["encoder_dir"]) / "encoding_dicts.pkl"
        encoding_data = joblib.load(encoder_path)
        self._demand_encoders = {
            'store_encoding_dict': encoding_data['store_encoding_dict'],
            'sku_encoding_dict': encoding_data['sku_encoding_dict'],
            'time_encoding_dicts': encoding_data.get('time_encoding_dicts', {}),
            'global_mean': encoding_data['global_mean'],
            'm_store': encoding_data.get('m_store', 10),
            'm_sku': encoding_data.get('m_sku', 10),
            'm_time': encoding_data.get('m_time', 5)
        }
    
    def preprocess_demand_features(
        self,
        week: str,
        store_id: int,
        sku_id: int,
        base_price: float,
        total_price: Optional[float],
        is_featured_sku: int,
        is_display_sku: int
    ) -> Dict[str, Any]:
        """Preprocess features for Demand Forecasting
        
        Args:
            week: Week in DD/MM/YY format
            store_id: Store ID
            sku_id: SKU ID
            base_price: Base price
            total_price: Total price (optional, uses base_price if None)
            is_featured_sku: Featured flag (0 or 1)
            is_display_sku: Display flag (0 or 1)
            
        Returns:
            Dictionary of processed features
        """
        # Load config and encoders if needed
        self._load_demand_config_and_encoders()
        
        # Handle missing total_price
        if total_price is None:
            total_price = base_price
        
        # Calculate price differences
        diff = base_price - total_price
        relative_diff_base = diff / base_price if base_price != 0 else 0.0
        relative_diff_total = diff / total_price if total_price != 0 else 0.0
        
        # Parse week date
        week_date = datetime.strptime(week, '%d/%m/%y')
        
        # Extract datetime features
        features = self._extract_datetime_features(week_date)
        
        # Encode store_id and sku_id
        store_encoded = self._encode_store(store_id)
        sku_encoded = self._encode_sku(sku_id)
        
        # Encode time features
        time_encoded = self._encode_time_features(features)
        
        # Build feature dict in order specified in config
        feature_dict = {
            'base_price': base_price,
            'total_price': total_price,
            'diff': diff,
            'relative_diff_base': relative_diff_base,
            'relative_diff_total': relative_diff_total,
            'is_featured_sku': is_featured_sku,
            'is_display_sku': is_display_sku,
            'store_encoded': store_encoded,
            'sku_encoded': sku_encoded,
            'store_id': store_id,
            'sku_id': sku_id,
            **time_encoded
        }
        
        return feature_dict
    
    def _extract_datetime_features(self, week_date: datetime) -> Dict[str, Any]:
        """Extract datetime features from week start date"""
        base_date = datetime.strptime(self._demand_config['base_date'], '%Y-%m-%d')
        
        # Weekend date (end of week)
        weekend_date = week_date + timedelta(days=6)
        
        # Calculate week serial (number of weeks from base_date)
        week_serial = (week_date - base_date).total_seconds() / (86400 * 7)
        end_week_serial = (weekend_date - base_date).total_seconds() / (86400 * 7)
        
        features = {
            'year': week_date.year,
            'date': week_date.day,
            'month': week_date.month,
            'weekday': week_date.weekday(),
            'weeknum': week_date.isocalendar().week,
            'week_serial': week_serial,
            'end_year': weekend_date.year,
            'end_date': weekend_date.day,
            'end_month': weekend_date.month,
            'end_weekday': weekend_date.weekday(),
            'end_weeknum': weekend_date.isocalendar().week,
            'end_week_serial': end_week_serial
        }
        
        return features
    
    def _encode_store(self, store_id: int) -> float:
        """Encode store_id using encoding dictionary"""
        store_dict = self._demand_encoders['store_encoding_dict']
        # Convert to same type as keys in dict
        store_id_key = str(store_id) if str(store_id) in store_dict else store_id
        
        if store_id_key in store_dict:
            return float(store_dict[store_id_key])
        
        # Fallback to global mean
        return float(self._demand_encoders['global_mean'])
    
    def _encode_sku(self, sku_id: int) -> float:
        """Encode sku_id using encoding dictionary"""
        sku_dict = self._demand_encoders['sku_encoding_dict']
        sku_id_key = str(sku_id) if str(sku_id) in sku_dict else sku_id
        
        if sku_id_key in sku_dict:
            return float(sku_dict[sku_id_key])
        
        # Fallback to global mean
        return float(self._demand_encoders['global_mean'])
    
    def _encode_time_features(self, features: Dict[str, Any]) -> Dict[str, float]:
        """Encode time features using encoding dictionaries"""
        time_dicts = self._demand_encoders.get('time_encoding_dicts', {})
        encoded = {}
        
        time_feature_names = self._demand_config.get('time_features', [])
        
        for feat_name in time_feature_names:
            if feat_name in features:
                value = features[feat_name]
                if feat_name in time_dicts:
                    # Lookup in encoding dict
                    feat_dict = time_dicts[feat_name]
                    value_key = str(value) if str(value) in feat_dict else value
                    if value_key in feat_dict:
                        encoded[feat_name] = float(feat_dict[value_key])
                    else:
                        encoded[feat_name] = float(self._demand_encoders['global_mean'])
                else:
                    # Use raw value if no encoding dict
                    encoded[feat_name] = float(value)
        
        return encoded
    
    def preprocess_price_features(
        self,
        store_id: int,
        dept_id: int,
        date: str,
        is_holiday: Optional[bool],
        features_dict: Optional[Dict[str, Any]] = None
    ) -> pd.DataFrame:
        """Preprocess features for Price Forecasting
        
        Args:
            store_id: Store ID
            dept_id: Department ID
            date: Date in YYYY-MM-DD format
            is_holiday: Holiday flag (optional, will be filled from features if None)
            features_dict: Optional pre-fetched features dict
            
        Returns:
            DataFrame with preprocessed features ready for model
        """
        from api.services.data_service import data_service
        
        # Get store info
        store_info = data_service.get_store_info(store_id)
        
        # Get features (from nearest date if needed)
        if features_dict is None:
            features_dict = data_service.get_nearest_date_features(store_id, date)
        
        # Use IsHoliday from features if not provided
        if is_holiday is None:
            is_holiday = features_dict.get('IsHoliday', False)
        
        # Get store-dept stats
        stats = data_service.get_store_dept_stats(store_id, dept_id)
        
        # Parse date
        date_obj = pd.to_datetime(date)
        year = date_obj.year
        month = date_obj.month
        week = date_obj.isocalendar().week
        
        # Calculate Total_MarkDown
        total_markdown = (
            features_dict.get('MarkDown1', 0) +
            features_dict.get('MarkDown2', 0) +
            features_dict.get('MarkDown3', 0) +
            features_dict.get('MarkDown4', 0) +
            features_dict.get('MarkDown5', 0)
        )
        
        # Create base dataframe
        data = pd.DataFrame({
            'Store': [store_id],
            'Dept': [dept_id],
            'Date': [date],
            'IsHoliday': [int(is_holiday)],
            'Year': [year],
            'Month': [month],
            'Week': [week],
            'Type': [store_info['Type']],
            'Size': [store_info['Size']],
            'Temperature': [features_dict.get('Temperature', 0)],
            'Fuel_Price': [features_dict.get('Fuel_Price', 0)],
            'CPI': [features_dict.get('CPI', 0)],
            'Unemployment': [features_dict.get('Unemployment', 0)],
            'Total_MarkDown': [total_markdown],
            'max': [stats['max']],
            'min': [stats['min']],
            'mean': [stats['mean']],
            'median': [stats['median']],
            'std': [stats['std']]
        })
        
        # One-hot encode Store, Dept, Type
        data = self._one_hot_encode_price_features(data)
        
        # Normalize features (if needed - model dependent)
        # This would be done by the strategy if needed
        
        return data
    
    def _one_hot_encode_price_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """One-hot encode categorical features for price forecasting"""
        # Get all possible values from original dataset
        from api.services.data_service import data_service
        data_service.load_price_forecast_datasets()
        
        # One-hot encode Store (1-45)
        for store_num in range(1, 46):
            df[f'Store_{store_num}'] = (df['Store'] == store_num).astype(int)
        
        # One-hot encode Dept (common ones from 1-99)
        # We'll encode all that exist in training data
        train_df = data_service._train_df
        if train_df is not None:
            dept_values = sorted(train_df['Dept'].unique())
            for dept_val in dept_values:
                df[f'Dept_{dept_val}'] = (df['Dept'] == dept_val).astype(int)
        
        # One-hot encode Type (A, B, C)
        for type_val in ['A', 'B', 'C']:
            df[f'Type_{type_val}'] = (df['Type'] == type_val).astype(int)
        
        return df


# Global instance
preprocessing_service = PreprocessingService()

