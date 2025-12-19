"""
Prediction Service - Orchestration service for predictions
"""
from typing import Dict, Any
import logging

from api.strategies.demand.registry import demand_registry
from api.strategies.price.registry import price_registry
from api.services.preprocessing_service import preprocessing_service

logger = logging.getLogger(__name__)


class PredictionService:
    """Orchestration service for making predictions"""
    
    def predict_demand(
        self,
        week: str,
        store_id: int,
        sku_id: int,
        base_price: float,
        total_price: float | None,
        is_featured_sku: int,
        is_display_sku: int,
        strategy_name: str = "lightgbm"
    ) -> Dict[str, Any]:
        """Predict units_sold for Demand Forecasting
        
        Args:
            week: Week in DD/MM/YY format
            store_id: Store ID
            sku_id: SKU ID
            base_price: Base price
            total_price: Total price (optional)
            is_featured_sku: Featured flag (0 or 1)
            is_display_sku: Display flag (0 or 1)
            strategy_name: Strategy to use (default: "lightgbm")
            
        Returns:
            Dictionary with prediction result
        """
        try:
            # Get strategy
            strategy = demand_registry.get(strategy_name)
            
            # Preprocess features
            features = preprocessing_service.preprocess_demand_features(
                week=week,
                store_id=store_id,
                sku_id=sku_id,
                base_price=base_price,
                total_price=total_price,
                is_featured_sku=is_featured_sku,
                is_display_sku=is_display_sku
            )
            
            # Make prediction
            predicted_units_sold = strategy.predict(features)
            
            return {
                "predicted_units_sold": float(predicted_units_sold),
                "record_id": None,
                "strategy_used": strategy_name,
                "status": "success"
            }
        except Exception as e:
            logger.error(f"Error in demand prediction: {e}", exc_info=True)
            raise
    
    def predict_price(
        self,
        store: int,
        dept: int,
        date: str,
        is_holiday: bool | None = None,
        strategy_name: str | None = None
    ) -> Dict[str, Any]:
        """Predict Weekly_Sales for Price Forecasting
        
        Args:
            store: Store ID
            dept: Department ID
            date: Date in YYYY-MM-DD format
            is_holiday: Holiday flag (optional)
            strategy_name: Strategy to use (None for default)
            
        Returns:
            Dictionary with prediction result
        """
        try:
            # Get strategy (use default if not specified)
            strategy = price_registry.get(strategy_name)
            
            # Preprocess features
            features_df = preprocessing_service.preprocess_price_features(
                store_id=store,
                dept_id=dept,
                date=date,
                is_holiday=is_holiday
            )
            
            # Make prediction
            predicted_sales = strategy.predict(features_df)
            
            return {
                "predicted_weekly_sales": float(predicted_sales),
                "store": store,
                "dept": dept,
                "date": date,
                "strategy_used": strategy.get_name(),
                "status": "success"
            }
        except Exception as e:
            logger.error(f"Error in price prediction: {e}", exc_info=True)
            raise


# Global instance
prediction_service = PredictionService()

