"""
Handler for Demand Forecasting API endpoints
"""
from fastapi import APIRouter, HTTPException
import logging

from api.models.demand_request import DemandForecastRequest
from api.models.demand_response import DemandForecastResponse
from api.services.prediction_service import PredictionService
from api.strategies.demand.registry import demand_registry

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/demand-forecast", tags=["Demand Forecasting"])


@router.post("/predict", response_model=DemandForecastResponse)
async def predict_demand(request: DemandForecastRequest):
    """
    Predict units sold for given demand forecasting inputs
    
    - **week**: Week in DD/MM/YY format (e.g., "17/01/11")
    - **store_id**: Store ID
    - **sku_id**: SKU ID
    - **base_price**: Base price (required)
    - **total_price**: Total price (optional, will use base_price if missing)
    - **is_featured_sku**: Is featured SKU (0 or 1)
    - **is_display_sku**: Is display SKU (0 or 1)
    - **strategy**: Strategy to use (default: "lightgbm")
    """
    try:
        # Validate strategy exists
        strategy_name = request.strategy or "lightgbm"
        if strategy_name not in demand_registry.list_all():
            raise HTTPException(
                status_code=400,
                detail=f"Strategy '{strategy_name}' not found. Available: {demand_registry.list_all()}"
            )
        
        # Get prediction service
        service = PredictionService()
        
        # Make prediction
        result = service.predict_demand(
            week=request.week,
            store_id=request.store_id,
            sku_id=request.sku_id,
            base_price=request.base_price,
            total_price=request.total_price,
            is_featured_sku=request.is_featured_sku,
            is_display_sku=request.is_display_sku,
            strategy_name=strategy_name
        )
        
        return DemandForecastResponse(
            predicted_units_sold=result["predicted_units_sold"],
            record_id=result.get("record_id"),
            strategy_used=result["strategy_used"],
            status="success"
        )
        
    except HTTPException:
        raise
    except ValueError as e:
        logger.error(f"Validation error in demand forecast: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Error in demand forecast prediction: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

