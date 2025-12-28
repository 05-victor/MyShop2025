"""
Data Service - Load datasets and find nearest date features
"""
import pandas as pd
from pathlib import Path
from typing import Dict, Optional, Any
from datetime import datetime
import logging

from api.core.config import get_config

logger = logging.getLogger(__name__)


class DataService:
    """Singleton service for loading and caching datasets"""
    
    _instance = None
    _initialized = False
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(DataService, cls).__new__(cls)
        return cls._instance
    
    def __init__(self):
        if not self._initialized:
            self.config = get_config()
            self._train_df: Optional[pd.DataFrame] = None
            self._stores_df: Optional[pd.DataFrame] = None
            self._features_df: Optional[pd.DataFrame] = None
            self._initialized = True
    
    def load_price_forecast_datasets(self) -> None:
        """Load and cache train.csv, stores.csv, features.csv"""
        if self._train_df is not None:
            return  # Already loaded
        
        dataset_config = self.config["price"]["datasets"]
        
        try:
            # Load datasets
            self._train_df = pd.read_csv(dataset_config["train"])
            self._stores_df = pd.read_csv(dataset_config["stores"])
            self._features_df = pd.read_csv(dataset_config["features"])
            
            # Convert Date columns
            self._train_df['Date'] = pd.to_datetime(self._train_df['Date'])
            self._features_df['Date'] = pd.to_datetime(self._features_df['Date'])
            
            logger.info("Price forecast datasets loaded successfully")
        except Exception as e:
            logger.error(f"Error loading datasets: {e}")
            raise
    
    def get_store_info(self, store_id: int) -> Dict[str, Any]:
        """Get store information
        
        Args:
            store_id: Store ID
            
        Returns:
            Dictionary with store info (Type, Size)
        """
        if self._stores_df is None:
            self.load_price_forecast_datasets()
        
        store_info = self._stores_df[self._stores_df['Store'] == store_id]
        
        if store_info.empty:
            raise ValueError(f"Store {store_id} not found")
        
        return {
            'Store': store_id,
            'Type': store_info.iloc[0]['Type'],
            'Size': int(store_info.iloc[0]['Size'])
        }
    
    def get_nearest_date_features(self, store_id: int, date: str) -> Dict[str, Any]:
        """Find features from nearest date in features.csv
        
        Args:
            store_id: Store ID
            date: Date string in YYYY-MM-DD format
            
        Returns:
            Dictionary with features (Temperature, Fuel_Price, CPI, etc.)
        """
        if self._features_df is None:
            self.load_price_forecast_datasets()
        
        target_date = pd.to_datetime(date)
        
        # Filter by store
        store_features = self._features_df[self._features_df['Store'] == store_id].copy()
        
        if store_features.empty:
            raise ValueError(f"No features found for store {store_id}")
        
        # Find exact match first
        exact_match = store_features[store_features['Date'] == target_date]
        
        if not exact_match.empty:
            return self._extract_features(exact_match.iloc[0])
        
        # Find nearest date
        store_features['date_diff'] = (store_features['Date'] - target_date).abs()
        nearest_row = store_features.loc[store_features['date_diff'].idxmin()]
        
        logger.info(f"Using features from nearest date: {nearest_row['Date']} for requested date: {date}")
        
        return self._extract_features(nearest_row)
    
    def _extract_features(self, row: pd.Series) -> Dict[str, Any]:
        """Extract feature values from a row"""
        features = {
            'Temperature': float(row['Temperature']) if pd.notna(row['Temperature']) else None,
            'Fuel_Price': float(row['Fuel_Price']) if pd.notna(row['Fuel_Price']) else None,
            'CPI': float(row['CPI']) if pd.notna(row['CPI']) else None,
            'Unemployment': float(row['Unemployment']) if pd.notna(row['Unemployment']) else None,
            'MarkDown1': float(row['MarkDown1']) if pd.notna(row['MarkDown1']) else 0.0,
            'MarkDown2': float(row['MarkDown2']) if pd.notna(row['MarkDown2']) else 0.0,
            'MarkDown3': float(row['MarkDown3']) if pd.notna(row['MarkDown3']) else 0.0,
            'MarkDown4': float(row['MarkDown4']) if pd.notna(row['MarkDown4']) else 0.0,
            'MarkDown5': float(row['MarkDown5']) if pd.notna(row['MarkDown5']) else 0.0,
            'IsHoliday': bool(row['IsHoliday']) if pd.notna(row['IsHoliday']) else False,
            'Date': str(row['Date'].date())
        }
        
        # Fill missing CPI/Unemployment with median if needed
        if features['CPI'] is None and self._features_df is not None:
            features['CPI'] = float(self._features_df['CPI'].median())
        if features['Unemployment'] is None and self._features_df is not None:
            features['Unemployment'] = float(self._features_df['Unemployment'].median())
        
        return features
    
    def get_store_dept_stats(self, store_id: int, dept_id: int) -> Dict[str, float]:
        """Calculate statistics for store-dept combination from historical data
        
        Args:
            store_id: Store ID
            dept_id: Department ID
            
        Returns:
            Dictionary with stats (max, min, mean, median, std)
        """
        if self._train_df is None:
            self.load_price_forecast_datasets()
        
        # Filter by store and dept
        filtered = self._train_df[
            (self._train_df['Store'] == store_id) & 
            (self._train_df['Dept'] == dept_id)
        ]
        
        if filtered.empty:
            # Return default stats if no historical data
            return {
                'max': 0.0,
                'min': 0.0,
                'mean': 0.0,
                'median': 0.0,
                'std': 0.0
            }
        
        sales = filtered['Weekly_Sales']
        return {
            'max': float(sales.max()),
            'min': float(sales.min()),
            'mean': float(sales.mean()),
            'median': float(sales.median()),
            'std': float(sales.std()) if len(sales) > 1 else 0.0
        }


# Global instance
data_service = DataService()

