"""
Handler for Price Forecasting API endpoints
"""
from fastapi import APIRouter, HTTPException
import logging

from api.models.price_request import PriceForecastRequest
from api.models.price_response import PriceForecastResponse
from api.services.prediction_service import PredictionService
from api.strategies.price.registry import price_registry

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/price-forecast", tags=["Price Forecasting"])


@router.post("/predict", response_model=PriceForecastResponse)
async def predict_price(request: PriceForecastRequest):
    """
    Predict weekly sales for given price forecasting inputs
    
    - **Store**: Store ID (1-45)
    - **Dept**: Department ID (1-99)
    - **Date**: Date in YYYY-MM-DD format
    - **IsHoliday**: Is holiday week (optional, will be looked up if missing)
    - **strategy**: Strategy to use (default: "linear", options: "linear", "dnn")
    """
    try:
        # Validate strategy exists
        strategy_name = request.strategy or "linear"
        if strategy_name not in price_registry.list_all():
            raise HTTPException(
                status_code=400,
                detail=f"Strategy '{strategy_name}' not found. Available: {price_registry.list_all()}"
            )
        
        # Get prediction service
        service = PredictionService()
        
        # Make prediction
        result = service.predict_price(
            store=request.Store,
            dept=request.Dept,
            date=request.Date,
            is_holiday=request.IsHoliday,
            strategy_name=strategy_name
        )
        
        return PriceForecastResponse(
            predicted_weekly_sales=result["predicted_weekly_sales"],
            store=result["store"],
            dept=result["dept"],
            date=result["date"],
            strategy_used=result["strategy_used"],
            status="success"
        )
        
    except HTTPException:
        raise
    except ValueError as e:
        logger.error(f"Validation error in price forecast: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Error in price forecast prediction: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

